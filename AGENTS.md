# Nice3point.TUnit.Revit Agent Instructions

Nice3point.TUnit.Revit is a public NuGet library that runs TUnit tests inside the Revit process. `source/Nice3point.TUnit.Revit` is the shipped framework: base test classes for the application lifecycle and the Revit API, and a test executor that marshals every test and hook onto the single thread that owns the Revit API. The framework injects into Revit through `Nice3point.Revit.Injector` and PolyHook.

## Non-Negotiables

* **The Revit thread is single and exclusive.** Every API call runs on the one thread that initialized Revit. The executor marshals test bodies and hooks onto it and caps Revit tests to one at a time. Never touch a Revit type off that thread.
* **The package adds Revit support, nothing else.** It exposes the base classes, the executor, and the injection lifecycle. Assertions, attributes, and discovery come from TUnit. Do not reimplement what TUnit already provides.
* **Inject and eject in matched pairs.** The application connects once per test session and releases on teardown. A connection that opens must close on the matching session hook.
* **Attributes carry the contract.** Mark public classes `[PublicAPI]`, read-only methods `[Pure]`, and a member the test platform invokes but consumers must not call `[EditorBrowsable(EditorBrowsableState.Never)]`.
* **Every type compiles under every supported configuration.** Gate version-specific Revit APIs with `#if REVIT2024_OR_GREATER`-style directives and runtime features with `#if NET` or `#if NETFRAMEWORK`.
* **Never break an existing public API.** Deprecate with `[Obsolete]` and keep the member functional. The public surface is a contract.
* **Tests ship with every change.** This framework dogfoods itself. A change to the base classes or the executor ships with a test that exercises it on the Revit thread. See [Testing](./docs/testing.md).
* **Verify unfamiliar APIs.** When unsure of a Revit, TUnit, or .NET API's behavior or signature, confirm it before use. Search the web for the official docs. To read a referenced library's source, query GitHub with `gh` (`gh api`, `gh search code`). If `gh` is unavailable, search the web or ask. Never inspect compiled DLLs or XML extracted from NuGet packages.
* **Keep docs in sync.** A public-surface change updates `README.md`, `CHANGELOG.md`, and the XML docs in the same commit. See [Documentation](./docs/documentation.md).

## Build

The build is a ModularPipelines project. Run `dotnet run -c Release` from the `build` directory to compile.

## Specialized Docs

Read the matching file before related work.

* [Project Structure](./docs/project-structure.md). Solution layout, the framework and test projects, and change placement.
* [Architecture](./docs/architecture.md). Design goals, the base-class model, the Revit-thread executor, and the injection lifecycle.
* [Code Style](./docs/code-style.md). Naming, attributes, language features, and project patterns.
* [Revit Best Practices](./docs/revit-best-practices.md). Revit API context, threading, the version matrix, and performance.
* [Testing](./docs/testing.md). How the framework's own tests run on the Revit thread.
* [Documentation](./docs/documentation.md). README, CHANGELOG, and XML documentation rules.
* [Package Management](./docs/package-management.md). Centralized NuGet and Revit-version package rules.
