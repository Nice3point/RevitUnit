using Nice3point.TUnit.Revit.Tests.Abstractions;

namespace Nice3point.TUnit.Revit.Tests;

//Shared tests for FamilySampleTests and ModelSampleTests
public abstract class RevitSampleTests(string extension, string? samplesDirectory = null) : RevitSampleTest(extension, samplesDirectory)
{
    [Test]
    [InstanceMethodDataSource(nameof(DocumentPaths))]
    public async Task FilteredElementCollector_ElementTypes_ValidAssignable(string path)
    {
        // Arrange
        var document = OpenDocument(path);

        // Act
        var elements = new FilteredElementCollector(document)
            .WhereElementIsElementType()
            .ToElements();

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(elements).IsNotEmpty();
            await Assert.That(elements).All().Satisfy(element => element.IsAssignableTo<ElementType>());
        }
    }

    [Test]
    [InstanceMethodDataSource(nameof(DocumentPaths))]
    public async Task FilteredElementCollector_ElementInstances_ReturnsNonEmptyCollection(string path)
    {
        // Arrange
        var document = OpenDocument(path);

        // Act
        var elements = new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .ToElements();

        // Assert
        await Assert.That(elements).IsNotEmpty();
    }
}

[InheritsTests]
public sealed class FamilySampleTests() : RevitSampleTests(".rfa")
{
    [Test]
    [InstanceMethodDataSource(nameof(DocumentPaths))]
    public async Task FamilyDocument_ValidFamily_HasFamilyManager(string path)
    {
        // Arrange & Act
        var document = OpenDocument(path);

        // Assert
        await Assert.That(document.IsFamilyDocument).IsTrue();
        await Assert.That(document.FamilyManager).IsNotNull();
        await Assert.That(document.FamilyManager.Types.Size).IsGreaterThanOrEqualTo(1);
    }

    [Test]
    [InstanceMethodDataSource(nameof(DocumentPaths))]
    public async Task Document_Parameters_ContainsBuiltInParameters(string path)
    {
        // Arrange
        var document = OpenDocument(path);

        // Act
        var familyManager = document.FamilyManager;
        var parameters = familyManager.Parameters.Cast<FamilyParameter>().ToList();

        // Assert
        await Assert.That(parameters).IsNotEmpty();
    }

    [Test]
    [InstanceMethodDataSource(nameof(DocumentPaths))]
    public async Task Document_Units_HasValidFormatOptions(string path)
    {
        // Arrange
        var document = OpenDocument(path);

        // Act
        var units = document.GetUnits();

        // Assert
        await Assert.That(units).IsNotNull();
    }
}

[InheritsTests]
public sealed class ModelSampleTests() : RevitSampleTests(".rvt")
{
    [Test]
    [InstanceMethodDataSource(nameof(DocumentPaths))]
    public async Task Delete_Dimensions_ElementsWithDependenciesDeleted(string path)
    {
        // Arrange
        var document = OpenDocument(path);
        var elementIds = new FilteredElementCollector(document)
            .WhereElementIsNotElementType()
            .OfCategory(BuiltInCategory.OST_Dimensions)
#if REVIT2025_OR_GREATER
            .OfClass(typeof(RadialDimension))
            .ToElementIds();
#else
            .Cast<Dimension>()
            .Where(dimension => dimension.DimensionShape == DimensionShape.Radial)
            .Select(dimension => dimension.Id)
            .ToList();
#endif

        // Act
        using var transaction = new Transaction(document);
        transaction.Start("Delete dimensions");
        var deletedElements = document.Delete(elementIds);
        transaction.Commit();

        // Assert
        await Assert.That(deletedElements.Count).IsGreaterThanOrEqualTo(elementIds.Count);
    }
}
