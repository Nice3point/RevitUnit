using Nice3point.TUnit.Revit.Tests.Abstractions;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class FamilySampleTests : RevitFamilySampleTest
{
    [Test]
    [MethodDataSource(nameof(RevitFamilies))]
    public async Task FamilyDocument_ValidFamily_HasFamilyManager(string path)
    {
        // Arrange & Act
        var document = FamilyDocuments[path];

        // Assert
        await Assert.That(document.IsFamilyDocument).IsTrue();
        await Assert.That(document.FamilyManager).IsNotNull();
        await Assert.That(document.FamilyManager.Types.Size).IsGreaterThanOrEqualTo(1);
    }

    [Test]
    [MethodDataSource(nameof(RevitFamilies))]
    public async Task FilteredElementCollector_ElementTypes_ReturnsValidTypes(string path)
    {
        // Arrange
        var document = FamilyDocuments[path];

        // Act
        var elementTypes = new FilteredElementCollector(document)
            .WhereElementIsElementType()
            .ToElements();

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(elementTypes).IsNotEmpty();
            await Assert.That(elementTypes).All().Satisfy(source => source.IsAssignableTo<ElementType>());
        }
    }

    [Test]
    [MethodDataSource(nameof(RevitFamilies))]
    public async Task FilteredElementCollector_AllElements_ReturnsNonEmptyCollection(string path)
    {
        // Arrange
        var document = FamilyDocuments[path];

        //Act
        var allElements = new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .ToElements();

        // Assert
        await Assert.That(allElements).IsNotEmpty();
    }

    [Test]
    [MethodDataSource(nameof(RevitFamilies))]
    public async Task Document_Parameters_ContainsBuiltInParameters(string path)
    {
        // Arrange
        var document = FamilyDocuments[path];

        // Act
        var familyManager = document.FamilyManager;
        var parameters = familyManager.Parameters.Cast<FamilyParameter>().ToList();

        // Assert
        await Assert.That(parameters).IsNotEmpty();
    }

    [Test]
    [MethodDataSource(nameof(RevitFamilies))]
    public async Task Document_Units_HasValidFormatOptions(string path)
    {
        // Arrange
        var document = FamilyDocuments[path];

        // Act
        var units = document.GetUnits();

        // Assert
        await Assert.That(units).IsNotNull();
    }
}