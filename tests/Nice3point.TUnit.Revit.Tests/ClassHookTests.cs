namespace Nice3point.TUnit.Revit.Tests;

public sealed class ClassHookTests : RevitApiTest
{
    private static int _beforeClassThreadId;

    [Before(Class)]
    public static void RecordBeforeClassThread()
    {
        _beforeClassThreadId = Environment.CurrentManagedThreadId;
    }

    [Test]
    public async Task BeforeClassHook_DerivedClass_RunsOnTheRevitThread()
    {
        // Arrange & Act
        var threadId = Environment.CurrentManagedThreadId;

        // Assert
        await Assert.That(_beforeClassThreadId).IsEqualTo(threadId);
    }
}
