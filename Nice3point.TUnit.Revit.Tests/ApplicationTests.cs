namespace Nice3point.TUnit.Revit.Tests;

public sealed class ApplicationTests : RevitApiTest
{
    [Test]
    public async Task Cities_BuiltinSet_IsNotEmpty()
    {
        // Arrange & Act
        var cities = Application.Cities.Cast<City>();

        // Assert
        await Assert.That(cities).IsNotEmpty();
    }

    [Test]
    public async Task Create_XYZ_ValidDistance()
    {
        // Arrange & Act
        var point = Application.Create.NewXYZ(3, 4, 5);

        // Assert
        await Assert.That(point.DistanceTo(XYZ.Zero)).IsEqualTo(7).Within(0.1);
    }
}