namespace Nice3point.TUnit.Revit.Tests;

public sealed class ParallelLimitUiTests : RevitApiUiTest
{
    private static int _concurrent;
    private static int _peak;

    [Test]
    [Repeat(4)]
    public async Task RevitUiTests_ScheduledTogether_ObserveThemselvesAlone()
    {
        // Arrange
        var observed = Interlocked.Increment(ref _concurrent);
        RecordPeak(observed);

        // Act
        await Task.Delay(50);
        Interlocked.Decrement(ref _concurrent);

        // Assert
        await Assert.That(observed).IsEqualTo(1);
    }

    [Test]
    [DependsOn(nameof(RevitUiTests_ScheduledTogether_ObserveThemselvesAlone))]
    public async Task RevitUiTests_ScheduledTogether_NeverOverlap()
    {
        // Arrange & Act & Assert
        await Assert.That(_peak).IsEqualTo(1);
    }

    private static void RecordPeak(int observed)
    {
        int previous;
        do
        {
            previous = _peak;
            if (observed <= previous)
            {
                return;
            }
        } while (Interlocked.CompareExchange(ref _peak, observed, previous) != previous);
    }
}
