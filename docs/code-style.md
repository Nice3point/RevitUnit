# Code Style

Production C# only. This is a public library, so its style is part of its contract.

## General Principles

* **SOLID and DRY.** One responsibility per type. Extract shared logic rather than duplicate it.
* **Explicit over implicit.** Code is self-explanatory. Avoid hidden behavior and unclear defaults.
* **Instance base classes.** Test base classes are instance classes consumers inherit. The lifecycle and thread plumbing live in the base class, never in the consumer's test.
* **Nullable safety.** Nullable reference types are enabled solution-wide. Treat every nullability warning as a defect.
* **StyleCop style.** Follow StyleCop conventions for layout, member ordering, and spacing.

## Modern C#

`LangVersion` is `latest`. Reach for the newest feature that expresses the intent directly, and do not hand-roll what the language already provides.

* Primary constructors when a type captures state.
* Collection expressions for literals and spans.
* Pattern matching and switch expressions over branching chains.
* The `field` keyword for a lazily initialized auto-property.
* Expression-bodied members for simple accessors.
* File-scoped namespaces and file-scoped types for non-public helpers.

## Comments

Public types and members carry XML doc comments, see [Documentation](./documentation.md). Inside the code, comments are the exception.

* Names and structure carry the meaning. Default to no comment.
* Add one only when the reason cannot be read from the code and a reader could break the code without it, such as a threading constraint on the Revit thread.
* A comment explains why, never what. Do not restate the code.

## Attributes

Decorate members with every JetBrains and .NET attribute that carries meaning, so analyzers, the debugger, and callers read the full contract.

* `[PublicAPI]` on every public class.
* `[Pure]` on a read-only method.
* `[EditorBrowsable(EditorBrowsableState.Never)]` on a member the test platform invokes but consumers must not call.
* `[Obsolete]` on a deprecated member that stays functional. See [Documentation](./documentation.md).

## Naming

* **Clarity first.** Names are descriptive and never abbreviated: `application` not `app`, `document` not `doc`, `dispatcher` not `disp`.
* Follow the Revit API naming conventions.
* A base test class names what a consumer inherits it for, such as `RevitApiTest`.
* No single-letter variables except in a short loop or lambda.

## File and Class Structure

* **File-scoped namespaces.** Use `namespace Nice3point.TUnit.Revit;` or a sub-namespace.
* **File-scoped types.** Keep the thread host and the parallel limiter `file`-scoped so they never leak into the public surface.
* **Member order:** private fields, constructors, public properties, public methods, private methods.

## The Revit-Thread Executor

Test code reaches the Revit thread through the executor, never directly. The executor queues an action onto the dispatcher that drives the Revit STA thread and returns a task that completes once the action and its continuations finish:

```csharp
public ValueTask InvokeAsync(Func<ValueTask> action)
{
    var operation = _dispatcher.InvokeAsync(() => action().AsTask(), DispatcherPriority.Normal);
    return new ValueTask(operation.Task.Unwrap());
}
```

The dispatcher owns COM marshaling and continuation routing. Do not start a second thread or call a Revit API from the thread pool. See [Revit Best Practices](./revit-best-practices.md).

## The Lifecycle Pair

The application connects on the session-setup hook and releases on the matching session-cleanup hook. Guard the eject against a connection that never opened:

```csharp
protected static void TerminateRevitConnection()
{
    _injector?.EjectApplication();
}
```

## Compilation Directives

* `#if REVIT2024_OR_GREATER` and similar for version-specific Revit APIs.
* `#if NET` or `#if NETFRAMEWORK` for runtime-specific references, such as the WPF dispatcher assemblies.
* Apply directives consistently across related members so a type's surface stays coherent per version. See [Revit Best Practices](./revit-best-practices.md).
