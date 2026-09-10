namespace Nice3point.TUnit.Revit.Tests;

public sealed class ConnectionLifetimeTests : RevitApiTest
{
    [Test]
    public async Task RevitSessionSetup_SecondCallInProcess_KeepsTheOpenConnection()
    {
        // Arrange
        var application = Application;

        // Act
        RevitSessionSetup();

        // Assert
        await Assert.That(Application).IsSameReferenceAs(application);
    }
}
