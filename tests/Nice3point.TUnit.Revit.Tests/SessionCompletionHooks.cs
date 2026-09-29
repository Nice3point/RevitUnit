using Nice3point.TUnit.Revit.Executors;
using Nice3point.TUnit.Revit.Tests.Attributes;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit.Tests;

public static class SessionCompletionHooks
{
    [After(TestSession)]
    [HookExecutor<RevitUiThreadExecutor>]
    public static void FailSessionCleanup()
    {
        if (Environment.GetEnvironmentVariable(SessionCompletionProbeAttribute.Variable) == "fail")
        {
            throw new InvalidOperationException("The UI session cleanup failed after the test passed.");
        }
    }
}
