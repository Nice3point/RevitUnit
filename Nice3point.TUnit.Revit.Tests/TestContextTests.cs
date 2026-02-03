using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class TestContextTests : RevitApiTest
{
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task TestContext_Current_ShouldNotBeNull_InRevitThreadExecutor()
    {
        // Arrange & Act
        var context = TestContext.Current;

        // Assert
        await Assert.That(context).IsNotNull();
        await Assert.That(context.Metadata.TestName).IsEqualTo(nameof(TestContext_Current_ShouldNotBeNull_InRevitThreadExecutor));
    }

    [Test]
    [TestExecutor<DedicatedThreadExecutor>]
    public async Task TestContext_Current_ShouldNotBeNull_InDedicatedThreadExecutor()
    {
        // Arrange & Act
        var context = TestContext.Current;

        // Assert
        await Assert.That(context).IsNotNull();
        await Assert.That(context.Metadata.TestName).IsEqualTo(nameof(TestContext_Current_ShouldNotBeNull_InDedicatedThreadExecutor));
    }
}