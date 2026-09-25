<p align="center">
    <picture>
        <source media="(prefers-color-scheme: dark)" width="610" srcset="https://github.com/user-attachments/assets/e539b207-f469-46ed-8543-5839146761ec">
        <img alt="RevitUnit" width="610" src="https://github.com/user-attachments/assets/297430e2-7f9c-498a-8ae5-dfd48caae621">
    </picture>
</p>

## Testing Framework for Revit

[![Nuget](https://img.shields.io/nuget/vpre/Nice3point.TUnit.Revit?style=for-the-badge&color=1A1A1A&labelColor=C42A2A)](https://www.nuget.org/packages/Nice3point.TUnit.Revit)
[![Downloads](https://img.shields.io/nuget/dt/Nice3point.TUnit.Revit?style=for-the-badge&color=1A1A1A&labelColor=C42A2A)](https://www.nuget.org/packages/Nice3point.TUnit.Revit)
[![Last Commit](https://img.shields.io/github/last-commit/Nice3point/RevitUnit/develop?style=for-the-badge&color=1A1A1A&labelColor=C42A2A)](https://github.com/Nice3point/RevitUnit/commits/develop)

Write unit tests for your Revit add-ins using the [TUnit](https://github.com/thomhurst/TUnit) testing framework with source-generated tests,
parallel execution, and Microsoft.Testing.Platform support.

## Installation

You can install this library as a [NuGet package](https://www.nuget.org/packages/Nice3point.TUnit.Revit).

The packages are compiled for specific versions of Revit. To support different versions of libraries in one project, use the `RevitVersion` property:

```xml
<PackageReference Include="Nice3point.TUnit.Revit" Version="$(RevitVersion).*"/>
```

## Writing your first test

Start by creating a new class inheriting from `RevitApiTest`:

```csharp
public class MyTestClass : RevitApiTest
{

}
```

Add a method with the `[Test]` attribute:

```csharp
public class MyTestClass : RevitApiTest
{
    [Test]
    public async Task MyTest()
    {

    }
}
```

This is your runnable test. `RevitApiTest` runs every test and hook of the class within Revit's single-threaded API context.

## Running your tests

**TUnit** is built on top of the **Microsoft.Testing.Platform**. Combined with source-generated tests, running your tests is available in multiple ways.

### dotnet run

For simple project execution, `dotnet run` is the preferred method, allowing easier command line flag passing.

```bash
cd 'C:/Your/Test/Directory'
dotnet run -c "Release.R26"
```

### dotnet test

`dotnet test` requires the configuration to target the desired Revit version.

```shell
cd 'C:/Your/Test/Directory'
dotnet test -c "Release.R26"
```

> [!IMPORTANT]
> You must have a licensed copy of Autodesk Revit installed on your machine to run tests, with a version that matches the selected Solution configuration.

### JetBrains Rider

The [Enable Testing Platform support option](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro?tabs=dotnetcli) must be selected in
Settings > Build, Execution, Deployment > Unit Testing > Testing Platform.

![](https://github.com/user-attachments/assets/d64c58f6-9223-4bdb-a513-c663daf4e0c1)

## Application testing

Test Revit application-level functionality using the `Application` property exposed by `RevitApiTest`:

```csharp
public sealed class ApplicationTests : RevitApiTest
{
    [Test]
    public async Task Cities_BuiltinSet_IsNotEmpty()
    {
        var cities = Application.Cities.Cast<City>();

        await Assert.That(cities).IsNotEmpty();
    }

    [Test]
    public async Task Create_XYZ_ValidDistance()
    {
        var point = Application.Create.NewXYZ(3, 4, 5);

        await Assert.That(point.DistanceTo(XYZ.Zero)).IsEqualTo(7).Within(0.1);
    }
}
```

## Document testing

Tests that pass alone but fail together are a classic sign of shared state. Give each test its own document — created in `[Before(Test)]`, closed in `[After(Test)]` — and that problem disappears entirely.
Use the setup hook to seed the document with exactly the state each test needs. Feel free to use `[Before(Class)]` hook for read-only tests.

```csharp
public sealed class ModelSeedTests : RevitApiTest
{
    private Document _document = null!;
    private IList<Wall> _exteriorWalls = null!;

    [Before(Test)]
    public void SeedModel()
    {
        _document = Application.NewProjectDocument(UnitSystem.Metric);

        using var transaction = new Transaction(_document, "Seed model");
        transaction.Start();

        _exteriorWalls =
        [
            Wall.Create(_document, Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)), level.Id, false),
            Wall.Create(_document, Line.CreateBound(new XYZ(10, 0, 0), new XYZ(10, 6, 0)), level.Id, false),
            Wall.Create(_document, Line.CreateBound(new XYZ(10, 6, 0), new XYZ(0, 6, 0)), level.Id, false),
            Wall.Create(_document, Line.CreateBound(new XYZ(0, 6, 0), new XYZ(0, 0, 0)), level.Id, false),
        ];

        transaction.Commit();
    }

    [After(Test)]
    public void CloseModel()
    {
        _document.Close(false);
    }

    [Test]
    public async Task FilteredElementCollector_ExteriorWalls_MatchSeededCount()
    {
        var walls = new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Wall))
            .ToList();

        await Assert.That(walls.Count).IsEqualTo(_exteriorWalls.Count);
    }

    [Test]
    public async Task Transaction_DemolishWall_RemainingWallCountDecreases()
    {
        var targetId = _exteriorWalls[0].Id;

        using var transaction = new Transaction(_document, "Demolish wall");
        transaction.Start();
        _document.Delete(targetId);
        transaction.Commit();

        var remainingWalls = new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Wall))
            .ToElementIds();

        await Assert.That(remainingWalls.Count).IsEqualTo(_exteriorWalls.Count - 1);
    }
}
```

## User interface testing

Test the functionality that needs the Revit user interface by deriving from `RevitApiUiTest`.
The class exposes `UiApplication`, and runs every test body and hook on the Revit thread inside a Revit API context:

```csharp
public sealed class UiDocumentTests : RevitApiUiTest
{
    private UIDocument _uiDocument = null!;

    [Before(Test)]
    public void OpenModel()
    {
        _uiDocument = UiApplication.OpenAndActivateDocument(modelPath);
    }

    [Test]
    public async Task SetElementIds_ActiveDocument_SelectsTheLevels()
    {
        var levelIds = uiDocument.Document.CollectElements()
            .OfClass<Level>()
            .ToElementIds();

        _uiDocument.Selection.SetElementIds(levelIds);

        await Assert.That(_uiDocument.Selection.GetElementIds()).IsEquivalentTo(levelIds);
    }

    [Test]
    public async Task GetOpenUIViews_ActiveDocument_ContainsTheActiveView()
    {
        var openViewIds = uiDocument.GetOpenUIViews()
            .Select(uiView => uiView.ViewId)
            .ToList();

        await Assert.That(openViewIds).Contains(_uiDocument.ActiveView.Id);
    }

    [Test]
    public async Task CanPostCommand_Default3DView_IsPostable()
    {
        var commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.Default3DView);

        await Assert.That(UiApplication.CanPostCommand(commandId)).IsTrue();
    }
}
```

> [!NOTE]
> The examples demonstrate basic testing functionality. This library **only adds support for working within the Revit API context**. For comprehensive documentation on assertions, attributes, test configuration, and
> advanced features, please refer to the official [TUnit documentation](https://thomhurst.github.io/TUnit/).

More examples, including parametrized model and family tests, are available in the [test project](https://github.com/Nice3point/RevitUnit/tree/main/Nice3point.TUnit.Revit.Tests).

## Test configuration

### Revit Environment

TUnit initializes Revit with the `English - United States` language and the `C:\Program Files\Autodesk\Revit {version}` installation path. To override these defaults:

- Add the assembly-level attributes to any .cs file in your project (e.g., TestsConfiguration.cs):

    ```csharp
    using Nice3point.Revit.Injector.Attributes;

    [assembly: RevitLanguage("ENU")]
    [assembly: RevitInstallationPath("D:\Autodesk\Revit Preview")]
    ```

- Or add the attributes directly to your .csproj file:

    ```xml
    <!-- Revit Environment Configuration -->
    <ItemGroup>

        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitLanguageAttribute">
            <_Parameter1>ENU</_Parameter1>
        </AssemblyAttribute>

        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitInstallationPathAttribute">
            <_Parameter1>D:\Autodesk\Revit $(RevitVersion)</_Parameter1>
        </AssemblyAttribute>

    </ItemGroup>
    ```

The `RevitLanguage` attribute accepts a [language](https://help.autodesk.com/view/RVT/2026/ENU/?guid=GUID-BD09C1B4-5520-475D-BE7E-773642EEBD6C) name (e.g., "English - United States"), code (e.g., "ENU")
or [LanguageType](https://www.revitapidocs.com/2026/dfda33cf-cbff-9fde-6672-38402e87510f.htm) enum value (e.g., "English_GB" or "15").
