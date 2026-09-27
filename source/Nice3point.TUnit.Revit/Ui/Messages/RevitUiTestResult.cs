using Microsoft.Testing.Platform.Extensions.Messages;

namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Represents the result of one UI test.
/// </summary>
[PublicAPI]
internal sealed record RevitUiTestResult : RevitUiTestEvent
{
    /// <summary>
    ///     Gets the final state of the test.
    /// </summary>
    public required RevitUiTestStatus Status { get; init; }

    /// <summary>
    ///     Gets the failure or skip message, or <see langword="null" /> when the test passed.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    ///     Gets the stack trace of a failure, or <see langword="null" /> when none applies.
    /// </summary>
    public string? StackTrace { get; init; }

    /// <summary>
    ///     Gets the standard output the test wrote, or <see langword="null" /> when it wrote none.
    /// </summary>
    public string? StandardOutput { get; init; }

    /// <summary>
    ///     Gets the standard error the test wrote, or <see langword="null" /> when it wrote none.
    /// </summary>
    public string? StandardError { get; init; }

    /// <summary>
    ///     Creates the failed result of the specified test.
    /// </summary>
    /// <param name="testId">The identifier of the failed test.</param>
    /// <param name="message">The failure message of the test.</param>
    /// <returns>The created result.</returns>
    [Pure]
    public static RevitUiTestResult Failed(string testId, string message)
    {
        return new RevitUiTestResult
        {
            TestId = testId,
            Status = RevitUiTestStatus.Failed,
            Message = message
        };
    }

    /// <summary>
    ///     Creates the result of a test from the node the test platform reports for it.
    /// </summary>
    /// <param name="testNode">The node the test platform reports for the test.</param>
    /// <returns>The created result, or <see langword="null" /> when the node holds no final state.</returns>
    [Pure]
    public static RevitUiTestResult? FromTestNode(TestNode testNode)
    {
        if (testNode.Properties.SingleOrDefault<TestNodeStateProperty>() is not { } state)
        {
            return null;
        }

        if (FromState(testNode.Uid.Value, state) is not { } result)
        {
            return null;
        }

        return result with
        {
            StandardOutput = testNode.Properties.SingleOrDefault<StandardOutputProperty>()?.StandardOutput,
            StandardError = testNode.Properties.SingleOrDefault<StandardErrorProperty>()?.StandardError
        };
    }

    private static RevitUiTestResult? FromState(string testId, TestNodeStateProperty state)
    {
        return state switch
        {
            PassedTestNodeStateProperty => new RevitUiTestResult
            {
                TestId = testId,
                Status = RevitUiTestStatus.Passed
            },
            SkippedTestNodeStateProperty => new RevitUiTestResult
            {
                TestId = testId,
                Status = RevitUiTestStatus.Skipped,
                Message = state.Explanation
            },
            FailedTestNodeStateProperty failed => FromFailure(testId, state, failed.Exception),
            ErrorTestNodeStateProperty error => FromFailure(testId, state, error.Exception),
            TimeoutTestNodeStateProperty timeout => FromFailure(testId, state, timeout.Exception),
            InProgressTestNodeStateProperty or DiscoveredTestNodeStateProperty => null,
            _ => FromFailure(testId, state, null)
        };
    }

    private static RevitUiTestResult FromFailure(string testId, TestNodeStateProperty state, Exception? exception)
    {
        return new RevitUiTestResult
        {
            TestId = testId,
            Status = RevitUiTestStatus.Failed,
            Message = exception?.Message ?? state.Explanation ?? $"The test ended with {state.GetType().Name} in Revit.",
            StackTrace = exception?.StackTrace
        };
    }
}
