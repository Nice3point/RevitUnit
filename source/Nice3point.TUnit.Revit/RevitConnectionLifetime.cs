using System.ComponentModel;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Nice3point.TUnit.Revit.Executors;

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Releases the Revit connection when the test application finishes.
/// </summary>
/// <remarks>
///     The test platform runs this after the last test session of the process and before the runtime begins shutting down, in the console host of <c>dotnet run</c> and <c>dotnet test</c> as well as in the server host an IDE keeps alive between runs.
///     It is the last point at which the Revit thread still accepts work.
///     A process that keeps Revit connected never terminates.
/// </remarks>
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class RevitConnectionLifetime : ITestHostApplicationLifetime
{
    /// <inheritdoc />
    public string Uid => nameof(RevitConnectionLifetime);

    /// <inheritdoc />
    public string Version => "1.0.0";

    /// <inheritdoc />
    public string DisplayName => "Revit connection lifetime";

    /// <inheritdoc />
    public string Description => "Releases the Revit connection on the Revit thread when the test application finishes.";

    /// <inheritdoc />
    public Task<bool> IsEnabledAsync()
    {
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task BeforeRunAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <remarks>
    ///     When the process holds no connection, as after a discovery request, this method returns without dispatching to the Revit thread.
    /// </remarks>
    public async Task AfterRunAsync(int exitCode, CancellationToken cancellationToken)
    {
        if (!RevitApplicationTest.IsConnected)
        {
            return;
        }

        await RevitThreadExecutor.InvokeAsync(() =>
        {
            RevitApplicationTest.ReleaseConnection();
            return default;
        }).ConfigureAwait(false);
    }
}
