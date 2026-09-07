using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Represents a test class for executing tests within the Revit environment.
///     This class provides dependency resolution, setup and cleanup methods for initializing and terminating
///     the connection to the Revit API before the test session and after the test application.
/// </summary>
public abstract class RevitApiTest : RevitApplicationTest
{
    /// <summary>
    ///     Sets up the Revit session by initializing the connection to the Revit API.
    ///     This method is executed before the test session begins, ensuring that the
    ///     necessary prerequisites for the tests interacting with the Revit environment are satisfied.
    /// </summary>
    /// <remarks>
    ///     Only the first session in a process injects Revit; later sessions reuse the connection.
    /// </remarks>
    [Before(TestSession)]
    [HookExecutor<RevitThreadExecutor>]
    public static void RevitSessionSetup()
    {
        InitializeRevitConnection();
    }

    /// <summary>
    ///     Cleans up the Revit session by terminating the connection to the Revit API.
    /// </summary>
    /// <remarks>
    ///     No longer runs as a hook. The connection outlives the session because a test host can run
    ///     many sessions in one process, and Revit's core cannot be loaded a second time; it is released
    ///     by <see cref="RevitApplicationLifetime" /> when the test application finishes.
    /// </remarks>
    [Obsolete("The Revit connection is released when the test application finishes, not after each session. Calling this ends the connection for the rest of the process.")]
    public static void RevitSessionCleanup()
    {
        TerminateRevitConnection();
    }
}
