using Autodesk.Revit.UI;
using Nice3point.TUnit.Revit.Executors;
using Nice3point.TUnit.Revit.Sessions;
using Nice3point.TUnit.Revit.Ui;
using TUnit.Core.Executors;
using TUnit.Core.Interfaces;
#if NET
using TUnit.Core.Enums;
#endif

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Represents a test class for executing tests within the Revit user interface environment.
/// </summary>
/// <remarks>
///     The Revit user interface environment opens on the first UI test a session executes and closes when the session finishes.
///     Test bodies and hooks run on the Revit thread inside a Revit API context.
///     UI tests run one at a time, and the next UI test starts once the body of a timed-out test returns.
///     Test discovery executes no test and does not start Revit.
/// </remarks>
[Category(RevitUiRuntime.Category)]
[TestExecutor<RevitUiThreadExecutor>]
[HookExecutor<RevitUiThreadExecutor>]
public abstract class RevitApiUiTest : ITestStartEventReceiver, ITestEndEventReceiver
{
    /// <inheritdoc cref="Autodesk.Revit.UI.UIApplication" />
    /// <exception cref="System.InvalidOperationException">The property is read outside a test body or a hook.</exception>
    protected static UIApplication UiApplication => RevitUiRuntime.Application?.Context.UiApplication ?? throw new InvalidOperationException("The Revit user interface application is available only inside a UI test or its hooks.");

#if NET
    EventReceiverStage ITestStartEventReceiver.Stage => EventReceiverStage.Early;
#else
    int IEventReceiver.Order => 0;
#endif

    ValueTask ITestStartEventReceiver.OnTestStart(TestContext context)
    {
        var testId = context.Metadata.TestDetails.TestId;
        if (RevitUiRuntime.Application is { } runtime)
        {
            return new ValueTask(runtime.WaitForExecutionAsync(testId));
        }

        var session = RevitUiSession.Get(context);
        return new ValueTask(session.StartTestAsync(testId, context.Execution.CancellationToken));
    }

    ValueTask ITestEndEventReceiver.OnTestEnd(TestContext context)
    {
        if (RevitUiRuntime.InRevitProcess)
        {
            return default;
        }

        RevitUiSession.Get(context).EndTest(context.Metadata.TestDetails.TestId);
        return default;
    }
}
