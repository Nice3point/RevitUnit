namespace Nice3point.TUnit.Revit.Ui;

/// <summary>
///     Represents a timeout error reported by a Revit UI test.
/// </summary>
/// <param name="message">The timeout message of the test.</param>
/// <param name="revitStackTrace">The stack trace of the timeout inside Revit, or <see langword="null" /> when none applies.</param>
internal sealed class RevitUiTimeoutException(string message, string? revitStackTrace) : TimeoutException(message)
{
    /// <inheritdoc />
    public override string? StackTrace => revitStackTrace ?? base.StackTrace;
}
