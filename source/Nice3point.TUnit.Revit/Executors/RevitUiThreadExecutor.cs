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
    /// <summary>
    ///     The <c>NotInParallel</c> key every UI test in the test host shares.
    /// </summary>
    private const string UiConstraintKey = "Nice3point.TUnit.Revit.Ui";

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

    /// <summary>
    ///     Adds the <see cref="UiConstraintKey" /> to the <see cref="NotInParallelConstraint" /> of the specified test.
    /// </summary>
    /// <param name="context">The registration context of the test.</param>
    /// <remarks>
    ///     TUnit reads only the first <see cref="NotInParallelConstraint" /> of a test, and the key joins the keys of a constraint the test declares.
    ///     A constraint without keys runs the test alone, and the method leaves it unchanged.
    /// </remarks>
    private static void AddUiConstraint(TestRegisteredContext context)
    {
        var parallelism = context.TestContext.Parallelism;
        var constraints = parallelism.Constraints.ToArray();
        var notInParallel = constraints.OfType<NotInParallelConstraint>().FirstOrDefault();
        if (notInParallel is null)
        {
            parallelism.AddConstraint(new NotInParallelConstraint([UiConstraintKey]));
            return;
        }

        var keys = notInParallel.NotInParallelConstraintKeys;
        if (keys.Count == 0 || keys.Contains(UiConstraintKey))
        {
            return;
        }

        context.ClearParallelConstraints();
        foreach (var constraint in constraints)
        {
            parallelism.AddConstraint(ReferenceEquals(constraint, notInParallel)
                ? notInParallel with { NotInParallelConstraintKeys = [.. keys, UiConstraintKey] }
                : constraint);
        }
    }

    private static ValueTask InvokeAsync(Func<ValueTask> action)
    {
        return RevitUiRuntime.Application?.InvokeAsync(action) ?? default;
    }
}
