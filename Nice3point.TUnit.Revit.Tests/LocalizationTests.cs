using Autodesk.Revit.ApplicationServices;
using Nice3point.TUnit.Revit.Tests.Attributes;

namespace Nice3point.TUnit.Revit.Tests;

public sealed class LocalizationTests : RevitApiTest
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

    [Test]
    [EnglishLocalizationOnly]
    public async Task Cities_EnglishAttribute_ValidName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        await Assert.That(city.Name).IsEqualTo("Aberdeen, MD");
    }
}

public sealed class LocalizationEnglishTests : RevitApiTest
{
    [Before(Test)]
    public void SkipUnmatchedLocalization()
    {
        if (Application.Language != LanguageType.English_USA)
        {
            Skip.Test("This test is only supported on English localisation");
        }
    }

    [Test]
    public async Task Cities_EnglishAttribute_ValidName()
    {
        // Arrange & Act
        var city = Application.Cities.Cast<City>().OrderBy(city => city.Name).First();

        // Assert
        await Assert.That(city.Name).IsEqualTo("Aberdeen, MD");
    }
}