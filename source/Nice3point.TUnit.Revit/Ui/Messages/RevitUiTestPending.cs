namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Represents the event that a UI test waits for the test host to execute it.
/// </summary>
[PublicAPI]
internal sealed record RevitUiTestPending : RevitUiTestEvent;
