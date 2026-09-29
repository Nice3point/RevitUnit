using System.Diagnostics;
using System.Xml.Linq;
using Nice3point.TUnit.Revit.Tests.Attributes;

namespace Nice3point.TUnit.Revit.Tests;

[NotInParallel]
public sealed class SessionCompletionTests
{
    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task Stop_AfterPassedUiTest_ReportsSessionOutcome(bool failCleanup, CancellationToken cancellationToken)
    {
        // Act
        var result = await StartTestSession(failCleanup ? "fail" : "pass", cancellationToken);

        // Assert
        await Assert.That(result.Outcome).IsEqualTo("Passed");
        if (failCleanup)
        {
            await Assert.That(result.ExitCode).IsNotEqualTo(0);
            await Assert.That(result.Output).Contains("The Revit test run exited with code");
        }
        else
        {
            await Assert.That(result.ExitCode).IsEqualTo(0);
        }
    }

    [Test]
    public async Task Timeout_BodyExceedsItsLimit_ReportsTimeout(CancellationToken cancellationToken)
    {
        // Act
        var result = await StartTestSession("timeout", cancellationToken);

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(result.ExitCode).IsEqualTo(2);
            await Assert.That(result.Outcome).IsEqualTo("Failed");
            await Assert.That(result.FailureMessage).Contains("TimeoutException:");
            await Assert.That(result.FailureMessage).Contains("00:00:00.5000000");
            await Assert.That(result.Output).DoesNotContain("-1ms");
        }
    }

    private static async Task<(int ExitCode, string Outcome, string? FailureMessage, string Output)> StartTestSession(string mode, CancellationToken cancellationToken)
    {
        var resultsDirectory = Path.Combine(Path.GetTempPath(), $"RevitSessionCompletion.{Guid.NewGuid():N}");
        Directory.CreateDirectory(resultsDirectory);

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.ChangeExtension(typeof(SessionCompletionTests).Assembly.Location, ".exe"),
            Arguments = $"--treenode-filter /*/*/{nameof(SessionCompletionUiTests)}/* --report-trx --report-trx-filename session.trx --results-directory \"{resultsDirectory}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        foreach (var variable in startInfo.EnvironmentVariables.Keys.Cast<string>().ToArray())
        {
            if (variable.StartsWith("TESTINGPLATFORM_", StringComparison.OrdinalIgnoreCase)
                || variable.StartsWith("DOTNET_TEST", StringComparison.OrdinalIgnoreCase)
                || variable.StartsWith("VSTEST_", StringComparison.OrdinalIgnoreCase))
            {
                startInfo.EnvironmentVariables.Remove(variable);
            }
        }

        startInfo.EnvironmentVariables[SessionCompletionProbeAttribute.Variable] = mode;
        startInfo.EnvironmentVariables["TUNIT_DISABLE_HTML_REPORTER"] = "true";
        startInfo.EnvironmentVariables["TUNIT_DISABLE_GITHUB_REPORTER"] = "true";
        startInfo.EnvironmentVariables["TUNIT_DISABLE_JUNIT_REPORTER"] = "true";

        using var process = new Process();
        process.StartInfo = startInfo;

        var hasStarted = false;
        try
        {
            process.Start();
            hasStarted = true;

            var standardOutput = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var standardError = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken).WaitAsync(TimeSpan.FromMinutes(3), cancellationToken);
            var output = $"{await standardOutput}{await standardError}";

            var report = XDocument.Load(Path.Combine(resultsDirectory, "session.trx"));
            XNamespace reportNamespace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010";

            var testResult = report.Descendants(reportNamespace + "UnitTestResult").Single(result => result.Attribute("testName")?.Value == nameof(SessionCompletionUiTests.Body_BeforeSessionCleanup_Passes));
            var failureMessage = testResult.Descendants(reportNamespace + "Message").SingleOrDefault()?.Value;

            return (process.ExitCode, testResult.Attribute("outcome")!.Value, failureMessage, output);
        }
        finally
        {
            if (hasStarted && !process.HasExited)
            {
                process.Kill(true);
                await process.WaitForExitAsync(CancellationToken.None);
            }

            Directory.Delete(resultsDirectory, true);
        }
    }
}
