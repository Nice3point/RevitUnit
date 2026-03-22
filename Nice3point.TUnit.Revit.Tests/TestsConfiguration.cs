using Autodesk.Revit.ApplicationServices;
using Nice3point.TUnit.Revit.Executors;
using TUnit.Core.Executors;

[assembly: TestExecutor<RevitThreadExecutor>]

namespace Nice3point.TUnit.Revit.Tests;

public static class TestsConfiguration
{
    private static readonly string[] Languages = Enum.GetNames<LanguageType>();
    private static readonly string CurrentLanguage = RevitApiContext.Application.Language.ToString();

    [BeforeEvery(Test)]
    public static void SkipUnmatchedLocalization(TestContext context)
    {
        foreach (var language in Languages)
        {
            if (!context.Metadata.TestName.Contains(language, StringComparison.OrdinalIgnoreCase)) continue;

            if (!CurrentLanguage.Equals(language, StringComparison.OrdinalIgnoreCase))
            {
                Skip.Test($"Test targets {language}");
            }

            return;
        }
    }
}