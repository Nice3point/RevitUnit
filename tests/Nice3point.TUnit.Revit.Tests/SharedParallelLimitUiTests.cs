using Nice3point.TUnit.Revit.Limiters;

namespace Nice3point.TUnit.Revit.Tests;

[ParallelLimiter<RevitParallelLimit>]
public sealed class SharedParallelLimitUiTests : RevitApiUiTest
{
    [Test]
    [Repeat(2)]
    public async Task ParallelLimiter_RevitParallelLimit_Completes()
    {
        // Arrange & Act
        var mainWindowHandle = UiApplication.MainWindowHandle;

        // Assert
        await Assert.That(mainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }
}
