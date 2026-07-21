---
name: tunit-revit-execution-model
description: >
  Maintain the Nice3point.TUnit.Revit runtime that runs every test and hook on the single thread that owns the Revit API.
  USE FOR: changing the Revit-thread executor, the dedicated STA dispatcher thread, the one-test-at-a-time parallel limit, the application base classes, or the inject and eject connection lifecycle.
  DO NOT USE FOR: writing a test suite that consumes the framework through its public RevitApiTest surface, or the multi-version build and package configuration.
license: MIT
---

# TUnit Revit Execution Model

Nice3point.TUnit.Revit adds a Revit execution model on top of TUnit: one process-wide STA thread initializes Revit and owns its API, and every test and hook is marshaled onto that thread.
This skill covers changing that runtime while preserving its invariants.
Assertions, discovery, and data sources stay with TUnit; the framework connects to Revit through `Nice3point.Revit.Injector` and PolyHook (`PolyHook2.NET`).

## When to use

- Changing `RevitThreadExecutor`, the dispatcher thread host, or the parallel limiter in `Executors/RevitThreadExecutor.cs`.
- Adding or changing a session hook or the inject and eject lifecycle in the base classes.
- Adjusting how `await` continuations return to the Revit thread.

## When not to use

- Writing tests that consume the framework — that uses the public `RevitApiTest` base class and TUnit assertions.
- The multi-version build and Revit package matrix — that lives in project configuration and is gated with `#if REVIT####_OR_GREATER`.

## Workflow

### Step 1: Keep one STA thread the sole owner of the Revit API

The thread host is a process-wide singleton that starts a background STA thread, runs a WPF `Dispatcher` on it, and hands work to that dispatcher.
It is `file`-scoped, so only the executor reaches it.

Never start a second thread for Revit work, and never expose the dispatcher: the `Dispatcher` pumps the Win32 messages COM marshaling needs and routes `await` continuations back through `DispatcherSynchronizationContext`.

### Step 2: Marshal every Revit call through the executor

`RevitThreadExecutor` is the only public entry point. It queues the action onto the thread host and returns a task that completes once the body and all of its continuations finish.

A member that touches Revit reaches this executor through `TestExecutor<RevitThreadExecutor>` for tests, or `[HookExecutor<RevitThreadExecutor>]` for a hook.

### Step 3: Cap Revit tests to one at a time

The parallel limiter keeps the Revit thread exclusive. Keep the limit at one; the thread cannot be shared between two tests.

```csharp
file sealed class RevitCountParallelLimit : IParallelLimit
{
    public int Limit => 1;
}
```

### Step 4: Own the connection in the base classes and inject and eject in matched pairs

`RevitApplicationTest` holds the injector and the static `Application`; `RevitApiTest` opens the connection before the session and closes it after, both on the Revit thread.

`InitializeRevitConnection` creates the `Injector` and stores `InjectApplication()`; `TerminateRevitConnection` calls `EjectApplication()`.
Every connection that opens must close on the matching session hook.

### Step 5: Keep the public surface minimal and attributed

Only the executor and the two base classes are public; the thread host and the limiter stay `file sealed`.
Mark public classes `[PublicAPI]`, read-only members `[Pure]`, and a member the platform invokes but consumers must not call `[EditorBrowsable(EditorBrowsableState.Never)]`.
Gate version-specific Revit APIs with `#if REVIT####_OR_GREATER` and runtime differences with `#if NET` / `#if NETFRAMEWORK` (the WPF `Dispatcher` needs a `WindowsBase` reference on .NET Framework).

### Step 6: Verify by dogfooding on the Revit thread

Add or adjust a test in `Nice3point.TUnit.Revit.Tests` that exercises the change, then run it against a matching Revit install.

```shell
dotnet test -c Release.RNN
```

`RNN` is the target Revit-year configuration, for example `Release.R26`.

## Validation

- [ ] All Revit work flows through `RevitThreadExecutor` onto the single `RevitDispatcherThread`; no second thread is created.
- [ ] The thread host and the parallel limiter stay `file sealed`; only the executor and base classes are public.
- [ ] The parallel limit stays at one.
- [ ] Every session hook that touches Revit carries `[HookExecutor<RevitThreadExecutor>]`, and every injected connection is ejected on its matching hook.
- [ ] A dogfood test covers the change and passes under a matching `Release.RNN`.

## Common Pitfalls

| Pitfall                                                     | Correct approach                                                                       |
|-------------------------------------------------------------|----------------------------------------------------------------------------------------|
| Starting a new thread or `Task.Run` for Revit work          | Route everything through `RevitDispatcherThread.Instance` via the executor.            |
| Returning the dispatcher operation without unwrapping       | Await the inner task (`operation.Task.Unwrap()`) so continuations complete.            |
| A Revit-touching `[Before]`/`[After]` hook with no executor | Add `[HookExecutor<RevitThreadExecutor>]`; hooks do not inherit the assembly executor. |
| Injecting without a matching eject                          | Open in the session-setup hook, release in the matching session-cleanup hook.          |
| Making the thread host or limiter public                    | Keep them `file sealed`; expose only the executor.                                     |
| `Dispatcher` missing on the .NET Framework build            | Reference `WindowsBase` under the `.NETFramework` condition.                           |
