using System.Windows.Threading;
using Nice3point.TUnit.Revit.Limiters;
using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Executors;

/// <summary>
///     Represents the executor that runs tests and hooks on the thread that owns the Revit API.
/// </summary>
/// <remarks>
///     Revit requires every API call to occur on the thread that initialized it.
///     The executor queues every action onto one process-wide STA thread, and <c>await</c> continuations return to the same thread.
///     The executor runs one Revit test at a time.
/// </remarks>
public sealed class RevitThreadExecutor : GenericAbstractExecutor, ITestRegisteredEventReceiver
{
    /// <inheritdoc />
    /// <remarks>
    ///     The executor applies the Revit parallel limit to the registered test.
    /// </remarks>
    public ValueTask OnTestRegistered(TestRegisteredContext context)
    {
        context.SetParallelLimiter(RevitParallelLimit.Default);
        return default;
    }

    /// <inheritdoc />
    protected override ValueTask ExecuteAsync(Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return RevitDispatcherThread.Instance.InvokeAsync(action);
    }

    /// <summary>
    ///     Runs the specified action on the Revit thread outside a test and a hook.
    /// </summary>
    /// <param name="action">The action to run on the Revit thread.</param>
    /// <returns>A task that completes once the action and its <c>await</c> continuations finish.</returns>
    internal static ValueTask InvokeAsync(Func<ValueTask> action)
    {
        return RevitDispatcherThread.Instance.InvokeAsync(action);
    }
}

/// <summary>
///     Represents the process-wide STA thread that runs every Revit API call.
/// </summary>
/// <remarks>
///     A WPF <see cref="Dispatcher" /> drives the thread.
///     It pumps the Win32 messages COM marshaling needs and routes <c>await</c> continuations back to the thread through <see cref="DispatcherSynchronizationContext" />.
/// </remarks>
file sealed class RevitDispatcherThread
{
    private readonly Dispatcher _dispatcher;

    private RevitDispatcherThread()
    {
        using var readyEvent = new ManualResetEventSlim(false);
        Dispatcher? dispatcher = null;

        var thread = new Thread(() =>
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            // ReSharper disable once AccessToDisposedClosure
            readyEvent.Set();
            Dispatcher.Run();
        })
        {
            IsBackground = true,
            Name = "Revit API Thread",
            Priority = ThreadPriority.Normal
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        readyEvent.Wait();
        _dispatcher = dispatcher!;
    }

    /// <summary>
    ///     Gets the process-wide instance of the Revit thread.
    /// </summary>
    public static RevitDispatcherThread Instance { get; } = new();

    /// <summary>
    ///     Queues the specified action on the Revit thread.
    /// </summary>
    /// <param name="action">The action to run on the Revit thread.</param>
    /// <returns>A task that completes once the action and its <c>await</c> continuations finish.</returns>
    public ValueTask InvokeAsync(Func<ValueTask> action)
    {
        var operation = _dispatcher.InvokeAsync(() => action().AsTask(), DispatcherPriority.Normal);
        return new ValueTask(operation.Task.Unwrap());
    }
}
