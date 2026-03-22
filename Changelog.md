# 2027.0.0-preview.3.20260322

This release adds support for Revit 2027, testing for different languages and custom Revit installation path.

## Localization support

TUnit initializes Revit with the `English - United States` language. To override these defaults, use assembly-level attributes:

- Add the attributes to any .cs file in your project (e.g., TestsConfiguration.cs):

    ```csharp
    using Nice3point.Revit.Injector.Attributes;
    
    [assembly: RevitLanguage("ENG")]
    ```

- Add the attributes directly to your .csproj file:

    ```xml
    <!-- Revit Environment Configuration -->
    <ItemGroup>
        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitLanguageAttribute">
            <_Parameter1>ENG</_Parameter1>
        </AssemblyAttribute>
    </ItemGroup>
    ```

The `RevitLanguage` attribute accepts a [language](https://help.autodesk.com/view/RVT/2026/ENU/?guid=GUID-BD09C1B4-5520-475D-BE7E-773642EEBD6C) name (e.g., "English - United States"), code (e.g., "ENU")
or [LanguageType](https://www.revitapidocs.com/2026/dfda33cf-cbff-9fde-6672-38402e87510f.htm) enum value (e.g., "English_GB" or "15").

## Custom Revit installation path

TUnit initializes Revit from `C:\Program Files\Autodesk\Revit {version}` installation path. To override these defaults, use assembly-level attributes:

- Add the attributes to any .cs file in your project (e.g., TestsConfiguration.cs):

    ```csharp
    using Nice3point.Revit.Injector.Attributes;
    
    [assembly: RevitInstallationPath("D:\Autodesk\Revit Preview")]
    ```

- Add the attributes directly to your .csproj file:

    ```xml
    <!-- Revit Environment Configuration -->
    <ItemGroup>
        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitInstallationPathAttribute">
            <_Parameter1>D:\Autodesk\Revit $(RevitVersion)</_Parameter1>
        </AssemblyAttribute>
    </ItemGroup>
    ```

## Enhancements

- Fix ExecutionContext to capture AsyncLocal values like TestContext
- Add new samples

# 2026.0.4

- Fix ExecutionContext to capture AsyncLocal values like TestContext
- Update Readme to include Global TestContext registration example

# 2026.0.3

- Changed the Revit initialization lifecycle. Now Revit is initialized [before TestDiscovery](https://tunit.dev/docs/test-lifecycle/lifecycle-overview) to support Data sources.
- Added new MethodDataSource samples based on Revit runtime values.

# 2026.0.2

Initial public release. Enjoy!

# 2026.0.1

Enable private Nuget source for testing

# 2026.0.0

Initial release. Enjoy!