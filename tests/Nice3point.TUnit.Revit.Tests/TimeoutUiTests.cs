using Nice3point.TUnit.Revit.Tests.Attributes;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class TimeoutUiTests : RevitApiUiTest
{
    private static DateTimeOffset? _timedOutBodyEnd;
    private static bool _cleanupObservedCompletedBody;

    [Before(Test)]
    public void ResetTimedOutBody(TestContext context)
    {
        if (context.Metadata.TestDetails.MethodName == nameof(Timeout_BodyIgnoresTheCancellation_TimesOut))
        {
            _timedOutBodyEnd = null;
            _cleanupObservedCompletedBody = false;
        }
    }

    [Test]
    [Timeout(500)]
    [ExpectedTimeout]
    public async Task Timeout_BodyIgnoresTheCancellation_TimesOut(CancellationToken cancellationToken)
    {
        // Act
        await Task.Delay(TimeSpan.FromSeconds(3), CancellationToken.None);
        _timedOutBodyEnd = DateTimeOffset.UtcNow;
    }

    [Test]
    [DependsOn(nameof(Timeout_BodyIgnoresTheCancellation_TimesOut))]
    public async Task Start_PreviousBodyOutlivesItsTimeout_FollowsTheEndOfThatBody()
    {
        // Arrange & Act
        var testStart = TestContext.Current!.Execution.TestStart!.Value;

        // Assert
        var timedOutBodyEnd = await Assert.That(_timedOutBodyEnd).IsNotNull();
        await Assert.That(testStart).IsGreaterThanOrEqualTo(timedOutBodyEnd);
        await Assert.That(_cleanupObservedCompletedBody).IsTrue();
    }

    [After(Test)]
    public void RecordCleanup()
    {
        _cleanupObservedCompletedBody = _timedOutBodyEnd is not null;
    }
}
