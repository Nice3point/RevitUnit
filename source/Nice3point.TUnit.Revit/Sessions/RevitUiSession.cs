using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Nice3point.Revit.Injector;
using Nice3point.Revit.Injector.Ui;
using Nice3point.TUnit.Revit.Ui;
using Nice3point.TUnit.Revit.Ui.Messages;
using TUnit.Core.Exceptions;
using Assembly = System.Reflection.Assembly;

namespace Nice3point.TUnit.Revit.Sessions;

/// <summary>
///     Represents the Revit user interface session that serves the UI tests of one test session.
/// </summary>
/// <remarks>
///     Every test session holds a session of its own, and an IDE that reuses one test host process across runs starts a test session per run.
///     The first UI test that starts opens the session, and <see cref="StopAsync" /> closes it when the test session finishes.
///     The session schedules in Revit the UI tests registered before it opens.
///     A test waits until it is pending in Revit and until Revit reports the result of the test executed before it, then the session executes it and awaits the result Revit reports for the identifier of the test.
///     A test that the test host cancels, or whose timeout elapses, keeps the next test waiting until Revit reports its result.
/// </remarks>
internal sealed class RevitUiSession
{
    private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(30);
    private static readonly ConditionalWeakTable<TestSessionContext, RevitUiSession> Sessions = new();
    private readonly SemaphoreSlim _executionSlot = new(1, 1);

    private readonly ConcurrentDictionary<string, byte> _registeredTestIds = new();
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Lock _startLock = new();
    private readonly ConcurrentDictionary<string, TestState> _tests = new();
    private volatile RevitUiConnection? _connection;
    private volatile Exception? _fault;
    private volatile HashSet<string>? _scheduledTestIds;
    private Task? _sessionTask;

    /// <summary>
    ///     Gets the session of the test session that runs the specified test.
    /// </summary>
    /// <param name="context">The context of the test.</param>
    /// <returns>The session of the test session.</returns>
    public static RevitUiSession Get(TestContext context)
    {
        return Sessions.GetValue(context.ClassContext.AssemblyContext.TestSessionContext, static _ => new RevitUiSession());
    }

    /// <summary>
    ///     Skips the pending UI tests of the specified test session, then closes its session once Revit finishes them, ending Revit when it does not finish in time.
    /// </summary>
    /// <param name="context">The context of the test session.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    /// <remarks>
    ///     A call for a test session that never started Revit has no effect.
    /// </remarks>
    public static Task StopAsync(TestSessionContext context)
    {
        if (!Sessions.TryGetValue(context, out var session))
        {
            return Task.CompletedTask;
        }

        Sessions.Remove(context);
        return session.CloseAsync();
    }

    /// <summary>
    ///     Records a UI test the test session registers.
    /// </summary>
    /// <param name="testId">The identifier of the registered test.</param>
    public void Register(string testId)
    {
        _registeredTestIds.TryAdd(testId, 0);
    }

    /// <summary>
    ///     Opens the session when it is closed, awaits the specified test to become pending in Revit, then awaits the slot that executes one test at a time.
    /// </summary>
    /// <param name="testId">The identifier of the test to start.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> used to cancel the wait.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    /// <remarks>
    ///     The test holds the slot until Revit reports its result, or until <see cref="EndTest" /> ends a test the session never executed.
    /// </remarks>
    public async Task StartTestAsync(string testId, CancellationToken cancellationToken)
    {
        await WaitPendingAsync(testId, cancellationToken).ConfigureAwait(false);
        await _executionSlot.WaitAsync(cancellationToken).ConfigureAwait(false);
        GetTest(testId).HoldSlot();
    }

    /// <summary>
    ///     Executes the specified pending test in Revit, then awaits its result and copies the output of the test into the context.
    /// </summary>
    /// <param name="context">The context of the test to execute.</param>
    /// <returns>A task that represents the asynchronous test run.</returns>
    /// <exception cref="SkipTestException">The test was skipped inside Revit.</exception>
    /// <exception cref="RevitUiTestException">The test failed inside Revit.</exception>
    /// <exception cref="RevitUiTimeoutException">The test timed out inside Revit.</exception>
    /// <exception cref="System.OperationCanceledException">The token of the test was cancelled, and the session cancels the test in Revit.</exception>
    public async Task RunTestAsync(TestContext context)
    {
        var testId = context.Metadata.TestDetails.TestId;
        var cancellationToken = context.Execution.CancellationToken;
        await WaitPendingAsync(testId, cancellationToken).ConfigureAwait(false);

        var test = GetTest(testId);
        if (!test.Result.Task.IsCompleted)
        {
            test.IsExecuted = true;
            _connection!.Send(new RevitUiTestExecution
            {
                TestId = testId
            }.Serialize());
        }

        RevitUiTestResult result;
        try
        {
            result = await test.Result.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested && test.IsExecuted)
        {
            _connection!.Send(new RevitUiTestCancellation
            {
                TestId = testId
            }.Serialize());
            throw;
        }

        context.Metadata.TestDetails.Timeout = result.Timeout;

        if (result.StandardOutput is not null)
        {
            await context.Output.StandardOutput.WriteAsync(result.StandardOutput).ConfigureAwait(false);
        }

        if (result.StandardError is not null)
        {
            await context.Output.ErrorOutput.WriteAsync(result.StandardError).ConfigureAwait(false);
        }

        switch (result.Status)
        {
            case RevitUiTestStatus.Passed:
                return;
            case RevitUiTestStatus.Skipped:
                throw new SkipTestException(result.Message ?? "The test was skipped in Revit.");
            case RevitUiTestStatus.TimedOut:
                throw new RevitUiTimeoutException(result.Message ?? "The test timed out in Revit.", result.StackTrace);
            default:
                throw new RevitUiTestException(result.Message ?? "The test failed in Revit.", result.StackTrace);
        }
    }

    /// <summary>
    ///     Releases the slot of the specified test once the test host ends it.
    /// </summary>
    /// <param name="testId">The identifier of the ended test.</param>
    /// <remarks>
    ///     A test executed in Revit keeps the slot until Revit reports its result.
    /// </remarks>
    public void EndTest(string testId)
    {
        if (!_tests.TryGetValue(testId, out var test))
        {
            return;
        }

        if (!test.IsExecuted || test.Result.Task.IsCompleted)
        {
            test.ReleaseSlot();
        }
    }

    private Task WaitPendingAsync(string testId, CancellationToken cancellationToken)
    {
        lock (_startLock)
        {
            _sessionTask ??= Task.Run(ReceiveEventsAsync, CancellationToken.None);
        }

        return GetTest(testId).Pending.Task.WaitAsync(cancellationToken);
    }

    private async Task CloseAsync()
    {
        if (_sessionTask is null)
        {
            return;
        }

        var connection = _connection;
        try
        {
            if (connection is not null)
            {
                var endSessionTask = Task.Run(() => connection.Send(new RevitUiTestSessionEnd().Serialize()), CancellationToken.None);
                await Task.WhenAll(endSessionTask, _sessionTask).WaitAsync(CloseTimeout).ConfigureAwait(false);
            }
            else
            {
                await _shutdown.CancelAsync().ConfigureAwait(false);
                await _sessionTask.WaitAsync(CloseTimeout).ConfigureAwait(false);
            }
        }
        finally
        {
            await _shutdown.CancelAsync().ConfigureAwait(false);
            RevitUiConnection? closingConnection;
            lock (_startLock)
            {
                closingConnection = _connection;
            }

            try
            {
                if (closingConnection is not null)
                {
                    await closingConnection.CloseAsync(CloseTimeout).ConfigureAwait(false);
                }
            }
            finally
            {
                await Task.WhenAny(_sessionTask).ConfigureAwait(false);
                try
                {
                    if (_connection is not null)
                    {
                        await _connection.DisposeAsync().ConfigureAwait(false);
                    }
                }
                finally
                {
                    _shutdown.Dispose();
                    _executionSlot.Dispose();
                }
            }
        }
    }

    private async Task ReceiveEventsAsync()
    {
        try
        {
            var options = new RevitUiApplicationOptions
            {
                EntryPoint = typeof(RevitUiTestRunner),
                Properties = new Dictionary<string, string>
                {
                    [RevitUiTestRunner.TestAssemblyProperty] = Assembly.GetEntryAssembly()!.Location
                }
            };

            var connection = await new Injector()
                .InjectUiApplicationAsync(options, _shutdown.Token)
                .ConfigureAwait(false);

            lock (_startLock)
            {
                _connection = connection;
                _shutdown.Token.ThrowIfCancellationRequested();
            }

            Schedule(connection);

            while (await connection.ReceiveAsync(_shutdown.Token).ConfigureAwait(false) is { } message)
            {
                switch (RevitUiTestEvent.Parse(message))
                {
                    case RevitUiTestPending pending:
                        GetTest(pending.TestId).Pending.TrySetResult();
                        break;
                    case RevitUiTestResult result:
                        GetTest(result.TestId).Complete(result);
                        break;
                }
            }

            Fault(new InvalidOperationException("Revit closed before it reported the test."));
        }
        catch (OperationCanceledException exception) when (_shutdown.IsCancellationRequested)
        {
            Fault(exception);
        }
        catch (Exception exception)
        {
            Fault(exception);
            throw;
        }
    }

    private void Schedule(RevitUiConnection connection)
    {
        var scheduledTestIds = new HashSet<string>(_registeredTestIds.Keys);
        _scheduledTestIds = scheduledTestIds;

        connection.Send(new RevitUiTestSchedule
        {
            TestIds = [.. scheduledTestIds]
        }.Serialize());

        foreach (var testId in _tests.Keys)
        {
            GetTest(testId);
        }
    }

    private TestState GetTest(string testId)
    {
#if NET
        var test = _tests.GetOrAdd(testId, static (_, executionSlot) => new TestState(executionSlot), _executionSlot);
#else
        var test = _tests.GetOrAdd(testId, _ => new TestState(_executionSlot));
#endif
        if (_fault is { } fault)
        {
            test.Fault(fault);
        }
        else if (_scheduledTestIds is { } scheduledTestIds && !scheduledTestIds.Contains(testId))
        {
            test.Complete(RevitUiTestResult.Failed(testId, "The test registered after the Revit session opened, and Revit has not scheduled it."));
        }

        return test;
    }

    private void Fault(Exception exception)
    {
        _fault = exception;
        foreach (var test in _tests.Values)
        {
            test.Fault(exception);
        }
    }

    private sealed class TestState(SemaphoreSlim executionSlot)
    {
        private int _isHoldingSlot;

        public TaskCompletionSource Pending { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource<RevitUiTestResult> Result { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool IsExecuted { get; set; }

        public void HoldSlot()
        {
            Volatile.Write(ref _isHoldingSlot, 1);
        }

        public void ReleaseSlot()
        {
            if (Interlocked.Exchange(ref _isHoldingSlot, 0) == 1)
            {
                executionSlot.Release();
            }
        }

        public void Complete(RevitUiTestResult result)
        {
            Result.TrySetResult(result);
            Pending.TrySetResult();
            ReleaseSlot();
        }

        public void Fault(Exception exception)
        {
            Result.TrySetException(exception);
            Pending.TrySetResult();
            ReleaseSlot();
        }
    }
}
