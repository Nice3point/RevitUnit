using Autodesk.Revit.ApplicationServices;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class ConnectionLifetimeTests : RevitApiTest
{
    private Application? _hookApplication;

    [Before(Test)]
    public void CaptureApplication()
    {
        _hookApplication = Application;
    }

    [Test]
    public async Task Application_InsideTestBody_IsOpenedBySessionSetup()
    {
        // Assert
        await Assert.That(Application).IsNotNull();
    }

    [Test]
    public async Task Application_InsideHookAndTestBody_ReturnsTheSameApplication()
    {
        // Assert
        await Assert.That(Application).IsSameReferenceAs(_hookApplication);
    }
}
