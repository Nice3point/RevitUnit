using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class TestContextTests : RevitApiTest
{
    [Test]
    public async Task Current_RevitThreadExecutor_ReturnsRunningTestContext()
    {
        // Arrange & Act
        var context = TestContext.Current;

        // Assert
        await Assert.That(context).IsNotNull();
        await Assert.That(context.Metadata.TestName).IsEqualTo(nameof(Current_RevitThreadExecutor_ReturnsRunningTestContext));
    }

    [Test]
    [TestExecutor<DedicatedThreadExecutor>]
    public async Task Current_DedicatedThreadExecutor_ReturnsRunningTestContext()
    {
        // Arrange & Act
        var context = TestContext.Current;

        // Assert
        await Assert.That(context).IsNotNull();
        await Assert.That(context.Metadata.TestName).IsEqualTo(nameof(Current_DedicatedThreadExecutor_ReturnsRunningTestContext));
    }
}
