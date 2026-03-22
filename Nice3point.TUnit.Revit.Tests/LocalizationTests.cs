using Autodesk.Revit.ApplicationServices;
using Nice3point.TUnit.Revit.Tests.Attributes;

namespace Nice3point.TUnit.Revit.Tests;

/// <summary>
///     Skips all tests in the class that don't match the current Revit localization.
/// </summary>
public sealed class LocalizationHookTests : RevitApiTest
{
    [Before(Test)]
    public void SkipUnmatchedLocalization()
    {
        if (Application.Language != LanguageType.English_USA)
        {
            Skip.Test("This test is only supported on English localization");
        }
    }

    [Test]
    public async Task Cities_English_ValidName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        await Assert.That(city.Name).IsEqualTo("Aberdeen, MD");
    }
}

/// <summary>
///     Skips tests that don't match the current Revit localization using attributes.
/// </summary>
public sealed class LocalizationAttributeTests : RevitApiTest
{
    [Test]
    [EnglishLocalizationOnly]
    public async Task Cities_English_ValidName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        await Assert.That(city.Name).IsEqualTo("Aberdeen, MD");
    }

    [Test]
    [RussianLocalizationOnly]
    public async Task Cities_Russian_ValidName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        await Assert.That(city.Name).IsEqualTo("Абердин, MD");
    }

    [Test]
    [ChineseLocalizationOnly]
    public async Task Cities_Chinese_ValidName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        await Assert.That(city.Name).IsEqualTo("K.I.索耶空军基地，密歇根");
    }
}

/// <summary>
///     Skips tests dynamically based on the current Revit localization.
/// </summary>
public sealed class LocalizationDynamicSkipTests : RevitApiTest
{
    [Test]
    public async Task Cities_RandomCity_ValidLocalizedName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        switch (Application.Language)
        {
            case LanguageType.English_USA:
                await Assert.That(city.Name).IsEqualTo("Aberdeen, MD");
                break;
            case LanguageType.Russian:
                await Assert.That(city.Name).IsEqualTo("Абердин, MD");
                break;
            case LanguageType.Chinese_Simplified:
                await Assert.That(city.Name).IsEqualTo("K.I.索耶空军基地，密歇根");
                break;
        }
    }
}

/// <summary>
///     Skips tests whose method name contains a language identifier that doesn't match the current Revit localization.
/// </summary>
public sealed class LocalizationNameFilterTests : RevitApiTest
{
    [Test]
    public async Task Cities_English_USA_ValidName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        await Assert.That(city.Name).IsEqualTo("Aberdeen, MD");
    }

    [Test]
    public async Task Cities_Russian_ValidName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        await Assert.That(city.Name).IsEqualTo("Абердин, MD");
    }

    [Test]
    public async Task Cities_Chinese_Simplified_ValidName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        await Assert.That(city.Name).IsEqualTo("K.I.索耶空军基地，密歇根");
    }
}

/// <summary>
///     Globally skips tests whose method name contains a language identifier that doesn't match the current Revit localization.
/// </summary>
/// <remarks>Applies to all tests in the project.</remarks>
public sealed class GlobalLocalizationSkipConfiguration : RevitApiTest
{
    private static readonly string[] Languages = Enum.GetNames<LanguageType>();

    [BeforeEvery(Test)]
    public static void SkipUnmatchedLocalization(TestContext context)
    {
        var currentLanguage = Application.Language.ToString();
        foreach (var language in Languages)
        {
            if (!context.Metadata.TestName.Contains(language, StringComparison.OrdinalIgnoreCase)) continue;

            if (!currentLanguage.Equals(language, StringComparison.OrdinalIgnoreCase))
            {
                Skip.Test($"This test is only supported on {language} localization");
            }

            return;
        }
    }
}