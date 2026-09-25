using Autodesk.Revit.ApplicationServices;
using Nice3point.Revit.Injector;
using Nice3point.TUnit.Revit.Executors;

namespace Nice3point.TUnit.Revit.Sessions;

/// <summary>
///     Represents the one Revit application session that serves every test of the test host process.
/// </summary>
/// <remarks>
///     One connection serves the whole test host process.
///     Revit activates once per process, and an IDE that reuses one test host process across runs starts a test session per run in that process.
/// </remarks>
internal sealed class RevitSession
{
    private readonly Lock _connectionLock = new();
    private Injector? _injector;

    /// <summary>
    ///     Gets the session of the test host process.
    /// </summary>
    public static RevitSession Instance { get; } = new();

    /// <summary>
    ///     Gets the Revit application of the open connection.
    /// </summary>
    /// <value>Defaults to <see langword="null" /> until <see cref="EnsureStarted" /> opens the connection.</value>
    public Application? Application { get; private set; }

    /// <summary>
    ///     Initializes the connection to the Revit application.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">The process terminated its connection earlier.</exception>
    /// <remarks>
    ///     The first call of the process opens the connection, and a later call has no effect.
    ///     The caller runs on the Revit thread.
    /// </remarks>
    public void EnsureStarted()
    {
        lock (_connectionLock)
        {
            if (_injector is not null)
            {
                return;
            }

            var injector = new Injector();
            Application = injector.InjectApplication();
            _injector = injector;
        }
    }

    /// <summary>
    ///     Terminates the connection to the Revit application on the Revit thread and releases its resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    /// <remarks>
    ///     A call without an open connection has no effect and does not dispatch to the Revit thread.
    ///     The connection cannot be reopened in the same process.
    /// </remarks>
    public async Task StopAsync()
    {
        lock (_connectionLock)
        {
            if (_injector is null)
            {
                return;
            }
        }

        await RevitThreadExecutor.InvokeAsync(() =>
        {
            Stop();
            return default;
        }).ConfigureAwait(false);
    }

    private void Stop()
    {
        lock (_connectionLock)
        {
            if (_injector is null)
            {
                return;
            }

            _injector.EjectApplication();
            _injector = null;
            Application = null;
        }
    }
}
