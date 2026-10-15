using System;
using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class ImageFetcherTests
{
    // Turkish lowercases 'I' to the dotless 'ı', while Uri.Host always lowercases it to 'i'.
    // See https://github.com/Mapsui/Mapsui/issues/3383
    [Test, SetCulture("tr-TR")]
    public static void EmbeddedResourceIndexMatchesUriHostUnderTurkishCulture()
    {
        // Arrange
        var host = new Uri("embedded://Mapsui.Resources.Images.Pin.svg").Host;

        // Act
        var embeddedResources = ImageFetcher.LoadEmbeddedResourcePaths();

        // Assert
        Assert.That(embeddedResources.ContainsKey(host), Is.True);
    }
}
