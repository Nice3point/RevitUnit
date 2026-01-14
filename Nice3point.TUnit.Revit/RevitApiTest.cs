using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit;

/// <summary>
/// Represents a test class for executing tests within the Revit environment.
/// This class provides dependency resolution, setup and cleanup methods for initializing and terminating
/// the connection to the Revit API before and after the test session.
/// </summary>
public abstract class RevitApiTest : RevitApplicationTest
{
    /// <summary>
    /// Sets up the Revit session by initializing the connection to the Revit API.
    /// This method is executed before the test session begins, ensuring that the
    /// necessary prerequisites for the tests interacting with the Revit environment are satisfied.
    /// </summary>
    [Before(TestDiscovery)]
    [HookExecutor<RevitThreadExecutor>]
    public static void RevitSessionSetup()
    {
        InitializeRevitConnection();
    }

    /// <summary>
    /// Cleans up the Revit session by terminating the connection to the Revit API.
    /// This method is executed after the test session concludes, ensuring that
    /// resources and connections related to the Revit environment are properly released.
    /// </summary>
    [After(TestSession)]
    [HookExecutor<RevitThreadExecutor>]
    public static void RevitSessionCleanup()
    {
        TerminateRevitConnection();
    }
}