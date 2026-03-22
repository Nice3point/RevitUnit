using Autodesk.Revit.ApplicationServices;
using Nice3point.Revit.Injector;

namespace Nice3point.TUnit.Revit.Tests.Attributes;

public sealed class ChineseLocalizationOnlyAttribute() : SkipAttribute("This test is only supported on Chinese localization")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        return Task.FromResult(RevitEnvironment.Language != nameof(LanguageType.Chinese_Simplified));
    }
}