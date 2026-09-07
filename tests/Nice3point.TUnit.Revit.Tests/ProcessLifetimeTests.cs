using Nice3point.TUnit.Revit.Executors;

namespace Nice3point.TUnit.Revit.Tests;

/// <summary>
///     The connection is process-wide: a host that runs several test sessions in one process, such as
///     Visual Studio Test Explorer in testing platform server mode, must be able to run the session
///     setup again and get the same application back.
/// </summary>
public sealed class ProcessLifetimeTests : RevitApiTest
{
    [Test]
    public async Task RevitSessionSetup_SecondSessionInSameProcess_ReusesConnection()
    {
        // Arrange
        var application = Application;

        // Act
        RevitSessionSetup();

        // Assert
        await Assert.That(Application).IsSameReferenceAs(application);
        await Assert.That(Application.VersionNumber).IsNotEmpty();
    }

    [Test]
    public async Task Limiter_RevitTest_RunsOneAtATime()
    {
        // Arrange & Act
        var limiter = TestContext.Current!.Parallelism.Limiter;

        // Assert
        await Assert.That(limiter).IsTypeOf<RevitCountParallelLimit>();
        await Assert.That(limiter!.Limit).IsEqualTo(1);
    }
}
