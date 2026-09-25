using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Limiters;

/// <summary>
///     Represents the limit that restricts Revit UI tests to one concurrent execution.
/// </summary>
/// <remarks>
///     TUnit keys a limit by its type.
///     Every Revit UI test of a run shares this one.
/// </remarks>
internal sealed class RevitUiParallelLimit : IParallelLimit
{
    /// <summary>
    ///     Gets the instance every registered Revit UI test shares.
    /// </summary>
    public static RevitUiParallelLimit Default { get; } = new();

    /// <inheritdoc />
    public int Limit => 1;
}
