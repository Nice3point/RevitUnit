namespace Nice3point.TUnit.Revit.Tests.Abstractions;

public abstract class ClassLifetimeUiTest : RevitApiUiTest
{
    private static readonly Lock StateLock = new();
    private static readonly Dictionary<Type, Type?> ClassesOpenAtSetup = [];
    private static Type? _openClass;

    [Before(Class)]
    public static void OpenClass(ClassHookContext context)
    {
        lock (StateLock)
        {
            ClassesOpenAtSetup[context.ClassType] = _openClass;
            _openClass = context.ClassType;
        }
    }

    [After(Class)]
    public static void CloseClass(ClassHookContext context)
    {
        lock (StateLock)
        {
            if (_openClass == context.ClassType)
            {
                _openClass = null;
            }
        }
    }

    protected async Task AssertClassRunsAloneAsync()
    {
        Type? openClass;
        Type? classOpenAtSetup;
        lock (StateLock)
        {
            openClass = _openClass;
            classOpenAtSetup = ClassesOpenAtSetup[GetType()];
        }

        using (Assert.Multiple())
        {
            await Assert.That(classOpenAtSetup).IsNull();
            await Assert.That(openClass).IsEqualTo(GetType());
        }
    }
}
