using System.ComponentModel;
using Microsoft.Testing.Platform.Extensions.TestHost;
using Nice3point.TUnit.Revit.Sessions;

namespace Nice3point.TUnit.Revit.Lifetimes;

/// <summary>
///     Represents the lifetime that releases the Revit connections when the test application finishes.
/// </summary>
/// <remarks>
///     The test platform runs the lifetime after the last test session of the process and before the runtime begins shutting down, in the console host of <c>dotnet run</c> and <c>dotnet test</c> as well as in the server host an IDE reuses across runs.
///     It is the last point at which the Revit thread still accepts work.
///     A process with an open Revit connection never terminates.
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
    public string Description => "Releases the Revit connections when the test application finishes.";

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
    ///     A connection that never opened, as after a discovery request, is skipped without dispatching to the Revit thread.
    /// </remarks>
    public async Task AfterRunAsync(int exitCode, CancellationToken cancellationToken)
    {
        try
        {
            await RevitSession.Instance.StopAsync().ConfigureAwait(false);
        }
        finally
        {
            await RevitUiSession.Instance.StopAsync().ConfigureAwait(false);
        }
    }
}
