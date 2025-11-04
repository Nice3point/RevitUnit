using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

namespace Nice3point.TUnit.Revit.Tests;

[NotInParallel(Order = 1)]
public sealed class RevitApplicationTests : RevitApiTest
{
    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task Documents_Startup_IsEmpty()
    {
        var documents = Application.Documents.Cast<Document>();

        await Assert.That(documents).IsEmpty();
    }

    [Test]
    [TestExecutor<RevitThreadExecutor>]
    public async Task Create_XYZ_ValidDistance()
    {
        var point = Application.Create.NewXYZ(3, 4, 5);

        await Assert.That(point.DistanceTo(XYZ.Zero))
#if NET
            .IsEqualTo(7).Within(0.1);
#else
            .IsBetween(7, 7.1);
#endif
    }
}