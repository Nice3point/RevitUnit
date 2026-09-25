using Nice3point.Revit.Injector.Ui;

namespace Nice3point.TUnit.Revit.Ui;

/// <summary>
///     Provides the state the UI test executor shares with the UI test runner.
/// </summary>
internal static class RevitUiRuntime
{
    /// <summary>
    ///     The category that marks a UI test.
    /// </summary>
    public const string Category = "RevitUiTest";

    /// <summary>
    ///     Gets or sets the context of the Revit user interface application.
    /// </summary>
    /// <value>Defaults to <see langword="null" /> outside the Revit user interface application.</value>
    public static RevitUiContext? Context { get; set; }

    /// <summary>
    ///     Gets a value indicating whether the current run executes inside the Revit user interface application.
    /// </summary>
    public static bool InRevitProcess => Context is not null;
}
