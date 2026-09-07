using System.ComponentModel;
using Microsoft.Testing.Platform.Builder;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Nice3point.TUnit.Revit.Executors;

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Releases the Revit connection when the test application finishes.
/// </summary>
/// <remarks>
///     This is the one point that runs after the last test session in every host, the console host of
///     <c>dotnet run</c> and <c>dotnet test</c> as well as the server host an IDE keeps alive between runs,
///     and before the runtime starts shutting down. Ejecting later, from <c>AppDomain.ProcessExit</c>, is
///     impossible: the runtime's shutdown flag is already set, so the dispatcher on the Revit thread refuses
///     the work. Ejecting from any other thread throws, and skipping the eject stalls the process for minutes
///     in Revit's native teardown.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class RevitApplicationLifetime : ITestHostApplicationLifetime
{
    private static readonly TimeSpan EjectTimeout = TimeSpan.FromSeconds(30);

    /// <inheritdoc />
    public string Uid => nameof(RevitApplicationLifetime);

    /// <inheritdoc />
    public string Version => "1.0.0";

    /// <inheritdoc />
    public string DisplayName => "Revit application lifetime";

    /// <inheritdoc />
    public string Description => "Releases the Revit connection on the Revit thread when the test application finishes.";

    /// <inheritdoc />
    public Task<bool> IsEnabledAsync() => Task.FromResult(true);

    /// <inheritdoc />
    public Task BeforeRunAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    /// <inheritdoc />
    public async Task AfterRunAsync(int exitCode, CancellationToken cancellationToken)
    {
        if (!RevitApplicationTest.IsConnected)
        {
            return;
        }

        var eject = RevitThreadExecutor.InvokeAsync(() =>
        {
            RevitApplicationTest.ReleaseConnection();
            return default;
        }).AsTask();

        // A stuck eject must not keep the host alive: a hung shutdown locks the output folder.
        // The delay ignores the token so a cancelled run still gets its bounded eject attempt.
        var completed = await Task.WhenAny(eject, Task.Delay(EjectTimeout)).ConfigureAwait(false);
        if (completed == eject)
        {
            await eject.ConfigureAwait(false);
        }
    }
}

/// <summary>
///     Registers <see cref="RevitApplicationLifetime" /> with the Microsoft.Testing.Platform builder.
/// </summary>
/// <remarks>
///     Referenced by the <c>TestingPlatformBuilderHook</c> item the package's <c>build/Nice3point.TUnit.Revit.props</c>
///     adds to every consuming project; the platform's build step compiles a call to <see cref="AddExtensions" />
///     into the generated entry point.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class RevitTestingPlatformBuilderHook
{
    /// <summary>
    ///     Adds the Revit lifetime extension to the test application.
    /// </summary>
    public static void AddExtensions(ITestApplicationBuilder builder, string[] args)
    {
        builder.TestHost.AddTestHostApplicationLifetime(_ => new RevitApplicationLifetime());
    }
}
