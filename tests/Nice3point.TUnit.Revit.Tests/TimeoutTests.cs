using Nice3point.TUnit.Revit.Tests.Attributes;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class TimeoutTests : RevitApiTest
{
    private static bool _bodyFinished;
    private static bool _cancellationRequested;
    private static bool _cleanupObservedCompletedBody;
    private static bool _cleanupObservedCancellation;

    [Before(Test)]
    public void ResetBody()
    {
        _bodyFinished = false;
        _cancellationRequested = false;
    }

    [Test]
    [Timeout(500)]
    [ExpectedTimeout]
    public async Task Timeout_BodyIgnoresTheCancellation_FinishesBeforeCleanup(CancellationToken cancellationToken)
    {
        // Act
        await Task.Delay(TimeSpan.FromSeconds(3), CancellationToken.None);

        // Assert
        await Assert.That(Application.VersionNumber).IsNotEmpty();
        _cancellationRequested = cancellationToken.IsCancellationRequested;
        _bodyFinished = true;
    }

    [Test]
    [DependsOn(nameof(Timeout_BodyIgnoresTheCancellation_FinishesBeforeCleanup))]
    public async Task Cleanup_TimedOutBody_ObservedItsCompletion()
    {
        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(_cleanupObservedCompletedBody).IsTrue();
            await Assert.That(_cleanupObservedCancellation).IsTrue();
        }
    }

    [After(Test)]
    public void RecordCleanup()
    {
        _cleanupObservedCompletedBody = _bodyFinished;
        _cleanupObservedCancellation = _cancellationRequested;
    }
}
