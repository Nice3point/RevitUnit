using Autodesk.Revit.ApplicationServices;
using Nice3point.Revit.Injector;

namespace Nice3point.TUnit.Revit;

/// <summary>
///     Represents an abstract base class for tests that require interaction with the Revit application environment.
///     Provides methods to initialize and terminate the connection to the Revit application.
/// </summary>
/// <remarks>
///     One connection serves the whole test host process.
///     Revit activates once per process, and an IDE that keeps a test host alive starts a test session per run in that process.
/// </remarks>
public abstract class RevitApplicationTest
{
    private static readonly Lock ConnectionLock = new();
    private static Injector? _injector;

    /// <summary>
    ///     Represents the database level Autodesk Revit Application, providing access to documents, options and other application wide data and settings.
    /// </summary>
    protected static Application Application { get; private set; } = null!;

    /// <summary>
    ///     Gets a value indicating whether the process holds an open connection to the Revit application.
    /// </summary>
    internal static bool IsConnected
    {
        get
        {
            lock (ConnectionLock)
            {
                return _injector is not null;
            }
        }
    }

    /// <summary>
    ///     Initializes the connection to the Revit application.
    /// </summary>
    /// <exception cref="InvalidOperationException">The process terminated its connection earlier.</exception>
    /// <remarks>
    ///     The first call of the process opens the connection, and every later call keeps the open one.
    /// </remarks>
    protected static void InitializeRevitConnection()
    {
        lock (ConnectionLock)
        {
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
    ///     Terminates the connection to the Revit application.
    ///     Frees associated resources and properly closes the interaction with the Revit environment.
    /// </summary>
    /// <remarks>
    ///     A call without an open connection has no effect.
    ///     The connection cannot be reopened in the same process.
    /// </remarks>
    protected internal static void TerminateRevitConnection()
    {
        lock (ConnectionLock)
        {
            if (_injector is null)
            {
                return;
            }

            _injector.EjectApplication();
            _injector = null;
            Application = null!;
        }
    }
}
