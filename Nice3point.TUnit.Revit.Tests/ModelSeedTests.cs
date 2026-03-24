using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class ModelSeedTests : RevitApiTest
{
    private Document _document = null!;
    private IList<Level> _levels = null!;
    private IList<Wall> _exteriorWalls = null!;
    private IList<Grid> _grids = null!;

    [Before(Test)]
    [HookExecutor<RevitThreadExecutor>]
    public void SeedModel()
    {
        _document = Application.NewProjectDocument(UnitSystem.Metric);

        using var transaction = new Transaction(_document, "Seed model");
        transaction.Start();

        _levels =
        [
            Level.Create(_document, 0),
            Level.Create(_document, 3),
            Level.Create(_document, 6),
        ];

        _exteriorWalls =
        [
            Wall.Create(_document, Line.CreateBound(new XYZ(0, 0, 0), new XYZ(10, 0, 0)), _levels[0].Id, false),
            Wall.Create(_document, Line.CreateBound(new XYZ(10, 0, 0), new XYZ(10, 6, 0)), _levels[0].Id, false),
            Wall.Create(_document, Line.CreateBound(new XYZ(10, 6, 0), new XYZ(0, 6, 0)), _levels[0].Id, false),
            Wall.Create(_document, Line.CreateBound(new XYZ(0, 6, 0), new XYZ(0, 0, 0)), _levels[0].Id, false),
        ];

        _grids =
        [
            Grid.Create(_document, Line.CreateBound(new XYZ(0, -1, 0), new XYZ(0, 7, 0))),
            Grid.Create(_document, Line.CreateBound(new XYZ(5, -1, 0), new XYZ(5, 7, 0))),
            Grid.Create(_document, Line.CreateBound(new XYZ(10, -1, 0), new XYZ(10, 7, 0))),
        ];

        _levels[0].Name = "Ground Floor";
        _levels[1].Name = "First Floor";
        _levels[2].Name = "Second Floor";

        transaction.Commit();
    }

    [After(Test)]
    [HookExecutor<RevitThreadExecutor>]
    public void CloseModel()
    {
        _document.Close(false);
    }

    [Test]
    public async Task FilteredElementCollector_ExteriorWalls_MatchSeededCount()
    {
        // Act
        var walls = new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Wall))
            .ToList();

        // Assert
        await Assert.That(walls.Count).IsEqualTo(_exteriorWalls.Count);
    }

    [Test]
    public async Task FilteredElementCollector_Levels_MatchSeededCount()
    {
        // Act
        var levels = new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Level))
            .ToList();

        // Assert
        await Assert.That(levels.Count - 1).IsEqualTo(_levels.Count);
    }

    [Test]
    public async Task FilteredElementCollector_Grids_MatchSeededCount()
    {
        // Act
        var grids = new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Grid))
            .ToList();

        // Assert
        await Assert.That(grids.Count).IsEqualTo(_grids.Count);
    }

    [Test]
    public async Task Transaction_DemolishWall_RemainingWallCountDecreases()
    {
        // Arrange
        var targetId = _exteriorWalls[0].Id;

        // Act
        using var transaction = new Transaction(_document, "Demolish wall");
        transaction.Start();
        _document.Delete(targetId);
        transaction.Commit();

        var remainingWalls = new FilteredElementCollector(_document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Wall))
            .ToElementIds();

        // Assert
        using (Assert.Multiple())
        {
            await Assert.That(_exteriorWalls).Contains(wall => !wall.IsValidObject);
            await Assert.That(remainingWalls.Count).IsEqualTo(_exteriorWalls.Count - 1);
        }
    }
}