<p align="center">
    <picture>
        <source media="(prefers-color-scheme: dark)" width="610" srcset="https://github.com/user-attachments/assets/e539b207-f469-46ed-8543-5839146761ec">
        <img alt="RevitUnit" width="610" src="https://github.com/user-attachments/assets/297430e2-7f9c-498a-8ae5-dfd48caae621">
    </picture>
</p>

## Unit testing framework for Revit API

This library provides a testing shell for Revit add-ins using a Microsoft testing platform.

## Installation

You can install the Toolkit as a [NuGet package](https://www.nuget.org/packages/Nice3point.TUnit.Revit).

The packages are compiled for specific versions of Revit. To support different versions of libraries in one project, use the `RevitVersion` property:

```xml

<PackageReference Include="Nice3point.TUnit.Revit" Version="$(RevitVersion).*"/>
```

> [!WARNING]   
> The public version of the package does not contain implementation for the framework.
> An open source version is not currently planned due to Autodesk export regulations.

## Writing your first test

Start by creating a new class inheriting from `RevitApiTest`:

```csharp
public class MyTestClass : RevitApiTest
{

}
```

Add a method with `[Test]` and `[TestExecutor<RevitThreadExecutor>]` attributes:

```csharp
public class MyTestClass : RevitApiTest
{
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task MyTest()
    {
        
    }
}
```

This is your runnable test. The `[TestExecutor<RevitThreadExecutor>]` attribute ensures the test executes within Revit's single-threaded API context.

## Running your tests

This library uses TUnit, which is built on top of the Microsoft.Testing.Platform. Combined with source-generated tests, running your tests is available in multiple ways.

### dotnet run

For simple project execution, `dotnet run` is the preferred method, allowing easier command line flag passing.

```bash
cd 'C:/Your/Test/Directory'
dotnet run -c "Release R26"
```

### dotnet test

`dotnet test` requires the configuration to target the desired Revit version.

```shell
cd 'C:/Your/Test/Directory'
dotnet test -c "Release R26"
```

### dotnet exec

If your test project has already been built, use `dotnet exec` or `dotnet` with the .dll path:

```shell
cd 'C:/Your/Test/Directory/bin/Release R26/'
dotnet exec YourTestProject.dll
```

or

```shell
cd 'C:/Your/Test/Directory/bin/Release R26/'
dotnet YourTestProject.dll
```

## Application testing

Test Revit application-level functionality:

```csharp
public sealed class RevitApplicationTests : RevitApiTest
{
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task Documents_Startup_IsEmpty()
    {
        var documents = Application.Documents.Cast<Document>();
        
        await Assert.That(documents).IsEmpty();
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task Create_XYZ_ValidDistance()
    {
        var point = Application.Create.NewXYZ(3, 4, 5);
        
        await Assert.That(point.DistanceTo(XYZ.Zero)).IsEqualTo(7).Within(0.1);
    }
}
```

## Document testing

Test document-specific operations with setup and cleanup:

```csharp
public sealed class RevitDocumentTests : RevitApiTest
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
    [NotInParallel]
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
    [NotInParallel]
    [TestExecutor<RevitThreadExecutor>]
    public async Task Delete_Dimensions_ElementsWithDependenciesDeleted()
    {
        var elementIds = new FilteredElementCollector(_documentFile)
            .WhereElementIsNotElementType()
            .OfCategory(BuiltInCategory.OST_Dimensions)
            .OfClass(typeof(RadialDimension))
            .ToElementIds();

        using var transaction = new Transaction(_documentFile);
        transaction.Start("Delete dimensions");
        var deletedElements = _documentFile.Delete(elementIds);
        transaction.Commit();

        await Assert.That(deletedElements.Count).IsGreaterThanOrEqualTo(elementIds.Count);
    }
}
```