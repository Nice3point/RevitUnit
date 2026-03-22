using Nice3point.TUnit.Revit.Tests.Abstractions;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class ModelSampleTests : RevitModelSampleTest
{
    [Test]
    [MethodDataSource(nameof(RevitModels))]
    public async Task FilteredElementCollector_ElementTypes_ValidAssignable(string path)
    {
        // Arrange & Act
        var document = ModelDocuments[path];
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
    [MethodDataSource(nameof(RevitModels))]
    public async Task Delete_Dimensions_ElementsWithDependenciesDeleted(string path)
    {
        // Arrange
        var document = ModelDocuments[path];
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