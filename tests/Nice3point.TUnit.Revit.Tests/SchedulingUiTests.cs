namespace Nice3point.TUnit.Revit.Tests;

public sealed class SchedulingUiTests : RevitApiUiTest
{
    private static int _attempts;

    [Test]
    [Retry(1)]
    public async Task Retry_FirstAttemptFails_PassesOnTheSecondAttempt()
    {
        // Arrange & Act
        var attempt = Interlocked.Increment(ref _attempts);

        // Assert
        await Assert.That(attempt).IsGreaterThan(1);
    }

    [Test]
    [Timeout(500)]
    [Retry(1, BackoffMs = 1_500)]
    public async Task Retry_BackoffExceedsBodyTimeout_PassesOnTheSecondAttempt(CancellationToken cancellationToken)
    {
        // Act
        var attempt = TestContext.Current!.Execution.CurrentRetryAttempt;

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(attempt).IsEqualTo(1);
            await Assert.That(cancellationToken.IsCancellationRequested).IsFalse();
            await Assert.That(UiApplication.MainWindowHandle).IsNotEqualTo(IntPtr.Zero);
        }
    }

    [Test]
    public async Task Delay_LongerThanTheTimeoutOfAnotherTest_Completes()
    {
        // Arrange & Act
        await Task.Delay(TimeSpan.FromSeconds(2));

        // Assert
        await Assert.That(UiApplication.MainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    [Timeout(1_000)]
    public async Task Timeout_TestPendingLongerThanItsTimeout_TimesOnlyTheExecution(CancellationToken cancellationToken)
    {
        // Arrange & Act
        var isCancelled = cancellationToken.IsCancellationRequested;

        // Assert
        await Assert.That(isCancelled).IsFalse();
    }
}
