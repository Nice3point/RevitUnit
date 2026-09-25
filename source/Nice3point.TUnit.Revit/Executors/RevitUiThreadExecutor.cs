using Nice3point.TUnit.Revit.Limiters;
using Nice3point.TUnit.Revit.Sessions;
using Nice3point.TUnit.Revit.Ui;
using Nice3point.TUnit.Revit.Ui.Messages;
using TUnit.Core.Exceptions;
using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Executors;

/// <summary>
///     Represents the executor that runs tests and hooks on the Revit user interface thread.
/// </summary>
/// <remarks>
///     A test body and its hooks run inside a Revit API context, and their <c>await</c> continuations stay on the same thread.
///     The executor runs one Revit UI test at a time.
///     The executor supports only tests of a class derived from <see cref="RevitApiUiTest" />.
/// </remarks>
public sealed class RevitUiThreadExecutor : ITestExecutor, IHookExecutor, ITestRegisteredEventReceiver
{
    /// <inheritdoc />
    public ValueTask ExecuteBeforeTestDiscoveryHook(MethodMetadata hookMethodInfo, BeforeTestDiscoveryContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteBeforeTestSessionHook(MethodMetadata hookMethodInfo, TestSessionContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteBeforeAssemblyHook(MethodMetadata hookMethodInfo, AssemblyHookContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteBeforeClassHook(MethodMetadata hookMethodInfo, ClassHookContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteBeforeTestHook(MethodMetadata hookMethodInfo, TestContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterTestDiscoveryHook(MethodMetadata hookMethodInfo, TestDiscoveryContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterTestSessionHook(MethodMetadata hookMethodInfo, TestSessionContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterAssemblyHook(MethodMetadata hookMethodInfo, AssemblyHookContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterClassHook(MethodMetadata hookMethodInfo, ClassHookContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteAfterTestHook(MethodMetadata hookMethodInfo, TestContext context, Func<ValueTask> action)
    {
        return RunHookAsync(action);
    }

    /// <inheritdoc />
    public ValueTask ExecuteTest(TestContext context, Func<ValueTask> action)
    {
        var testId = context.Metadata.TestDetails.TestId;
        if (RevitUiRuntime.InRevitProcess)
        {
            return RunAndReportAsync(testId, action);
        }

        return new ValueTask(RevitUiSession.Instance.RunTestAsync(testId));
    }

    /// <inheritdoc />
    public int Order => 0;

    /// <inheritdoc />
    public ValueTask OnTestRegistered(TestRegisteredContext context)
    {
        context.SetParallelLimiter(RevitUiParallelLimit.Default);
        return default;
    }

    private static async ValueTask RunAndReportAsync(string testId, Func<ValueTask> action)
    {
        try
        {
            await InvokeAsync(action).ConfigureAwait(false);
            Report(new RevitUiTestResult
            {
                TestId = testId,
                Status = RevitUiTestStatus.Passed
            });
        }
        catch (SkipTestException exception)
        {
            Report(new RevitUiTestResult
            {
                TestId = testId,
                Status = RevitUiTestStatus.Skipped,
                Message = exception.Message
            });

            throw;
        }
        catch (Exception exception)
        {
            Report(new RevitUiTestResult
            {
                TestId = testId,
                Status = RevitUiTestStatus.Failed,
                Message = exception.Message,
                StackTrace = exception.StackTrace
            });

            throw;
        }
    }

    private static ValueTask RunHookAsync(Func<ValueTask> action)
    {
        return RevitUiRuntime.InRevitProcess ? InvokeAsync(action) : default;
    }

    private static ValueTask InvokeAsync(Func<ValueTask> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var executionContext = ExecutionContext.Capture();
        if (executionContext is null)
        {
            return RevitUiRuntime.Context!.InvokeAsync(action);
        }

        return RevitUiRuntime.Context!.InvokeAsync(() =>
        {
            var result = default(ValueTask);
            ExecutionContext.Run(executionContext, _ => result = action(), null);
            return result;
        });
    }

    private static void Report(RevitUiTestResult result)
    {
        RevitUiRuntime.Context!.Send(result.Serialize());
    }
}
