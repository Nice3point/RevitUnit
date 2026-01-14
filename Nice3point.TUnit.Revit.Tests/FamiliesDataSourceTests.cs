using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class FamiliesDataSourceTests : RevitApiTest
{
    private static string SamplesPath => $@"C:\Program Files\Autodesk\Revit {Application.VersionNumber}\Samples";

    [Before(Class)]
    public static void ValidateSamples()
    {
        if (!Directory.Exists(SamplesPath))
        {
            Skip.Test($"Samples folder not found at {SamplesPath}");
            return;
        }

        if (!Directory.EnumerateFiles(SamplesPath, "*.rfa").Any())
        {
            Skip.Test($"No .rfa files found in {SamplesPath}");
        }
    }

    public static IEnumerable<string> GetSampleFiles()
    {
        if (!Directory.Exists(SamplesPath))
        {
            yield return string.Empty;
            yield break;
        }

        foreach (var file in Directory.EnumerateFiles(SamplesPath, "*.rfa")) yield return file;
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task OpenDocument_ValidFamily_OpensSuccessfully(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            await Assert.That(document).IsNotNull();
            await Assert.That(document.IsValidObject).IsTrue();
            await Assert.That(document.PathName).IsEqualTo(filePath);
        }
        finally
        {
            document?.Close(false);
        }
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task FamilyDocument_ValidFamily_HasFamilyManager(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            await Assert.That(document.IsFamilyDocument).IsTrue();
            await Assert.That(document.FamilyManager).IsNotNull();
            await Assert.That(document.FamilyManager.Types.Size).IsGreaterThanOrEqualTo(1);
        }
        finally
        {
            document?.Close(false);
        }
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task FilteredElementCollector_ElementTypes_ReturnsValidTypes(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            var elementTypes = new FilteredElementCollector(document)
                .WhereElementIsElementType()
                .ToElements();

            using (Assert.Multiple())
            {
                await Assert.That(elementTypes).IsNotEmpty();
                await Assert.That(elementTypes).All().Satisfy(e => e.IsAssignableTo<ElementType>());
            }
        }
        finally
        {
            document?.Close(false);
        }
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task FilteredElementCollector_AllElements_ReturnsNonEmptyCollection(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            var allElements = new FilteredElementCollector(document)
                .WhereElementIsNotElementType()
                .ToElements();

            await Assert.That(allElements).IsNotEmpty();
        }
        finally
        {
            document?.Close(false);
        }
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task Document_Parameters_ContainsBuiltInParameters(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            var familyManager = document.FamilyManager;
            var parameters = familyManager.Parameters.Cast<FamilyParameter>().ToList();

            await Assert.That(parameters).IsNotEmpty();
        }
        finally
        {
            document?.Close(false);
        }
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    [MethodDataSource(nameof(GetSampleFiles))]
    public async Task Document_Units_HasValidFormatOptions(string filePath)
    {
        Document? document = null;

        try
        {
            document = Application.OpenDocumentFile(filePath);

            var units = document.GetUnits();

            await Assert.That(units).IsNotNull();
        }
        finally
        {
            document?.Close(false);
        }
    }
}