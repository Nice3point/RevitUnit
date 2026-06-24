using Nice3point.Revit.Injector;
using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit.Tests.Abstractions;

public abstract class RevitSampleTest : RevitApiTest
{
    private string? _isolatedSamplePath;

    protected RevitSampleTest(string extension, string? samplesDirectory = null)
    {
        var path = samplesDirectory ?? $@"C:\Program Files\Autodesk\Revit {RevitEnvironment.MajorVersion}\Samples";
        DocumentPaths = Directory.Exists(path)
            ? Directory.EnumerateFiles(path, $"*{extension}").ToArray()
            : [];
    }

    public string[] DocumentPaths { get; }
    public Document? Document { get; private set; }

    [After(Test)]
    [HookExecutor<RevitThreadExecutor>]
    public void CloseDocument()
    {
        Document?.Close(false);
        if (_isolatedSamplePath is null) return;
        
        File.SetAttributes(_isolatedSamplePath, FileAttributes.Normal);
        File.Delete(_isolatedSamplePath);
    }

    protected Document OpenDocument(string path)
    {
        var extension = Path.GetExtension(path);
        _isolatedSamplePath = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}{extension}");
        File.Copy(path, _isolatedSamplePath);

        using (RevitApiContext.BeginFailureSuppressionScope())
        {
            Document = Application.OpenDocumentFile(_isolatedSamplePath);
        }

        return Document;
    }
}