# Testing

This project is a testing framework, so its own tests are the proof that the framework works. The test project dogfoods the framework: it inherits the base classes, runs on the Revit thread through the executor, and so doubles as the reference for how a consumer writes Revit tests. Every change ships with tests.

## What to Test

* **The framework's behavior.** A change to a base class, the lifecycle hooks, or the executor ships with a test that exercises it. Confirm the `Application` connects, the executor runs the body on the Revit thread, and the test platform context flows through.
* **Edge cases:** an executor other than the Revit one, a hook that never connected, a missing samples folder.
* **No UI tests.** The framework injects Revit headless. Skip anything that needs the Revit window.

## Framework

* **TUnit** on the Microsoft.Testing.Platform. Assertions use the TUnit API: `await Assert.That(actual).IsNotNull()`.
* A test inherits `RevitApiTest` and writes against the `Application` property.
* `[assembly: TestExecutor<RevitThreadExecutor>]` registers the Revit executor for every test in the assembly, so individual tests omit the per-test attribute. A test that needs a different executor overrides it with `[TestExecutor<...>]`.
* A hook that touches Revit carries `[HookExecutor<RevitThreadExecutor>]` so it runs on the Revit thread.

## Execution Model

The test process injects a real Revit application and runs every marked test and hook on the single Revit thread.

* The session-setup hook injects Revit, the session-cleanup hook ejects it.
* A per-test document is created in a `[Before(Test)]` hook and closed in the matching `[After(Test)]`, so a test never inherits another test's state. Read-only tests share state through a `[Before(Class)]` hook instead.
* Sample-driven tests open the installed Revit `Samples` files. When the folder is missing the sample set is empty, so guard the test to skip cleanly rather than fail.

## Structure

Split each test into blocks marked with `// Arrange`, `// Act`, and `// Assert` comments. Merge the labels when a step is trivial, as `// Arrange & Act`.

```csharp
public sealed class ApplicationTests : RevitApiTest
{
    [Test]
    public async Task Create_XYZ_ValidDistance()
    {
        // Arrange & Act
        var point = Application.Create.NewXYZ(3, 4, 5);

        // Assert
        await Assert.That(point.DistanceTo(XYZ.Zero)).IsEqualTo(7).Within(0.1);
    }
}
```

## Version Coverage

* Tests build per Revit configuration (`Release.RNN`). Prefer the latest supported configuration unless the change is version-specific.
* When changing version-specific behavior, run or document coverage for each affected configuration the project declares.

## Build and Test

TUnit runs on the Microsoft.Testing.Platform, so `dotnet test` runs the suite directly. The configuration carries the target Revit version, for example `dotnet test -c Release.R26`. A licensed Revit install matching the configuration must be present, because the tests run against a real Revit process.
