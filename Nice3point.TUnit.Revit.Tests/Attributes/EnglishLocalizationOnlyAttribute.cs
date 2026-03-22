using Autodesk.Revit.ApplicationServices;
using Nice3point.Revit.Injector;

namespace Nice3point.TUnit.Revit.Tests.Attributes;

public sealed class EnglishLocalizationOnlyAttribute() : SkipAttribute("This test is only supported on English localization")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        if (string.IsNullOrEmpty(RevitEnvironment.Language))
        {
            // English localization is default
            return Task.FromResult(false);
        }

        return Task.FromResult(RevitEnvironment.Language != nameof(LanguageType.English_USA));
    }
}