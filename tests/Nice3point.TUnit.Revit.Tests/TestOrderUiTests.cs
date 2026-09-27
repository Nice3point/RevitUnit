namespace Nice3point.TUnit.Revit.Tests;

public sealed class TestOrderUiTests : RevitApiUiTest
{
    private static string? _preparedTestId;

    [Before(Test)]
    public void RecordPreparedTest(TestContext context)
    {
        _preparedTestId = context.Metadata.TestDetails.TestId;
    }

    [Test]
    [Repeat(3)]
    public async Task BeforeTestHook_ExecutedTest_RunsRightBeforeItsBody()
    {
        // Arrange & Act
        var testId = TestContext.Current!.Metadata.TestDetails.TestId;

        // Assert
        await Assert.That(_preparedTestId).IsEqualTo(testId);
    }
}
