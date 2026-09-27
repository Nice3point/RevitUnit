using Nice3point.TUnit.Revit.Ui;

namespace Nice3point.TUnit.Revit.Sessions;

/// <summary>
///     Provides the test session hooks of the UI tests.
/// </summary>
/// <remarks>
///     The hooks run without the Revit thread, in the test host process and in the Revit user interface application alike.
/// </remarks>
internal static class RevitUiSessionHooks
{
    /// <summary>
    ///     Fails every scheduled UI test the run inside Revit has not registered.
    /// </summary>
    [Before(TestSession)]
    public static void RevitUiSessionSetup()
    {
        RevitUiRuntime.Application?.ReportUnregisteredTests();
    }

    /// <summary>
    ///     Closes the Revit user interface session of the finished test session.
    /// </summary>
    /// <param name="context">The context of the test session.</param>
    /// <returns>A task that represents the asynchronous close operation.</returns>
    /// <remarks>
    ///     An IDE that reuses one test host process across runs starts a test session per run, and the next run opens a new Revit user interface session.
    /// </remarks>
    [After(TestSession)]
    public static Task RevitUiSessionCleanup(TestSessionContext context)
    {
        return RevitUiSession.StopAsync(context);
    }
}
