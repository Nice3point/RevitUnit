using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class RevitDocumentTests : RevitApiTest
{
    private static Document? _documentFile;

    [Before(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Setup()
    {
        var samplePath = $@"C:\Program Files\Autodesk\Revit {Application.VersionNumber}\Samples\rac_basic_sample_family.rfa";
        if (!File.Exists(samplePath))
        {
            Skip.Test("No sample family found");
            return;
        }

        _documentFile = Application.OpenDocumentFile(samplePath);
    }

    [After(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Cleanup()
    {
        _documentFile?.Close(false);
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task FilteredElementCollector_ElementTypes_ValidAssignable()
    {
        var elements = new FilteredElementCollector(_documentFile)
            .WhereElementIsElementType()
            .ToElements();

        using (Assert.Multiple())
        {
            await Assert.That(elements).IsNotEmpty();
            await Assert.That(elements).All().Satisfy(element => element.IsAssignableTo<ElementType>());
        }
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [DependsOn(nameof(FilteredElementCollector_ElementTypes_ValidAssignable))]
    public async Task Delete_Dimensions_ElementsWithDependenciesDeleted()
    {
        var elementIds = new FilteredElementCollector(_documentFile)
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

        using var transaction = new Transaction(_documentFile);
        transaction.Start("Delete dimensions");
        var deletedElements = _documentFile!.Delete(elementIds);
        transaction.Commit();

        await Assert.That(deletedElements.Count).IsGreaterThanOrEqualTo(elementIds.Count);
    }
}