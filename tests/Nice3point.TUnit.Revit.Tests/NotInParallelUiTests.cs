namespace Nice3point.TUnit.Revit.Tests;

public sealed class NotInParallelUiTests : RevitApiUiTest
{
    [Test]
    [Repeat(2)]
    [NotInParallel]
    public async Task NotInParallel_GlobalConstraint_Completes()
    {
        // Arrange & Act
        var mainWindowHandle = UiApplication.MainWindowHandle;

        // Assert
        await Assert.That(mainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    [Repeat(2)]
    [NotInParallel("First")]
    public async Task NotInParallel_FirstKey_Completes()
    {
        // Arrange & Act
        var mainWindowHandle = UiApplication.MainWindowHandle;

        // Assert
        await Assert.That(mainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    [Repeat(2)]
    [NotInParallel(["First", "Second"])]
    public async Task NotInParallel_BothKeys_Completes()
    {
        // Arrange & Act
        var mainWindowHandle = UiApplication.MainWindowHandle;

        // Assert
        await Assert.That(mainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }

    [Test]
    [Repeat(2)]
    [NotInParallel("Second")]
    public async Task NotInParallel_SecondKey_Completes()
    {
        // Arrange & Act
        var mainWindowHandle = UiApplication.MainWindowHandle;

        // Assert
        await Assert.That(mainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }
}
