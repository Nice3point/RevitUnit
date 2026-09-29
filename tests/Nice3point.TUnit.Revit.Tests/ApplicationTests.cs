using Nice3point.TUnit.Revit.Executors;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class ApplicationTests : RevitApiTest
{
    [Test]
    public async Task Cities_BuiltinSet_IsNotEmpty()
    {
        // Arrange & Act
        var cities = Application.Cities.Cast<City>();

        // Assert
        await Assert.That(cities).IsNotEmpty();
    }

    [Test]
    public async Task Create_XYZ_ValidDistance()
    {
        // Arrange & Act
        var point = Application.Create.NewXYZ(3, 4, 5);

        // Assert
        await Assert.That(point.DistanceTo(XYZ.Zero)).IsEqualTo(7).Within(0.1);
    }

    [Test]
    public async Task RevitSessionSetup_OffTheRevitThread_ThrowsInvalidOperationException()
    {
        // Arrange
        var testSession = TestContext.Current!.ClassContext.AssemblyContext.TestSessionContext;

        // Act & Assert
        await Assert.That(() => Task.Run(() => RevitSessionSetup(testSession))).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task RevitThreadExecutor_NestedExecution_CompletesOnTheRevitThread()
    {
        // Arrange
        var threadId = Environment.CurrentManagedThreadId;
        var context = TestContext.Current!;
        var executor = new RevitThreadExecutor();

        // Act & Assert
        await executor.ExecuteTest(context, async () =>
        {
            await Task.Yield();
            await Assert.That(Environment.CurrentManagedThreadId).IsEqualTo(threadId);
            await Assert.That(Application.VersionNumber).IsNotEmpty();
        }).AsTask().WaitAsync(TimeSpan.FromSeconds(2));
    }
}
