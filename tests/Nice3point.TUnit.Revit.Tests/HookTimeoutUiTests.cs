namespace Nice3point.TUnit.Revit.Tests;

public sealed class HookTimeoutUiTests : RevitApiUiTest
{
    private bool _beforeHookCompleted;

    [Before(Test)]
    public async Task DelayBeforeTestAsync(TestContext context)
    {
        if (context.Metadata.TestDetails.MethodName == nameof(Timeout_BeforeHookExceedsBodyTimeout_RunsTheBody))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1_500), CancellationToken.None);
            _beforeHookCompleted = true;
        }
    }

    [After(Test)]
    public async Task DelayAfterTestAsync(TestContext context)
    {
        if (context.Metadata.TestDetails.MethodName == nameof(Timeout_AfterHookExceedsBodyTimeout_CompletesTheHook))
        {
            await Task.Delay(TimeSpan.FromMilliseconds(1_500), CancellationToken.None);
            await Assert.That(UiApplication.MainWindowHandle).IsNotEqualTo(IntPtr.Zero);
        }
    }

    [Test]
    [Timeout(500)]
    public async Task Timeout_BeforeHookExceedsBodyTimeout_RunsTheBody(CancellationToken cancellationToken)
    {
        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(_beforeHookCompleted).IsTrue();
            await Assert.That(cancellationToken.IsCancellationRequested).IsFalse();
            await Assert.That(UiApplication.MainWindowHandle).IsNotEqualTo(IntPtr.Zero);
        }
    }

    [Test]
    [Timeout(500)]
    public async Task Timeout_AfterHookExceedsBodyTimeout_CompletesTheHook(CancellationToken cancellationToken)
    {
        // Assert
        await Assert.That(UiApplication.MainWindowHandle).IsNotEqualTo(IntPtr.Zero);
    }
}
