# Nice3point.TUnit.Revit

Nice3point.TUnit.Revit is a Nuget package that runs TUnit tests inside a Revit process.
It owns the single thread that initializes the Revit API and marshals every test and hook onto it.
It adds only the Revit execution model on top of TUnit; TUnit provides the assertions, attributes, discovery, and data sources.

## Non-negotiables

* One thread owns the Revit API. Every API call runs on the thread that initialized Revit; the executor marshals test bodies and hooks onto it and caps Revit tests to one at a time. Never touch a Revit type off that thread, and never start a second thread or `Task.Run` for Revit work.
* Inject and eject in matched pairs, once per test host process. Revit activates once per process, and an IDE that reuses one test host process across runs starts a test session per run in that process. The first session that executes a test connects, and `RevitConnectionLifetime` releases the connection when the test application finishes. A process with an open Revit connection never terminates.
* Revit starts on the first test a session executes, never on discovery. Nothing that runs for a discovery request opens the connection.
* The package adds the Revit execution model only. Never reimplement what TUnit provides.
* Never break the public surface. Deprecate a renamed member with `[Obsolete]`, name the replacement, and keep the member functional.
* Every type compiles under every supported configuration.
* A change to the base classes or the executor ships with a test that exercises it on the Revit thread.
* Confirm an unfamiliar Revit, TUnit, or .NET API before use through official docs or `gh` (`gh api`, `gh search code`).
* A public-surface change updates `README.md`, `CHANGELOG.md`, and the XML docs in the same commit.

## Execution model

* The Revit thread is one process-wide background STA thread running a WPF `Dispatcher`. The dispatcher pumps the Win32 messages COM marshaling needs and routes `await` continuations back to the thread through `DispatcherSynchronizationContext`.
* `RevitThreadExecutor` unwraps the dispatcher operation (`operation.Task.Unwrap()`). The returned task completes only after the body and its `await` continuations finish.
* `RevitThreadExecutor` declares an `IParallelLimit` of `1` through `ITestRegisteredEventReceiver`, and TUnit applies it to every test the executor runs. The limit belongs to the executor; never reintroduce it as an attribute on the base class.
* `RevitUiThreadExecutor` declares `RevitUiParallelLimit`. TUnit groups tests by the type of their limit, and UI tests run one at a time in parallel with the tests of `RevitThreadExecutor`. A UI test outside Revit awaits its result without the Revit thread.
* The application connection opens in the `RevitSessionSetup` hook, a `Before(TestSession)` hook, and never lazily in the executor. TUnit creates test instances on pool threads, and a class that references a Revit API type loads that type off the Revit thread. An injection running at the same time fails with `Attempted to write protected memory`.
* `RevitApiUiTest` implements `ITestStartEventReceiver`. The receiver opens `RevitUiSession` and awaits the connection before TUnit starts the timer of the test, and the start of Revit is excluded from the duration of the first UI test.
* TUnit dispatches only `OnTestRegistered` to an executor. A receiver interface an executor implements is never called; a test receiver belongs on an attribute or on the test base class.
* Outside Revit the UI executor awaits the result `RevitUiSession` receives for the test, and inside Revit `RevitUiTestRunner` runs the tests of the `RevitUiTest` category and reports each result. `RevitConnectionLifetime` closes that session together with the application connection.
* `RevitConnectionLifetime` is an `ITestHostApplicationLifetime`. The package registers it through the `TestingPlatformBuilderHook` item of `build/Nice3point.TUnit.Revit.props`, packed into `build` and `buildTransitive`; a project that writes its own entry point calls `AddRevit` instead. The test project imports the same props, which a project reference does not deliver.

## Repository map

* `source/Nice3point.TUnit.Revit/` — the core testing framework, packed as a Nuget package.
* `tests/Nice3point.TUnit.Revit.Tests/` — the test project that tests the framework.
* `build/` — the ModularPipelines build.
* Root — `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `README.md`, `CHANGELOG.md`, CI workflows.

## Build and verify

* Build: `dotnet build -c Release.R##`, where the `R##` suffix is the Revit year (`R27` targets Revit 2027).
* Test: `dotnet test -c Release.R##`; requires a matching licensed Revit installation.
