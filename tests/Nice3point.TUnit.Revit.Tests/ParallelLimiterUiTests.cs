using Nice3point.TUnit.Revit.Tests.Limiters;

namespace Nice3point.TUnit.Revit.Tests;

[ParallelLimiter<SingleParallelLimit>]
public sealed class ParallelLimiterUiTests : RevitApiUiTest
{
    [Test]
    [Repeat(2)]
    public async Task ParallelLimiter_FirstTestOfTheLimit_Completes()
    {
        // Arrange & Act
        var mainWindowHandle = UiApplication.MainWindowHandle;

        // Assert
        await Assert.That(mainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    [Repeat(2)]
    public async Task ParallelLimiter_SecondTestOfTheLimit_Completes()
    {
        // Arrange & Act
        var mainWindowHandle = UiApplication.MainWindowHandle;

        // Assert
        await Assert.That(mainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }
}
