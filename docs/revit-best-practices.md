# Revit Best Practices

The framework runs inside the Revit process and owns Revit's threading and lifecycle on the test's behalf. Respect the API's rules and stay allocation-conscious on hot paths.

## Threading & the API Context

* The Revit API may only be called on the thread that initialized it. The framework dedicates one process-wide STA thread to Revit and routes every test and hook there through `RevitThreadExecutor`.
* A WPF `Dispatcher` drives that thread. It pumps the Win32 messages COM marshaling needs and routes `await` continuations back to the same thread.
* Never call a Revit API from the thread pool or a second thread. Mark a test or hook with the Revit executor, or register it globally for the assembly.
* The parallel limiter caps Revit tests to one at a time, so the framework never shares the Revit thread between two tests.

## Injection Lifecycle

* The framework brings Revit up through `Nice3point.Revit.Injector` and PolyHook rather than the UI. It injects once per test session and ejects on the matching teardown.
* An injected connection must always eject. Pair the session-setup and session-cleanup hooks and guard the eject against a connection that never opened.
* The Revit language and installation path resolve from the injector's assembly attributes. The README documents the consumer-facing configuration.

## Revit Versions

The active version comes from the `$(RevitVersion)` build property. The project (SDK `Nice3point.Revit.Sdk`) declares the full `Debug.RNN`/`Release.RNN` configuration list across the supported Revit range and runtimes.

* Use conditional compilation (`#if REVIT2024_OR_GREATER`, and similar) only where the Revit API genuinely differs between versions.
* Use `#if NET` or `#if NETFRAMEWORK` for runtime differences, such as the WPF dispatcher references on .NET versus .NET Framework.
* Apply directives consistently across related members so a type's surface stays coherent per version.
* Every type must compile under every declared `Debug.RNN`/`Release.RNN` configuration.
* Version-specific package versions belong in `Directory.Packages.props`. See [Package Management](./package-management.md).

## Performance

* **Avoid LINQ on hot paths.** Use traditional loops where allocations or iterator overhead matter.
* **Keep the executor lean.** It runs for every test, so it allocates only what a single dispatch needs.
* **Pre-size collections** when the count is known.
* **Prefer batch Revit APIs** over per-element calls inside a test body, and minimize transaction scope.
