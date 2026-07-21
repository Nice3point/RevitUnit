# Nice3point.TUnit.Revit

Nice3point.TUnit.Revit is a public NuGet library that runs TUnit tests inside a live Revit process.
It owns the single thread that initializes the Revit API, marshals every test and hook onto it, and exposes the injected database-level `Application` through base classes.
The framework adds only the Revit execution model on top of TUnit and connects to Revit.

## Non-negotiables

* The Revit thread is single and exclusive. Every API call runs on the one thread that initialized Revit; the executor marshals test bodies and hooks onto it and caps Revit tests to one at a time. Never touch a Revit type off that thread.
* The package adds Revit support, nothing else. It exposes the base classes, the executor, and the injection lifecycle; assertions, attributes, and discovery come from TUnit. Never reimplement what TUnit provides.
* Inject and eject in matched pairs. The application connects once per test session and releases on the matching session teardown hook.
* Attributes carry the contract. Mark public classes `[PublicAPI]`, read-only members `[Pure]`, and a member the test platform invokes but consumers must not call `[EditorBrowsable(EditorBrowsableState.Never)]`.
* Every type compiles under every supported configuration. Gate version-specific Revit APIs with `#if REVIT2024_OR_GREATER`-style directives and runtime features with `#if NET` or `#if NETFRAMEWORK`.
* Never break an existing public API. Deprecate with `[Obsolete]` and keep the member functional. The public surface is a contract.
* Tests ship with every change. The framework dogfoods itself; a change to the base classes or the executor ships with a test that exercises it on the Revit thread.
* Verify unfamiliar APIs. Confirm a Revit, TUnit, or .NET API before use — official docs on the web, or `gh` (`gh api`, `gh search code`) for a library's source. Never inspect compiled DLLs or XML extracted from NuGet packages.
* Keep docs in sync. A public-surface change updates `README.md`, `CHANGELOG.md`, and the XML docs in the same commit.

## Repository map

* `Nice3point.TUnit.Revit/` — the shipped framework and the only package-producing project. The base classes `RevitApplicationTest` and `RevitApiTest` sit at the project root; the `Executors/` folder holds `RevitThreadExecutor` together with the file-scoped STA thread host and parallel limiter.
* `Nice3point.TUnit.Revit.Tests/` — the test project that dogfoods the framework and doubles as the consumer reference. The assembly-level `[assembly: TestExecutor<RevitThreadExecutor>]` lives in `TestsConfiguration.cs`.
* `build/` — the ModularPipelines build that compiles, tests, packages, and publishes.
* Root — build and package configuration (`Directory.Build.props`, `Directory.Packages.props`, `global.json`), `README.md`, `CHANGELOG.md`, and the CI workflows.

## Build and verify

* Compile every configuration: `dotnet run -c Release` from the `build` directory (ModularPipelines).
* Run the tests: `dotnet test -c Release.RNN`, where `RNN` is the target Revit-year configuration (for example `Release.R26`); a matching licensed Revit installation must be present.
* Pack a nuget package: `dotnet run -c Release pack`.
