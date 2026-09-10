# Nice3point.TUnit.Revit

Nice3point.TUnit.Revit is a Nuget package that runs TUnit tests inside a Revit process.
It owns the single thread that initializes the Revit API, marshals every test and hook onto it.
It adds only the Revit execution model on top of TUnit; assertions, attributes, discovery, and data sources stay with TUnit.

## Non-negotiables

* One thread owns the Revit API. Every API call runs on the thread that initialized Revit; the executor marshals test bodies and hooks onto it and declares a cap of one Revit test at a time. Never touch a Revit type off that thread, and never start a second thread or `Task.Run` for Revit work.
* Inject and eject in matched pairs, once per test host process. Revit activates once per process, and a host such as Visual Studio Test Explorer runs a test session per run inside one process. The first session that executes a test connects, and `RevitConnectionLifetime` releases when the test application finishes. A process that leaves Revit connected never terminates.
* Revit starts on the first test a session executes, never on discovery. Nothing that runs for a discovery request opens the connection. An IDE that lists the tests of an assembly leaves Revit unstarted.
* The package adds the Revit execution model only. It exposes the base classes, the executor, and the injection lifecycle; assertions, attributes, and discovery come from TUnit. Never reimplement what TUnit provides.
* Never break the public surface. Deprecate a renamed member with `[Obsolete]`, name the replacement, and keep the member functional.
* Mark a member the test platform invokes but consumers must not call `[EditorBrowsable(EditorBrowsableState.Never)]`.
* Every type compiles under every supported configuration.
* A change to the base classes or the executor ships with a test that exercises it on the Revit thread.
* Confirm an unfamiliar Revit, TUnit, or .NET API before use through official docs or `gh` (`gh api`, `gh search code`).
* A public-surface change updates `README.md`, `CHANGELOG.md`, and the XML docs in the same commit.

## Execution model

* A process-wide singleton starts one background STA thread and runs a WPF `Dispatcher` on it. The dispatcher pumps the Win32 messages COM marshaling needs and routes `await` continuations through `DispatcherSynchronizationContext`.
* `RevitThreadExecutor` is the public entry point. It queues the action onto the thread host and returns a task that completes once the body and its continuations finish. Unwrap the dispatcher operation (`operation.Task.Unwrap()`) to await the continuations.
* `RevitThreadExecutor` declares an `IParallelLimit` of `1` through `ITestRegisteredEventReceiver`. TUnit applies it from the version that forwards the event to an installed executor. A TUnit without that forwarding schedules every Revit test at once, and the bodies interleave at each `await` on the shared thread. The limit belongs to TUnit; never reintroduce it here as an attribute on the base class.
* `RevitApplicationTest` holds the static `Application`. `RevitApiTest` opens the connection before the session, and `RevitConnectionLifetime` closes it after the test application, both on the Revit thread.
* `RevitConnectionLifetime` is an `ITestHostApplicationLifetime`. The package registers it through the `TestingPlatformBuilderHook` item of `build/Nice3point.TUnit.Revit.props`, packed into `build` and `buildTransitive`; a project that writes its own entry point calls `AddRevit` instead. The test project imports the same props, which a project reference does not deliver.

## Repository map

* `source/Nice3point.TUnit.Revit/` — the core testing framework, packed as a Nuget package. It exposes a `RevitApiTest` for users.
* `tests/Nice3point.TUnit.Revit.Tests/` — the test project that tests the framework.
* `build/` — the ModularPipelines build.
* Root — `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `README.md`, `CHANGELOG.md`, CI workflows.

## Build and verify

* Build: `dotnet build -c Release.R##`, where the `R##` suffix is the Revit year (`R27` targets Revit 2027).
* Test: `dotnet test -c Release.R##`; requires a matching licensed Revit installation.
