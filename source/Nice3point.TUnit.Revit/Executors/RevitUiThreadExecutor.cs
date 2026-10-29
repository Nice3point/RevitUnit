using Nice3point.TUnit.Revit.Sessions;
using Nice3point.TUnit.Revit.Ui;
using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Executors;

/// <summary>
///     Represents the executor that runs tests and hooks on the Revit user interface thread.
/// </summary>
/// <remarks>
///     A test body and its hooks run inside a Revit API context, and their <c>await</c> continuations stay on the same thread.
///     The executor runs one Revit UI test at a time, and the test host runs the UI tests class by class.
///     The executor supports only tests of a class derived from <see cref="RevitApiUiTest" />.
/// </remarks>
public sealed class RevitUiThreadExecutor : ITestExecutor, IHookExecutor, ITestRegisteredEventReceiver
{
    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public ValueTask ExecuteBeforeTestDiscoveryHook(MethodMetadata hookMethodInfo, BeforeTestDiscoveryContext context, Func<ValueTask> action)
    {
        return InvokeAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteBeforeTestSessionHook(MethodMetadata hookMethodInfo, TestSessionContext context, Func<ValueTask> action)
    {
        return InvokeAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteBeforeAssemblyHook(MethodMetadata hookMethodInfo, AssemblyHookContext context, Func<ValueTask> action)
    {
        return InvokeAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteBeforeClassHook(MethodMetadata hookMethodInfo, ClassHookContext context, Func<ValueTask> action)
    {
        return InvokeAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteBeforeTestHook(MethodMetadata hookMethodInfo, TestContext context, Func<ValueTask> action)
    {
        return RevitUiRuntime.Application?.InvokeBeforeTestHookAsync(context.Metadata.TestDetails.TestId, action) ?? default;
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterTestDiscoveryHook(MethodMetadata hookMethodInfo, TestDiscoveryContext context, Func<ValueTask> action)
    {
        return InvokeAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterTestSessionHook(MethodMetadata hookMethodInfo, TestSessionContext context, Func<ValueTask> action)
    {
        return InvokeAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterAssemblyHook(MethodMetadata hookMethodInfo, AssemblyHookContext context, Func<ValueTask> action)
    {
        return InvokeAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterClassHook(MethodMetadata hookMethodInfo, ClassHookContext context, Func<ValueTask> action)
    {
        return InvokeAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterTestHook(MethodMetadata hookMethodInfo, TestContext context, Func<ValueTask> action)
    {
        return InvokeAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        if (RevitUiRuntime.Application is { } runtime)
        {
            return runtime.ExecuteTestAsync(context.Metadata.TestDetails.TestId, action);
        }

        var session = RevitUiSession.Get(context);
        return new ValueTask(session.RunTestAsync(context));
    }

    /// <inheritdoc />
    public ValueTask OnTestRegistered(TestRegisteredContext context)
    {
        if (RevitUiRuntime.Application is { } runtime)
        {
            runtime.Register(context);
            return default;
        }

        RevitUiSession.Get(context.TestContext).Register(context.TestDetails.TestId);
        AddUiConstraint(context);
        context.TestDetails.Timeout = Timeout.InfiniteTimeSpan;
        context.TestDetails.RetryLimit = 0;
        return default;
    }

    // TUnit reads only the first NotInParallel constraint of a test, and the UI key joins the keys of an existing one.
    private static void AddUiConstraint(TestRegisteredContext context)
    {
        const string uiConstraintKey = "Nice3point.TUnit.Revit.Ui";

        var parallelism = context.TestContext.Parallelism;
        var constraints = parallelism.Constraints.ToArray();
        var notInParallel = constraints.OfType<NotInParallelConstraint>().FirstOrDefault();
        if (notInParallel is null)
        {
            parallelism.AddConstraint(new NotInParallelConstraint([uiConstraintKey]));
            return;
        }

        var keys = notInParallel.NotInParallelConstraintKeys;
        if (keys.Count == 0 || keys.Contains(uiConstraintKey))
        {
            return;
        }

        context.ClearParallelConstraints();
        foreach (var constraint in constraints)
        {
            parallelism.AddConstraint(ReferenceEquals(constraint, notInParallel)
                ? notInParallel with { NotInParallelConstraintKeys = [.. keys, uiConstraintKey] }
                : constraint);
        }
    }

    private static ValueTask InvokeAsync(Func<ValueTask> action)
    {
        return RevitUiRuntime.Application?.InvokeAsync(action) ?? default;
    }
}
