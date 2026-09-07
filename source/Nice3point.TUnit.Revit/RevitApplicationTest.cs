using Autodesk.Revit.ApplicationServices;
using Nice3point.Revit.Injector;
using Nice3point.TUnit.Revit.Executors;

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Represents an abstract base class for tests that require interaction with the Revit application environment.
///     Provides methods to initialize and terminate the connection to the Revit application.
/// </summary>
/// <remarks>
///     The connection lives as long as the test host process. Revit's core can be loaded into a process
///     only once, and a host such as Visual Studio Test Explorer runs many test sessions in one process,
///     so the connection is opened by the first session that needs it and released when the test
///     application finishes, not per session.
/// </remarks>
[ParallelLimiter<RevitCountParallelLimit>]
public abstract class RevitApplicationTest
{
    private static readonly object InjectionLock = new();
    private static Injector? _injector;
    private static bool _ejected;

    /// <summary>
    ///     Represents the database level Autodesk Revit Application, providing access to documents, options and other application wide data and settings.
    /// </summary>
    protected static Application Application { get; private set; } = null!;

    /// <summary>
    ///     Initializes the connection to the Revit application.
    /// </summary>
    /// <remarks>
    ///     Injects the Revit core the first time it is called in the process; every later call is a no-op,
    ///     so any number of test sessions can share one connection.
    /// </remarks>
    /// <exception cref="InvalidOperationException">The connection was already terminated in this process.</exception>
    protected static void InitializeRevitConnection()
    {
        lock (InjectionLock)
        {
            if (_ejected)
            {
                throw new InvalidOperationException(
                    "The Revit connection was already terminated in this process. Revit's core cannot be loaded twice; start a new test host.");
            }

            if (_injector is not null)
            {
                return;
            }

            var injector = new Injector();
            Application = injector.InjectApplication();
            _injector = injector;
        }
    }

    /// <summary>
    ///     Whether the process holds a live Revit connection.
    /// </summary>
    internal static bool IsConnected
    {
        get
        {
            lock (InjectionLock)
            {
                return _injector is not null && !_ejected;
            }
        }
    }

    /// <summary>
    ///     Entry point for <see cref="RevitApplicationLifetime" />, which is not a test class.
    /// </summary>
    internal static void ReleaseConnection()
    {
        TerminateRevitConnection();
    }

    /// <summary>
    ///     Terminates the connection to the Revit application.
    ///     Frees associated resources and properly closes the interaction with the Revit environment.
    /// </summary>
    /// <remarks>
    ///     Ejects the Revit core once; later calls are no-ops. After this, the connection cannot be
    ///     re-established in the same process.
    /// </remarks>
    protected static void TerminateRevitConnection()
    {
        lock (InjectionLock)
        {
            if (_injector is null || _ejected)
            {
                return;
            }

            _ejected = true;
            _injector.EjectApplication();
        }
    }
}
