namespace Nice3point.TUnit.Revit.Ui;

/// <summary>
///     Represents a UI test failure inside the Revit user interface application.
/// </summary>
/// <param name="message">The failure message of the test.</param>
/// <param name="revitStackTrace">The stack trace of the failure inside Revit, or <see langword="null" /> when none applies.</param>
/// <remarks>
///     The exception reports the stack trace of the failure inside Revit as its own.
/// </remarks>
internal sealed class RevitUiTestException(string message, string? revitStackTrace) : Exception(message)
{
    /// <inheritdoc />
    public override string? StackTrace => revitStackTrace ?? base.StackTrace;
}
