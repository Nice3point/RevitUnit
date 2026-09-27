namespace Nice3point.TUnit.Revit.Tests;

public sealed class OutputUiTests : RevitApiUiTest
{
    public const string StandardOutputLine = "Standard output of the UI test";
    public const string StandardErrorLine = "Standard error of the UI test";

    [Test]
    public async Task WriteLine_Console_WritesTheOutputOfTheTest()
    {
        // Act
        Console.WriteLine(StandardOutputLine);
        await Console.Error.WriteLineAsync(StandardErrorLine);

        // Assert
        await Assert.That(TestContext.Current!.GetStandardOutput()).Contains(StandardOutputLine);
    }
}
