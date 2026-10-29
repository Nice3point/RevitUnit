using Nice3point.TUnit.Revit.Tests.Abstractions;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class FirstClassLifetimeUiTests : ClassLifetimeUiTest
{
    [Test]
    [Repeat(2)]
    public Task ClassHooks_AnotherUiClass_DoNotOverlap()
    {
        return AssertClassRunsAloneAsync();
    }
}
