using System.Windows.Threading;
using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Executors;

/// <summary>
/// Marshals test code onto a single dedicated STA thread that owns the Revit API.
/// </summary>
/// <remarks>
/// Revit requires every API call to occur on the same thread that initialised it.
/// All actions are queued to a process-wide STA thread driven by a WPF
/// <see cref="Dispatcher"/>: it pumps Win32 messages for COM marshaling and routes
/// <c>await</c> continuations back to the same thread through
/// <see cref="DispatcherSynchronizationContext"/>. Concurrent execution is capped
/// at one test at a time to keep exclusive access to the Revit thread.
/// </remarks>
public sealed class RevitThreadExecutor : GenericAbstractExecutor, ITestRegisteredEventReceiver
{
    /// <summary>
    /// Runs <paramref name="action"/> on the Revit thread and yields its completion as a task.
    /// </summary>
    protected override ValueTask ExecuteAsync(Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return RevitDispatcherThread.Instance.InvokeAsync(action);
    }

    /// <summary>
    /// Applies the Revit parallel limiter so registered tests never run concurrently.
    /// </summary>
    public ValueTask OnTestRegistered(TestRegisteredContext context)
    {
        context.SetParallelLimiter(RevitCountParallelLimit.Default);
        return default;
    }
}

/// <summary>
/// Restricts Revit API tests to a single concurrent execution.
/// </summary>
file sealed class RevitCountParallelLimit : IParallelLimit
{
    /// <summary>
    /// Shared instance used by every registered Revit test.
    /// </summary>
    public static RevitCountParallelLimit Default { get; } = new();

    /// <summary>
    /// Maximum number of Revit tests allowed to execute simultaneously.
    /// </summary>
    public int Limit => 1;
}

/// <summary>
/// Hosts the process-wide STA thread used for every Revit API call and dispatches
/// asynchronous actions onto its WPF <see cref="Dispatcher"/>.
/// </summary>
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
            Priority = ThreadPriority.Normal,
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        readyEvent.Wait();
        _dispatcher = dispatcher!;
    }

    /// <summary>
    /// Process-wide instance backed by the dedicated Revit STA thread.
    /// </summary>
    public static RevitDispatcherThread Instance { get; } = new();

    /// <summary>
    /// Queues <paramref name="action"/> on the Revit thread and returns a task that
    /// completes once the action and all of its <c>await</c> continuations finish.
    /// </summary>
    public ValueTask InvokeAsync(Func<ValueTask> action)
    {
        var operation = _dispatcher.InvokeAsync(() => action().AsTask(), DispatcherPriority.Normal);
        return new ValueTask(operation.Task.Unwrap());
    }
}