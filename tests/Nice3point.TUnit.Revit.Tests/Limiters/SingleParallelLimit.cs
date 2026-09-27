using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Tests.Limiters;

[UsedImplicitly]
public sealed class SingleParallelLimit : IParallelLimit
{
    public int Limit => 1;
}
