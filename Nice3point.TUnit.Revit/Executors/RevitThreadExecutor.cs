using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Executors;

/// <summary>
/// Executes test code on the Revit thread with advanced message pumping.
/// </summary>
/// <remarks>
/// Provides functionality to execute test actions on the Revit thread,
/// which is necessary for operations that interact with the Revit API.
/// Uses a shared STA thread with custom task scheduling for all tests.
/// </remarks>
public sealed class RevitThreadExecutor : GenericAbstractExecutor, ITestRegisteredEventReceiver
{
    private static BlockingCollection<Action>? _executionQueue;
    private static readonly Lock InitializationLock = new();

    /// <summary>
    /// Executes an asynchronous action on the Revit thread dispatcher.
    /// Ensures that the action is executed within the context of Revit's single-threaded API.
    /// </summary>
    /// <param name="action">The asynchronous action to execute on the Revit thread dispatcher.</param>
    /// <returns>A <see cref="ValueTask"/> that represents the asynchronous execution operation.</returns>
    protected override async ValueTask ExecuteAsync(Func<ValueTask> action)
    {
        EnsureThreadInitialized();

        var tcs = new TaskCompletionSource<object?>();

        // Capture ExecutionContext (including AsyncLocal values like TestContext.Current)
        var executionContext = ExecutionContext.Capture();
        var state = (action, tcs);

        _executionQueue!.Add(() =>
        {
            if (executionContext != null)
            {
                ExecutionContext.Run(executionContext, static state =>
                {
                    var (action, tcs) = (ValueTuple<Func<ValueTask>, TaskCompletionSource<object?>>)state!;
                    ExecuteAsyncActionWithMessagePump(action, tcs);
                }, state);
            }
            else
            {
                ExecuteAsyncActionWithMessagePump(action, tcs);
            }
        });

        await tcs.Task;
    }

    [SuppressMessage("ReSharper", "AccessToDisposedClosure")]
    private static void EnsureThreadInitialized()
    {
        lock (InitializationLock)
        {
            if (_executionQueue is not null) return;

            _executionQueue = new BlockingCollection<Action>();
            using var threadReadyEvent = new ManualResetEventSlim(false);

            var thread = new Thread(() =>
            {
                try
                {
                    threadReadyEvent.Set();
                    foreach (var executeAction in _executionQueue.GetConsumingEnumerable())
                    {
                        executeAction();
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Revit thread error: {exception}");
                }
            });

            ConfigureThread(thread);
            thread.Start();

            threadReadyEvent.Wait();
        }
    }

    private static void ExecuteAsyncActionWithMessagePump(Func<ValueTask> action, TaskCompletionSource<object?> tcs)
    {
        try
        {
            var previousContext = SynchronizationContext.Current;
            var workAvailableEvent = new ManualResetEventSlim(false);
            var taskScheduler = new RevitThreadTaskScheduler(Thread.CurrentThread, workAvailableEvent);
            var dedicatedContext = new RevitThreadSynchronizationContext(workAvailableEvent);

            SynchronizationContext.SetSynchronizationContext(dedicatedContext);

            try
            {
                var task = Task.Factory.StartNew(static async action =>
                {
                    // Inside this task, TaskScheduler.Current will be our scheduler
                    await ((Func<ValueTask>)action!)();
                }, action, CancellationToken.None, TaskCreationOptions.None, taskScheduler).Unwrap();

                // Try fast path first - many tests complete quickly
                // Use IsCompleted to avoid synchronous wait
                if (task.IsCompleted)
                {
                    HandleTaskCompletion(task, tcs);
                    return;
                }

                // Pump messages until the task completes with event-driven signaling
                var deadline = DateTime.UtcNow.AddMinutes(5);
                var spinWait = new SpinWait();
                const int maxSpinCount = 50;
                const int waitTimeoutMs = 100;

                while (!task.IsCompleted)
                {
                    var hadWork = dedicatedContext.ProcessPendingWork();
                    hadWork |= taskScheduler.ProcessPendingTasks();

                    if (!hadWork)
                    {
                        // Fast path: spin briefly for immediate continuations
                        if (spinWait.Count < maxSpinCount)
                        {
                            spinWait.SpinOnce();
                        }
                        else
                        {
                            // No work after spinning - use event-driven wait
                            workAvailableEvent.Wait(waitTimeoutMs);
                            workAvailableEvent.Reset();
                            spinWait.Reset();

                            // Check timeout after waiting
                            if (DateTime.UtcNow >= deadline)
                            {
                                tcs.SetException(new TimeoutException("Async operation timed out after 5 minutes"));
                                return;
                            }
                        }
                    }
                    else
                    {
                        // Had work, reset spin counter
                        spinWait.Reset();
                    }
                }

                HandleTaskCompletion(task, tcs);
            }
            finally
            {
                workAvailableEvent.Dispose();
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }
        catch (Exception exception)
        {
            tcs.SetException(exception);
        }
    }

    private static void HandleTaskCompletion(Task task, TaskCompletionSource<object?> tcs)
    {
        if (task.IsFaulted)
        {
            tcs.SetException(task.Exception!.InnerExceptions.Count == 1
                ? task.Exception.InnerException!
                : task.Exception);
        }
        else if (task.IsCanceled)
        {
            tcs.SetCanceled();
        }
        else
        {
            tcs.SetResult(null);
        }
    }

    private static void ConfigureThread(Thread thread)
    {
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
    }

    /// <summary>
    ///     Set parallel limit to 1 for all Revit api tests.
    /// </summary>
    public ValueTask OnTestRegistered(TestRegisteredContext context)
    {
        context.SetParallelLimiter(new RevitCountParallelLimit());
        return default;
    }
}

file sealed class RevitCountParallelLimit : IParallelLimit
{
    public int Limit => 1;
}

file sealed class RevitThreadSynchronizationContext(ManualResetEventSlim? workAvailableEvent) : SynchronizationContext
{
    private Queue<(SendOrPostCallback callback, object? state)>? _workQueue;
    private readonly Thread _dedicatedThread = Thread.CurrentThread;
    private readonly Lock _queueLock = new();

    public override void Post(SendOrPostCallback callback, object? state)
    {
        // Always queue the work to ensure it runs on the dedicated thread
        lock (_queueLock)
        {
            _workQueue ??= new Queue<(SendOrPostCallback callback, object? state)>();
            _workQueue.Enqueue((callback, state));
        }

        // Signal that work is available (wake message pump immediately)
        workAvailableEvent?.Set();
    }

    public override void Send(SendOrPostCallback callback, object? state)
    {
        if (Thread.CurrentThread == _dedicatedThread)
        {
            // We're already on the dedicated thread, execute immediately
            callback(state);
        }
        else
        {
            // For Send, we need to block until completion
            // Use Task.Run to avoid potential deadlocks by ensuring we don't capture any synchronization context
            var tcs = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            Post(_ =>
            {
                try
                {
                    callback(state);
                    tcs.SetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }, null);

            // Use a more robust synchronous wait pattern to avoid deadlocks
            // We use Task.Run to ensure we don't capture the current SynchronizationContext
            // which is a common cause of deadlocks
            var waitTask = Task.Run(async () =>
            {
                // For .NET Standard 2.0 compatibility, use Task.Delay for timeout
                var timeoutTask = Task.Delay(TimeSpan.FromMinutes(30));
                var completedTask = await Task.WhenAny(tcs.Task, timeoutTask).ConfigureAwait(false);

                if (completedTask == timeoutTask)
                {
                    throw new TimeoutException("Synchronous operation on Revit thread timed out after 30 minutes");
                }

                // Await the actual task to get its result or exception
                await tcs.Task.ConfigureAwait(false);
            });

            // This wait is safe because it's on a Task.Run thread without SynchronizationContext
            waitTask.GetAwaiter().GetResult();
        }
    }

    public bool ProcessPendingWork()
    {
        // Only the dedicated thread should call this
        if (Thread.CurrentThread != _dedicatedThread)
        {
            return false;
        }

        var hadWork = false;
        while (true)
        {
            (SendOrPostCallback callback, object? state) workItem;

            lock (_queueLock)
            {
                if (_workQueue == null || _workQueue.Count == 0)
                {
                    break;
                }

                workItem = _workQueue.Dequeue();
                hadWork = true;
            }

            try
            {
                workItem.callback(workItem.state);
            }
            catch
            {
                // Swallow exceptions in work items to avoid crashing the message pump
                // The exception will be handled by the async machinery
            }
        }

        return hadWork;
    }

    public override SynchronizationContext CreateCopy()
    {
        return this;
    }
}

file sealed class RevitThreadTaskScheduler(Thread dedicatedThread, ManualResetEventSlim? workAvailableEvent) : TaskScheduler
{
    private readonly List<Task> _taskQueue = [];
    private readonly Lock _queueLock = new();

    protected override void QueueTask(Task task)
    {
        lock (_queueLock)
        {
            _taskQueue.Add(task);
        }

        // Signal that work is available (wake message pump immediately)
        workAvailableEvent?.Set();
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        // ALWAYS execute inline if we're on the dedicated thread
        // This is crucial for capturing continuations that would otherwise escape
        if (Thread.CurrentThread == dedicatedThread)
        {
            if (taskWasPreviouslyQueued)
            {
                lock (_queueLock)
                {
                    if (_taskQueue.Contains(task))
                    {
                        _taskQueue.Remove(task);
                    }
                }
            }

            return TryExecuteTask(task);
        }

        // If we're not on the dedicated thread, queue it to be executed later
        if (!taskWasPreviouslyQueued)
        {
            QueueTask(task);
        }

        return false;
    }

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        lock (_queueLock)
        {
            return _taskQueue.ToArray();
        }
    }

    public bool ProcessPendingTasks()
    {
        if (Thread.CurrentThread != dedicatedThread)
        {
            throw new InvalidOperationException("ProcessPendingTasks can only be called from the dedicated thread.");
        }

        var hadWork = false;
        while (true)
        {
            Task? task;

            lock (_queueLock)
            {
                if (_taskQueue.Count == 0)
                {
                    break;
                }

                task = _taskQueue[0];
                _taskQueue.RemoveAt(0);
                hadWork = true;
            }

            TryExecuteTask(task);
        }

        return hadWork;
    }

    public override int MaximumConcurrencyLevel => 1;
}