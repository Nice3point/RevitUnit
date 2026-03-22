using Autodesk.Revit.ApplicationServices;
using Nice3point.Revit.Injector;

namespace Nice3point.TUnit.Revit.Tests.Attributes;

public sealed class RussianLocalizationOnlyAttribute() : SkipAttribute("This test is only supported on Russian localization")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        return Task.FromResult(RevitEnvironment.Language != nameof(LanguageType.Russian));
    }
}