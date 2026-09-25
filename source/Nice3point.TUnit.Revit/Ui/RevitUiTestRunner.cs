using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Testing.Platform.Builder;
using Nice3point.Revit.Injector.Ui;
using TUnit.Engine.Extensions;
using Assembly = System.Reflection.Assembly;

namespace Nice3point.TUnit.Revit.Ui;

/// <summary>
///     Represents the entry point that runs the UI tests of the test assembly inside the Revit user interface application.
/// </summary>
/// <remarks>
///     The runner starts a nested test application restricted to <see cref="RevitUiRuntime.Category" />.
///     Every UI test reports its result through <see cref="RevitUiContext.Send" />.
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

    /// <inheritdoc />
    /// <exception cref="System.InvalidOperationException">The nested test application ended without a result for every test.</exception>
    public async Task RunAsync(RevitUiContext context)
    {
        RevitUiRuntime.Context = context;
        ClearTestPlatformVariables();

        var testAssembly = Assembly.LoadFrom(context.Properties[TestAssemblyProperty]);

        // Assembly.LoadFrom runs no module initializer, and the TUnit source generator registers the tests from one.
        RuntimeHelpers.RunModuleConstructor(testAssembly.ManifestModule.ModuleHandle);

        var exitCode = await RunTestApplicationAsync().ConfigureAwait(false);

        // Exit code 0 means every test passed and 2 means a test failed; both report every test.
        if (exitCode is not (0 or 2))
        {
            throw new InvalidOperationException($"The Revit test run exited with code {exitCode}.");
        }
    }

    private static async Task<int> RunTestApplicationAsync()
    {
        string[] arguments =
        [
            "--treenode-filter", $"/*/*/*/*[Category={RevitUiRuntime.Category}]",
            "--results-directory", Path.Combine(Path.GetTempPath(), "RevitTestResults", Guid.NewGuid().ToString("N"))
        ];

        var builder = await TestApplication.CreateBuilderAsync(arguments).ConfigureAwait(false);
        builder.AddTUnit();

        using var application = await builder.BuildAsync().ConfigureAwait(false);
        return await application.RunAsync().ConfigureAwait(false);
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
}
