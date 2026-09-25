namespace Nice3point.TUnit.Revit.Ui.Messages;

/// <summary>
///     Determines the final state of a UI test.
/// </summary>
internal enum RevitUiTestStatus
{
    /// <summary>
    ///     Reports the test as passed.
    /// </summary>
    Passed,

    /// <summary>
    ///     Reports the test as failed.
    /// </summary>
    Failed,

    /// <summary>
    ///     Reports the test as skipped.
    /// </summary>
    Skipped
}
