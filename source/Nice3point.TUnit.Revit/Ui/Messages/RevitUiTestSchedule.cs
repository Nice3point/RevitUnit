namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Represents the command that schedules the UI tests of the run.
/// </summary>
/// <remarks>
///     The run executes the scheduled tests, and only them.
/// </remarks>
[PublicAPI]
internal sealed record RevitUiTestSchedule : RevitUiTestCommand
{
    /// <summary>
    ///     Gets the identifiers of the scheduled tests.
    /// </summary>
    public required IReadOnlyList<string> TestIds { get; init; }
}
