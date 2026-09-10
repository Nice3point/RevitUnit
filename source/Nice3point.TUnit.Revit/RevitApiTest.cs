using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Represents a test class for executing tests within the Revit environment.
///     This class provides dependency resolution and the setup that establishes the connection to the Revit API before the test session.
/// </summary>
/// <remarks>
///     <see cref="RevitConnectionLifetime"/> closes the connection when the test application finishes.
/// </remarks>
public abstract class RevitApiTest : RevitApplicationTest
{
    /// <summary>
    ///     Sets up the Revit session by initializing the connection to the Revit API.
    ///     This method is executed before the test session begins, ensuring that the
    ///     necessary prerequisites for the tests interacting with the Revit environment are satisfied.
    /// </summary>
    /// <remarks>
    ///     The first test a session executes triggers the hook.
    ///     Listing the tests of an assembly executes none, and leaves Revit unstarted.
    /// </remarks>
    [Before(TestSession)]
    [HookExecutor<RevitThreadExecutor>]
    public static void RevitSessionSetup()
    {
        InitializeRevitConnection();
    }
}
