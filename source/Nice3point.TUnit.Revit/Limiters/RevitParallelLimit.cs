using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Limiters;

/// <summary>
///     Represents the limit that restricts Revit tests to one concurrent execution.
/// </summary>
/// <remarks>
///     TUnit keys a limit by its type.
///     Every Revit test of a run shares this one.
/// </remarks>
internal sealed class RevitParallelLimit : IParallelLimit
{
    /// <summary>
    ///     Gets the instance every registered Revit test shares.
    /// </summary>
    public static RevitParallelLimit Default { get; } = new();

    /// <inheritdoc />
    public int Limit => 1;
}
