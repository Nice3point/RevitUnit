using Autodesk.Revit.UI;
using Nice3point.TUnit.Revit.Executors;
using Nice3point.TUnit.Revit.Sessions;
using Nice3point.TUnit.Revit.Ui;
using TUnit.Core.Executors;
using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Represents a test class for executing tests within the Revit user interface environment.
/// </summary>
/// <remarks>
///     The Revit user interface environment opens on the first UI test a session executes and closes when the test application finishes.
///     Test bodies and hooks run on the Revit thread inside a Revit API context.
///     Test discovery executes no test and does not start Revit.
/// </remarks>
[Category(RevitUiRuntime.Category)]
[TestExecutor<RevitUiThreadExecutor>]
[HookExecutor<RevitUiThreadExecutor>]
public abstract class RevitApiUiTest : ITestStartEventReceiver
{
    /// <inheritdoc cref="Autodesk.Revit.UI.UIApplication" />
    /// <exception cref="System.InvalidOperationException">The property is read outside a test body or a hook.</exception>
    protected static UIApplication UiApplication => RevitUiRuntime.Context?.UiApplication ?? throw new InvalidOperationException("The Revit user interface application is available only inside a UI test or its hooks.");

    int IEventReceiver.Order => 0;

    ValueTask ITestStartEventReceiver.OnTestStart(TestContext context)
    {
        if (RevitUiRuntime.InRevitProcess)
        {
            return default;
        }

        return new ValueTask(RevitUiSession.Instance.WaitConnectedAsync());
    }
}
