using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Limiters;

/// <summary>
///     Represents the limit that restricts Revit UI tests to one concurrent execution.
/// </summary>
[PublicAPI]
public sealed class RevitUiParallelLimit : IParallelLimit
{
    /// <summary>
    ///     Gets the instance every registered Revit UI test shares.
    /// </summary>
    public static RevitUiParallelLimit Default { get; } = new();

    /// <inheritdoc />
    public int Limit => 1;
}
