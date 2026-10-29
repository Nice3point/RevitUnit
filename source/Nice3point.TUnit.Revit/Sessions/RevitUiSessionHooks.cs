using Nice3point.TUnit.Revit.Ui;

namespace Nice3point.TUnit.Revit.Sessions;

/// <summary>
///     Provides the test session and class hooks of the UI tests.
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
    ///     Waits inside Revit until the test host executes the first test of a UI test class.
    /// </summary>
    /// <param name="context">The context of the class.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken" /> used to cancel the wait.</param>
    /// <returns>A task that represents the asynchronous wait operation.</returns>
    /// <remarks>
    ///     TUnit runs the hook before the <c>[Before(Class)]</c> hooks of the class, and the timeout of a <c>[Before(Class)]</c> hook excludes the wait.
    ///     The hook returns at once in the test host process and for a class not derived from <see cref="RevitApiUiTest" />.
    /// </remarks>
    [BeforeEvery(Class, Order = int.MinValue)]
    [Timeout(int.MaxValue)]
    public static Task RevitUiClassSetup(ClassHookContext context, CancellationToken cancellationToken)
    {
        if (RevitUiRuntime.Application is not { } runtime || !typeof(RevitApiUiTest).IsAssignableFrom(context.ClassType))
        {
            return Task.CompletedTask;
        }

        return runtime.WaitForAnyExecutionAsync(context.Tests.Select(static test => test.Metadata.TestDetails.TestId), cancellationToken);
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
