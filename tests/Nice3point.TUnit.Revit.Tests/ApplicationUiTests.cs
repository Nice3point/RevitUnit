using Autodesk.Revit.UI;
using Nice3point.TUnit.Revit.Tests.Commands;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class ApplicationUiTests : RevitApiUiTest
{
    [Test]
    public async Task AddPushButton_ToolkitCommand_AppearsOnTheRibbonPanel()
    {
        // Arrange
        const string tabName = "TUnit";
        const string buttonName = "TUnit";

        var controlledApplication = UiApplication.AsControlledApplication();
        var panel = controlledApplication.CreatePanel(buttonName, tabName);

        try
        {
            // Act
            var button = panel.AddPushButton<EmptyCommand>("Run");

            // Assert
            using (Assert.Multiple())
            {
                await Assert.That(UiApplication.GetRibbonPanels(tabName).Select(ribbonPanel => ribbonPanel.Name)).Contains(panel.Name);
                await Assert.That(button.ClassName).IsEqualTo(typeof(EmptyCommand).FullName);
            }
        }
        finally
        {
            panel.RemovePanel();
        }
    }

    [Test]
    public async Task SetElementIds_ActiveDocument_SelectsTheElements()
    {
        // Arrange
        var uiDocument = OpenAndActivateNewDocument();
        var levelIds = uiDocument.Document.CollectElements()
            .OfClass<Level>()
            .ToElementIds();

        // Act
        uiDocument.Selection.SetElementIds(levelIds);

        // Assert
        await Assert.That(uiDocument.Selection.GetElementIds()).IsEquivalentTo(levelIds);
    }

    [Test]
    public async Task GetOpenUIViews_ActiveDocument_ContainsTheActiveView()
    {
        // Arrange
        var uiDocument = OpenAndActivateNewDocument();

        // Act
        var openViewIds = uiDocument.GetOpenUIViews()
            .Select(uiView => uiView.ViewId)
            .ToList();

        // Assert
        await Assert.That(openViewIds).Contains(uiDocument.ActiveView.Id);
    }

    [Test]
    public async Task CanPostCommand_BuiltInCommand_IsPostable()
    {
        // Arrange
        var commandId = RevitCommandId.LookupPostableCommandId(PostableCommand.Default3DView);

        // Act
        var canPost = UiApplication.CanPostCommand(commandId);

        // Assert
        await Assert.That(canPost).IsTrue();
    }

    private static UIDocument OpenAndActivateNewDocument()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Path.GetRandomFileName()}.rvt");
        var document = UiApplication.Application.NewProjectDocument(UnitSystem.Metric);
        document.SaveAs(path);
        document.Close(false);

        return UiApplication.OpenAndActivateDocument(path);
    }
}
