using Autodesk.Revit.DB;
using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class RevitDocumentTest : RevitApiTest
{
    private static Document _documentFile = null!;

    [Before(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Setup()
    {
        _documentFile = Application.OpenDocumentFile($@"C:\Program Files\Autodesk\Revit {Application.VersionNumber}\Samples\rac_basic_sample_family.rfa");
    }

    [After(Class)]
    [HookExecutor<RevitThreadExecutor>]
    public static void Cleanup()
    {
        _documentFile.Close(false);
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task ModelIsNotEmpty()
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
    public async Task ModelTitleIsNotEmpty()
    {
        await Assert.That(_documentFile.Title).IsNotEmpty();
    }
}