using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.Testing.Platform.Builder;
using Nice3point.Revit.Injector.Ui;
using TUnit.Engine.Extensions;
#if NET
using System.Runtime.Loader;

#else
using Assembly = System.Reflection.Assembly;
#endif

namespace Nice3point.TUnit.Revit.Ui;

/// <summary>
///     Represents the entry point that runs the UI tests of the test assembly inside the Revit user interface application.
/// </summary>
/// <remarks>
///     The runner starts a nested test application restricted to the tests the test host schedules.
///     Every UI test stays pending in <see cref="RevitUiApplicationRuntime" /> until the test host executes it, and <see cref="RevitUiTestResultConsumer" /> reports its final state.
///     The nested test application writes no test report, and the test host reports every UI test.
/// </remarks>
[UsedImplicitly]
[SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
internal sealed class RevitUiTestRunner : IRevitUiEntryPoint
{
    /// <summary>
    ///     The property that names the absolute path of the test assembly.
    /// </summary>
    public const string TestAssemblyProperty = "TestAssembly";

    private static readonly string[] TestPlatformVariablePrefixes =
    [
        "TESTINGPLATFORM_",
        "DOTNET_TEST",
        "VSTEST_"
    ];

    private static readonly string[] DisabledReporterVariables =
    [
        "TUNIT_DISABLE_HTML_REPORTER",
        "TUNIT_DISABLE_GITHUB_REPORTER",
        "TUNIT_DISABLE_JUNIT_REPORTER"
    ];

    /// <inheritdoc />
    /// <exception cref="System.InvalidOperationException">The nested test application exited with a code other than a completed run.</exception>
    public async Task RunAsync(RevitUiContext context)
    {
        var runtime = new RevitUiApplicationRuntime(context);
        RevitUiRuntime.Application = runtime;
        ClearTestPlatformVariables();
        DisableTestReporters();

        // The listener ends once the test host closes the connection.
        var listenTask = runtime.ListenAsync();

        var testIds = await runtime.WaitForScheduleAsync().ConfigureAwait(false);
        if (testIds.Count > 0)
        {
            await RunTestsAsync(runtime, context, testIds).ConfigureAwait(false);
        }

        if (listenTask.IsFaulted)
        {
            await listenTask.ConfigureAwait(false);
        }
    }

    private static async Task RunTestsAsync(RevitUiApplicationRuntime runtime, RevitUiContext context, IReadOnlyList<string> testIds)
    {
        var testAssemblyPath = context.Properties[TestAssemblyProperty];
#if NET
        // The test assembly shares the load context of the runner, and with it the TUnit assemblies the runner starts.
        var testAssembly = AssemblyLoadContext.GetLoadContext(typeof(RevitUiTestRunner).Assembly)!.LoadFromAssemblyPath(testAssemblyPath);
#else
        var testAssembly = Assembly.LoadFrom(testAssemblyPath);
#endif

        // Loading an assembly runs no module initializer, and the TUnit source generator registers the tests from one.
        RuntimeHelpers.RunModuleConstructor(testAssembly.ManifestModule.ModuleHandle);

        var exitCode = await RunTestApplicationAsync(runtime, testIds).ConfigureAwait(false);

        // Exit code 2 reports a failed test and 8 a run without tests; the consumer reports the final state of every test in both cases.
        if (exitCode is not (0 or 2 or 8))
        {
            throw new InvalidOperationException($"The Revit test run exited with code {exitCode}.");
        }
    }

    private static async Task<int> RunTestApplicationAsync(RevitUiApplicationRuntime runtime, IReadOnlyList<string> testIds)
    {
        var resultsDirectory = Path.Combine(Path.GetTempPath(), "RevitTestResults", Guid.NewGuid().ToString("N"));
        string[] arguments =
        [
            // A pending UI test holds a parallel slot, and the test the host executes needs a free one.
            "--maximum-parallel-tests", int.MaxValue.ToString(CultureInfo.InvariantCulture),
            "--results-directory", resultsDirectory,
            "--filter-uid", .. testIds
        ];

        try
        {
            var builder = await TestApplication.CreateBuilderAsync(arguments).ConfigureAwait(false);
            builder.AddTUnit();
            builder.TestHost.AddDataConsumer(_ => new RevitUiTestResultConsumer(runtime));

            using var application = await builder.BuildAsync().ConfigureAwait(false);
            return await application.RunAsync().ConfigureAwait(false);
        }
        finally
        {
            if (Directory.Exists(resultsDirectory))
            {
                Directory.Delete(resultsDirectory, true);
            }
        }
    }

    private static void ClearTestPlatformVariables()
    {
        foreach (DictionaryEntry variable in Environment.GetEnvironmentVariables())
        {
            var name = (string)variable.Key;
            if (TestPlatformVariablePrefixes.Any(prefix => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
            {
                Environment.SetEnvironmentVariable(name, null);
            }
        }
    }

    private static void DisableTestReporters()
    {
        foreach (var variable in DisabledReporterVariables)
        {
            Environment.SetEnvironmentVariable(variable, "true");
        }
    }
}
