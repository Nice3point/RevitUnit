namespace Nice3point.TUnit.Revit.Tests.Abstractions;

public abstract class ClassLifetimeUiTest : RevitApiUiTest
{
    private static readonly Lock StateLock = new();
    private static readonly Dictionary<Type, Type?> ClassesOpenAtSetup = [];
    private static Type? _openClass;

    protected static Type? OpenClass
    {
        get
        {
            lock (StateLock)
            {
                return _openClass;
            }
        }
    }

    protected Type? ClassOpenAtSetup
    {
        get
        {
            lock (StateLock)
            {
                return ClassesOpenAtSetup[GetType()];
            }
        }
    }

    [Before(Class)]
    public static void OpenTestClass(ClassHookContext context)
    {
        lock (StateLock)
        {
            ClassesOpenAtSetup[context.ClassType] = _openClass;
            _openClass = context.ClassType;
        }
    }

    [After(Class)]
    public static void CloseTestClass(ClassHookContext context)
    {
        lock (StateLock)
        {
            if (_openClass == context.ClassType)
            {
                _openClass = null;
            }
        }
    }
}
