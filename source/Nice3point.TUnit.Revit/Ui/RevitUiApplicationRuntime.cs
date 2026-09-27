using Nice3point.Revit.Injector.Ui;
using Nice3point.TUnit.Revit.Ui.Messages;
using TUnit.Core.Exceptions;

namespace Nice3point.TUnit.Revit.Ui;

/// <summary>
///     Represents the runtime of the UI tests inside the Revit user interface application.
/// </summary>
/// <param name="context">The context of the Revit user interface application.</param>
/// <remarks>
///     The test host schedules the tests of the run, and every scheduled test stays pending until the test host executes it.
///     The test host executes the pending tests one at a time, in its own order, and the runtime removes the parallel limit and the parallel constraints of every registered test.
///     The runtime reports the result of a test once its body finishes, and the test host executes the next test after that result.
///     <see cref="RevitUiTestCancellation" /> cancels the token of an executing test.
///     The end of the messages of the test host skips every pending test.
///     The members are safe to call from any thread.
/// </remarks>
internal sealed class RevitUiApplicationRuntime(RevitUiContext context)
{
    private readonly Dictionary<string, TaskCompletionSource<bool>> _executions = [];
    private readonly Dictionary<string, TestContext> _registeredTests = [];
    private readonly TaskCompletionSource<IReadOnlyList<string>> _schedule = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly Lock _stateLock = new();
    private readonly Dictionary<string, Task> _testBodies = [];

    private bool _isPendingSkipped;
    private IReadOnlyList<string> _scheduledTestIds = [];

    /// <summary>
    ///     Gets the context of the Revit user interface application.
    /// </summary>
    public RevitUiContext Context => context;

    /// <summary>
    ///     Records a UI test the run registers.
    /// </summary>
    /// <param name="registeredContext">The registration context of the test.</param>
    public void Register(TestRegisteredContext registeredContext)
    {
        lock (_stateLock)
        {
            _registeredTests[registeredContext.TestDetails.TestId] = registeredContext.TestContext;
        }

        registeredContext.ClearParallelConstraints();
        registeredContext.ClearParallelLimiter();
    }

    /// <summary>
    ///     Fails every scheduled test the run has not registered.
    /// </summary>
    /// <remarks>
    ///     A data source that yields other cases inside Revit registers tests the test host does not schedule.
    /// </remarks>
    public void ReportUnregisteredTests()
    {
        string[] unregisteredTestIds;
        lock (_stateLock)
        {
            unregisteredTestIds = [.. _scheduledTestIds.Where(testId => !_registeredTests.ContainsKey(testId))];
        }

        foreach (var testId in unregisteredTestIds)
        {
            Send(RevitUiTestResult.Failed(testId, "Revit registered no test with this identifier. The data source of the test yields other cases inside Revit."));
        }
    }

    /// <summary>
    ///     Waits for the test host to execute the specified test.
    /// </summary>
    /// <param name="testId">The identifier of the test.</param>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    /// <exception cref="SkipTestException">The test host skipped the pending tests.</exception>
    /// <remarks>
    ///     The first call for a test sends <see cref="RevitUiTestPending" />.
    /// </remarks>
    public async Task WaitForExecutionAsync(string testId)
    {
        var isExecuted = await GetExecution(testId).Task.ConfigureAwait(false);
        if (!isExecuted)
        {
            throw new SkipTestException("The test host skipped the pending test.");
        }
    }

    /// <summary>
    ///     Runs the specified test body or hook on the Revit thread inside a Revit API context.
    /// </summary>
    /// <param name="action">The body of the test or the hook.</param>
    /// <returns>A task that completes once the action and its <c>await</c> continuations finish.</returns>
    public ValueTask InvokeAsync(Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return context.InvokeAsync(action);
    }

    /// <summary>
    ///     Runs the body of the specified test on the Revit thread inside a Revit API context.
    /// </summary>
    /// <param name="testId">The identifier of the test.</param>
    /// <param name="action">The body of the test.</param>
    /// <returns>A task that completes once the body and its <c>await</c> continuations finish.</returns>
    /// <remarks>
    ///     <see cref="ReportAsync" /> sends the result of the test once the body finishes, including a body that outlives the timeout of its test.
    /// </remarks>
    public ValueTask ExecuteTestAsync(string testId, Func<ValueTask> action)
    {
        var testBody = InvokeAsync(action).AsTask();
        lock (_stateLock)
        {
            _testBodies[testId] = testBody;
        }

        return new ValueTask(testBody);
    }

    /// <summary>
    ///     Runs a <c>Before(Test)</c> hook of the specified test on the Revit thread inside a Revit API context.
    /// </summary>
    /// <param name="testId">The identifier of the test.</param>
    /// <param name="action">The body of the hook.</param>
    /// <returns>A task that completes once the hook and its <c>await</c> continuations finish.</returns>
    /// <remarks>
    ///     On .NET Framework, TUnit invokes the test start receivers after the <c>Before(Test)</c> hooks, and the hook waits for the test host to execute its test.
    /// </remarks>
    public ValueTask InvokeBeforeTestHookAsync(string testId, Func<ValueTask> action)
    {
#if NET
        return InvokeAsync(action);
#else
        return WaitAndInvokeAsync(testId, action);
#endif
    }

    /// <summary>
    ///     Waits for the test host to schedule the tests of the run.
    /// </summary>
    /// <returns>
    ///     A task that represents the asynchronous wait operation.
    ///     The task result contains the identifiers of the scheduled tests, or an empty collection once the connection closes first.
    /// </returns>
    public Task<IReadOnlyList<string>> WaitForScheduleAsync()
    {
        return _schedule.Task;
    }

    /// <summary>
    ///     Sends the result of a test to the test host once the body of the test finishes.
    /// </summary>
    /// <param name="result">The result to send.</param>
    /// <returns>A task that represents the asynchronous report operation.</returns>
    public async Task ReportAsync(RevitUiTestResult result)
    {
        Task? testBody;
        lock (_stateLock)
        {
            _testBodies.TryGetValue(result.TestId, out testBody);
        }

        if (testBody is not null)
        {
            // Task.WhenAny completes with the body, whatever the outcome of the body.
            await Task.WhenAny(testBody).ConfigureAwait(false);
        }

        Send(result);
    }

    /// <summary>
    ///     Applies the commands of the test host until the connection closes.
    /// </summary>
    /// <returns>A task that represents the asynchronous receive operation.</returns>
    public async Task ListenAsync()
    {
        try
        {
            while (await context.ReceiveAsync().ConfigureAwait(false) is { } message)
            {
                switch (RevitUiTestCommand.Parse(message))
                {
                    case RevitUiTestSchedule schedule:
                        Schedule(schedule.TestIds);
                        break;
                    case RevitUiTestExecution execution:
                        Execute(execution.TestId);
                        break;
                    case RevitUiTestCancellation cancellation:
                        Cancel(cancellation.TestId);
                        break;
                }
            }
        }
        finally
        {
            _schedule.TrySetResult([]);
            SkipPending();
        }
    }

#if !NET
    private async ValueTask WaitAndInvokeAsync(string testId, Func<ValueTask> action)
    {
        await WaitForExecutionAsync(testId).ConfigureAwait(false);
        await InvokeAsync(action).ConfigureAwait(false);
    }
#endif

    private void Send(RevitUiTestResult result)
    {
        context.Send(result.Serialize());
    }

    private void Schedule(IReadOnlyList<string> testIds)
    {
        lock (_stateLock)
        {
            _scheduledTestIds = testIds;
        }

        _schedule.TrySetResult(testIds);
    }

    private TaskCompletionSource<bool> GetExecution(string testId)
    {
        TaskCompletionSource<bool> execution;
        lock (_stateLock)
        {
            if (_executions.TryGetValue(testId, out var existingExecution))
            {
                return existingExecution;
            }

            execution = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _executions.Add(testId, execution);

            if (_isPendingSkipped)
            {
                execution.SetResult(false);
                return execution;
            }
        }

        context.Send(new RevitUiTestPending { TestId = testId }.Serialize());
        return execution;
    }

    private void Execute(string testId)
    {
        lock (_stateLock)
        {
            if (_executions.TryGetValue(testId, out var execution))
            {
                execution.TrySetResult(true);
            }
        }
    }

    private void Cancel(string testId)
    {
        TestContext? testContext;
        lock (_stateLock)
        {
            _registeredTests.TryGetValue(testId, out testContext);
        }

        testContext?.Execution.Cancel();
    }

    private void SkipPending()
    {
        lock (_stateLock)
        {
            _isPendingSkipped = true;
            foreach (var execution in _executions.Values)
            {
                execution.TrySetResult(false);
            }
        }
    }
}
