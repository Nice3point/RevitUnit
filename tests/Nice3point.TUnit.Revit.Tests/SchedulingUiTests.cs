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
