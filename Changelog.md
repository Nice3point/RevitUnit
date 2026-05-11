# 2027.0.1

- Updated TUnit to 1.44
- Reworked `RevitThreadExecutor`, fixing crashes and thread-affinity issues
- TUnit 1.33 changed the order in which test classes are initialised: instance field initialisers now run before the Revit session hook.
  Field initialisers that previously worked are no longer safe, because they load Revit API types before the application is injected.
  Rewrite them lazily https://github.com/Nice3point/RevitUnit/commit/c92b6b99855c2a14bd0dff5cf9c6ccb7802c8ebd#diff-6f718c2ac5a94cce0c4a11e9d8c457891d449eb7b32ba49e8b7595d0a2b42cdb.

  Before:

  ```csharp
  private protected Dictionary<string, Document> Documents { get; } = [];
  ```

  After:

  ```csharp
  private protected Dictionary<string, Document> Documents => field ??= new();
  ```

# 2027.0.0

This release adds support for Revit 2027, testing for different languages and custom Revit installation path.

## Localization support

TUnit initializes Revit with the `English - United States` language. To override these defaults, use assembly-level attributes:

- Add the attributes to any .cs file in your project (e.g., TestsConfiguration.cs):

    ```csharp
    using Nice3point.Revit.Injector.Attributes;
    
    [assembly: RevitLanguage("ENU")]
    ```

- Add the attributes directly to your .csproj file:

    ```xml
    <!-- Revit Environment Configuration -->
    <ItemGroup>
        <AssemblyAttribute Include="Nice3point.Revit.Injector.Attributes.RevitLanguageAttribute">
            <_Parameter1>ENU</_Parameter1>
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

- Added new samples
- Fixed ExecutionContext to capture AsyncLocal values like TestContext

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