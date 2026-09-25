using System.Collections.Concurrent;
using Nice3point.Revit.Injector;
using Nice3point.Revit.Injector.Ui;
using Nice3point.TUnit.Revit.Ui;
using Nice3point.TUnit.Revit.Ui.Messages;
using TUnit.Core.Exceptions;
using Assembly = System.Reflection.Assembly;

namespace Nice3point.TUnit.Revit.Sessions;

/// <summary>
///     Represents the one Revit user interface session that serves every UI test of the test host process.
/// </summary>
/// <remarks>
///     The first UI test opens the session, and <see cref="StopAsync" /> closes it when the test application finishes.
///     A test awaits the result the session receives for its identifier.
/// </remarks>
internal sealed class RevitUiSession
{
    private static readonly TimeSpan CloseTimeout = TimeSpan.FromSeconds(30);
    private readonly TaskCompletionSource _connected = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly ConcurrentQueue<string> _receivedTestIds = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<RevitUiTestResult>> _results = new();
    private readonly CancellationTokenSource _shutdown = new();
    private readonly Lock _startLock = new();
    private RevitUiConnection? _connection;
    private Exception? _fault;
    private Task? _sessionTask;

    /// <summary>
    ///     Gets the session of the test host process.
    /// </summary>
    public static RevitUiSession Instance { get; } = new();

    /// <summary>
    ///     Opens the session when it is closed, then awaits the connection to the Revit user interface application.
    /// </summary>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    public Task WaitConnectedAsync()
    {
        EnsureStarted();
        return _connected.Task;
    }

    /// <summary>
    ///     Opens the session when it is closed, then awaits the result of the specified test.
    /// </summary>
    /// <param name="testId">The identifier of the test to await.</param>
    /// <returns>A task that represents the asynchronous test run.</returns>
    /// <exception cref="SkipTestException">The test was skipped inside Revit.</exception>
    /// <exception cref="RevitUiTestException">The test failed inside Revit.</exception>
    public async Task RunTestAsync(string testId)
    {
        EnsureStarted();

        var completion = GetCompletion(testId);
        if (_fault is not null)
        {
            completion.TrySetException(_fault);
        }

        var result = await completion.Task.ConfigureAwait(false);
        switch (result.Status)
        {
            case RevitUiTestStatus.Passed:
                return;
            case RevitUiTestStatus.Skipped:
                throw new SkipTestException(result.Message ?? "The test was skipped inside Revit.");
            default:
                throw new RevitUiTestException(result.Message ?? "The test failed inside Revit.", result.StackTrace);
        }
    }

    /// <summary>
    ///     Closes the session once Revit finishes the UI tests, ending it when Revit does not finish in time.
    /// </summary>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    /// <remarks>
    ///     A call on a session that never opened has no effect.
    /// </remarks>
    public async Task StopAsync()
    {
        if (_sessionTask is null)
        {
            return;
        }

        if (_connection is not null)
        {
            await _connection.CloseAsync(CloseTimeout).ConfigureAwait(false);
        }

        await _shutdown.CancelAsync().ConfigureAwait(false);
        await _sessionTask.ConfigureAwait(false);

        if (_connection is not null)
        {
            await _connection.DisposeAsync().ConfigureAwait(false);
        }
    }

    private void EnsureStarted()
    {
        lock (_startLock)
        {
            _sessionTask ??= Task.Run(ReceiveResultsAsync);
        }
    }

    private async Task ReceiveResultsAsync()
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

            _connection = connection;
            _connected.TrySetResult();

            while (await connection.ReceiveAsync(_shutdown.Token).ConfigureAwait(false) is { } message)
            {
                var result = RevitUiTestResult.Parse(message);
                _receivedTestIds.Enqueue(result.TestId);
                GetCompletion(result.TestId).TrySetResult(result);
            }

            FaultPendingResults(new InvalidOperationException(
                $"Revit closed before it reported the test. Revit reported {_receivedTestIds.Count} result(s): [{string.Join(", ", _receivedTestIds)}]."));
        }
        catch (Exception exception)
        {
            FaultPendingResults(exception);
        }
    }

    private TaskCompletionSource<RevitUiTestResult> GetCompletion(string testId)
    {
        return _results.GetOrAdd(testId, static _ => new TaskCompletionSource<RevitUiTestResult>(TaskCreationOptions.RunContinuationsAsynchronously));
    }

    private void FaultPendingResults(Exception exception)
    {
        _fault = exception;
        _connected.TrySetResult();
        foreach (var completion in _results.Values)
        {
            completion.TrySetException(exception);
        }
    }
}
