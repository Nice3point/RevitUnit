namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Represents the command that executes one pending UI test.
/// </summary>
[PublicAPI]
internal sealed record RevitUiTestExecution : RevitUiTestCommand
{
    /// <summary>
    ///     Gets the identifier of the test to execute.
    /// </summary>
    public required string TestId { get; init; }
}
