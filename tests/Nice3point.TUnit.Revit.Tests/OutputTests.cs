namespace Nice3point.TUnit.Revit.Tests;

public sealed class OutputTests
{
    [Test]
    [DependsOn(typeof(OutputUiTests), nameof(OutputUiTests.WriteLine_Console_WritesTheOutputOfTheTest))]
    public async Task Output_UiTestWritesToTheConsole_AppearsInTheResultOfTheUiTest()
    {
        // Arrange
        var uiTest = TestContext.Current!.Dependencies.GetTests(nameof(OutputUiTests.WriteLine_Console_WritesTheOutputOfTheTest), typeof(OutputUiTests)).Single();

        // Act
        var standardOutput = uiTest.GetStandardOutput();
        var standardError = uiTest.GetErrorOutput();

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(standardOutput).Contains(OutputUiTests.StandardOutputLine);
            await Assert.That(standardError).Contains(OutputUiTests.StandardErrorLine);
        }
    }
}
