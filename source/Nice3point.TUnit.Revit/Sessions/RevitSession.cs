using System.Runtime.CompilerServices;
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
    private readonly ConditionalWeakTable<TestSessionContext, TestSessionContext> _requestingSessions = new();
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
    ///     Records that a registered test of the specified test session needs the connection.
    /// </summary>
    /// <param name="context">The context of the test session.</param>
    /// <remarks>
    ///     A discovery request registers the tests of a test session that executes none of them.
    /// </remarks>
    public void Request(TestSessionContext context)
    {
        _requestingSessions.GetValue(context, static requestingSession => requestingSession);
    }

    /// <summary>
    ///     Determines whether a registered test of the specified test session needs the connection.
    /// </summary>
    /// <param name="context">The context of the test session.</param>
    /// <returns><see langword="true" /> if a registered test of the test session needs the connection; otherwise, <see langword="false" />.</returns>
    [Pure]
    public bool IsRequested(TestSessionContext context)
    {
        return _requestingSessions.TryGetValue(context, out _);
    }

    /// <summary>
    ///     Initializes the connection to the Revit application.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">The caller runs off the Revit thread, or the process terminated its connection earlier.</exception>
    /// <remarks>
    ///     The first call of the process opens the connection, and a later call has no effect.
    /// </remarks>
    public void EnsureStarted()
    {
        if (!RevitThreadExecutor.IsRevitThread)
        {
            throw new InvalidOperationException("The Revit connection opens on the Revit thread only.");
        }

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
