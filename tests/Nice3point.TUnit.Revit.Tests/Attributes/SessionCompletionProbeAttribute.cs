namespace Nice3point.TUnit.Revit.Tests.Attributes;

public sealed class SessionCompletionProbeAttribute() : SkipAttribute("The session completion probe runs in an isolated test host.")
{
    public const string Variable = "REVIT_TEST_SESSION_COMPLETION";

    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        return Task.FromResult(Environment.GetEnvironmentVariable(Variable) is null);
    }
}
