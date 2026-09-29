namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Represents the command that skips pending UI tests and finishes the session inside Revit.
/// </summary>
[PublicAPI]
internal sealed record RevitUiTestSessionEnd : RevitUiTestCommand;
