using Nice3point.TUnit.Revit.Tests.Abstractions;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class FirstClassLifetimeUiTests : ClassLifetimeUiTest
{
    [Test]
    [Repeat(2)]
    public async Task ClassHooks_AnotherUiClass_DoNotOverlap()
    {
        // Arrange & Act
        var openClass = OpenClass;
        var classOpenAtSetup = ClassOpenAtSetup;

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(classOpenAtSetup).IsNull();
            await Assert.That(openClass).IsEqualTo(typeof(FirstClassLifetimeUiTests));
        }
    }
}
