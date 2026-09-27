namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Represents the command that cancels one executing UI test.
/// </summary>
[PublicAPI]
internal sealed record RevitUiTestCancellation : RevitUiTestCommand
{
    /// <summary>
    ///     Gets the identifier of the test to cancel.
    /// </summary>
    public required string TestId { get; init; }
}
