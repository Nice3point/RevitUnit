using TUnit.Core.Interfaces;

namespace Nice3point.TUnit.Revit.Tests.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public sealed class ExpectedTimeoutAttribute : Attribute, ITestEndEventReceiver
{
    public int Order => 0;

    public ValueTask OnTestEnd(TestContext context)
    {
        if (context.Execution.Result?.Exception is TimeoutException)
        {
            context.Execution.OverrideResult(TestState.Passed, "The test timed out as expected.");
        }

        return default;
    }
}
