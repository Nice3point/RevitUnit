using Autodesk.Revit.ApplicationServices;

namespace Nice3point.TUnit.Revit.Tests.Attributes;

public sealed class EnglishLocalizationOnlyAttribute() : SkipAttribute("This test is only supported on English localisation")
{
    public override Task<bool> ShouldSkip(TestRegisteredContext context)
    {
        return Task.FromResult(RevitApiContext.Application.Language != LanguageType.English_USA);
    }
}