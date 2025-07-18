using Autodesk.Revit.ApplicationServices;
using Nice3point.Revit.Injector;

namespace Nice3point.TUnit.Revit;

/// <summary>
/// Represents a base class for testing Revit-based applications inside the test framework.
/// This class provides methods to initialize and terminate the connection with the Revit application.
/// </summary>
public class RevitApplicationTest
{
    private static Injector? _injector;

    /// <summary>
    ///     Represents the database level Autodesk Revit Application, providing access to documents, options and other application wide data and settings.
    /// </summary>
    protected static Application Application { get; private set; } = null!;

    /// <summary>
    /// Initializes the connection to the Revit application.
    /// </summary>
    protected static void InitializeRevitConnection()
    {
        _injector = new Injector();
        Application = _injector.InjectApplication();
    }

    /// <summary>
    /// Terminates the connection to the Revit application.
    /// Frees associated resources and properly closes the interaction with the Revit environment.
    /// </summary>
    protected static void TerminateRevitConnection()
    {
        _injector!.EjectApplication();
    }
}