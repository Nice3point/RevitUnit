using Autodesk.Revit.ApplicationServices;
using Nice3point.TUnit.Revit.Executors;
using Nice3point.TUnit.Revit.Lifetimes;
using Nice3point.TUnit.Revit.Sessions;
using Nice3point.TUnit.Revit.Ui;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Represents a test class for executing tests within the Revit environment.
/// </summary>
/// <remarks>
///     The connection to the Revit API opens on the first test a session executes, and <see cref="RevitConnectionLifetime" /> closes it when the test application finishes.
///     Test discovery executes no test and does not start Revit.
/// </remarks>
[TestExecutor<RevitThreadExecutor>]
[HookExecutor<RevitThreadExecutor>]
public abstract class RevitApiTest
{
    /// <inheritdoc cref="Autodesk.Revit.ApplicationServices.Application" />
    /// <exception cref="System.InvalidOperationException">The property is read outside a test body or a hook.</exception>
    protected static Application Application => RevitSession.Instance.Application ?? throw new InvalidOperationException("The Revit application is available only inside a test or its hooks.");

    /// <summary>
    ///     Sets up the Revit session by initializing the connection to the Revit API.
    /// </summary>
    /// <param name="context">The context of the test session.</param>
    /// <remarks>
    ///     The first test a session executes triggers the hook.
    ///     Test discovery executes no test and does not start Revit.
    /// </remarks>
    [Before(TestSession, Order = int.MinValue)]
    public static void RevitSessionSetup(TestSessionContext context)
    {
        if (RevitUiRuntime.InRevitProcess)
        {
            return;
        }

        // TestClasses includes the classes a property filter, such as a category, excludes.
        if (!context.TestClasses.Any(static testClass => testClass.ClassType.IsSubclassOf(typeof(RevitApiTest))))
        {
            return;
        }

        RevitSession.Instance.EnsureStarted();
    }
}
