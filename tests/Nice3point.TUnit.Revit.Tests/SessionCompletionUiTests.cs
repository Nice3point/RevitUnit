using Nice3point.TUnit.Revit.Tests.Attributes;

namespace Nice3point.TUnit.Revit.Tests;

[SessionCompletionProbe]
public sealed class SessionCompletionUiTests : RevitApiUiTest
{
    [Test]
    [Timeout(500)]
    public async Task Body_BeforeSessionCleanup_Passes(CancellationToken cancellationToken)
    {
        // Act
        if (Environment.GetEnvironmentVariable(SessionCompletionProbeAttribute.Variable) == "timeout")
        {
            await Task.Delay(TimeSpan.FromSeconds(3), CancellationToken.None);
        }

        // Assert
        await Assert.That(UiApplication.MainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }
}
