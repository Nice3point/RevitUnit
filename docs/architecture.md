# Architecture & Design Principles

Nice3point.TUnit.Revit exists to run unit tests inside a live Revit process. It connects to the Revit API, marshals every test and hook onto the single thread that owns that API, and exposes the result as ordinary TUnit tests. The framework adds the Revit execution model on top of TUnit and leaves assertions, discovery, and data sources to TUnit.

## Core Design Goals

* **Revit API access from a test.** A test inherits a base class and reaches the database-level `Application` without any add-in plumbing.
* **Single-threaded execution.** Revit requires every API call on the thread that initialized it. The framework owns that thread and routes all test work to it.
* **Real Revit, not a mock.** Tests run against an injected Revit application, so they exercise the actual API rather than a stand-in.
* **A thin layer over TUnit.** The framework supplies only the Revit base classes and the executor. Consumers keep the full TUnit assertion and attribute surface.
* **Backward compatibility.** The base classes, the `Application` property, and the executor are public surface. A consumer's test suite must keep compiling.

## Base-Class Model

Tests inherit one of two **instance base classes** that consumers extend.

* `RevitApplicationTest` owns the application lifecycle. It exposes the static `Application` property and the methods that connect to and release the Revit API.
* `RevitApiTest` subclasses it and adds the session hooks that open the connection before the test session and close it after. A consumer inherits `RevitApiTest` and writes tests against `Application`.

The connection opens once per test session and releases on the matching teardown hook. The hooks run on the Revit thread through the executor.

## Revit-Thread Executor

`RevitThreadExecutor` is the heart of the framework. TUnit invokes it for any test or hook marked with `[TestExecutor<RevitThreadExecutor>]` or `[HookExecutor<RevitThreadExecutor>]`, or for every test when the assembly registers it globally.

* A single process-wide STA thread initializes Revit and owns its API. A WPF `Dispatcher` drives that thread, pumps the Win32 messages COM marshaling needs, and routes `await` continuations back to the same thread.
* The executor queues the test body onto the dispatcher and returns a task that completes when the body and all of its continuations finish.
* A parallel limiter caps Revit tests to one at a time, so the Revit thread is never shared between two tests.

The thread host and the parallel limiter are file-scoped. Consumers see only the executor type.

## Injection Lifecycle

The framework brings up Revit through `Nice3point.Revit.Injector` and PolyHook rather than launching the UI. The session-setup hook injects the application and stores the `Application` reference. The session-cleanup hook ejects it. An injected connection must always be ejected on the matching session hook.

The Revit language and installation path are configurable through the injector's assembly attributes. The README documents the consumer-facing configuration.

## Design Rules

* Keep the public surface to the base classes and the executor. Keep the thread host, the limiter, and any injection internals non-public.
* Mark public classes `[PublicAPI]`, read-only members `[Pure]`, and a member the test platform invokes but consumers must not call `[EditorBrowsable(EditorBrowsableState.Never)]`.
* Isolate version-specific Revit API differences behind compilation directives, not duplicated types. See [Revit Best Practices](./revit-best-practices.md).
