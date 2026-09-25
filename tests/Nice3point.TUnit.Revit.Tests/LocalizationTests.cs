using Autodesk.Revit.ApplicationServices;
using Nice3point.TUnit.Revit.Tests.Attributes;

namespace Nice3point.TUnit.Revit.Tests;

/// <summary>
///     Represents tests that a test hook skips when the current Revit localization does not match.
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
///     Represents tests that an attribute skips when the current Revit localization does not match.
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
///     Represents tests that skip at run time when the current Revit localization does not match.
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
