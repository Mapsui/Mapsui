using Mapsui.Styles;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class ImageSourceCacheTests
{
    [Test]
    public static async Task AddAndRemoveEntryAsync()
    {
        // Arrange
        var imageSource = "embedded://Mapsui.Resources.Images.Pin.svg";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.RegisterAsync(imageSource);

        // Act
        imageSourceCache.Unregister(imageSource);

        // Assert
        Assert.That(imageSourceCache.Get(imageSource), Is.Null);
    }

    [Test]
    public static async Task AddAndRemoveUriResourceEmbeddedRegisterAsync()
    {
        // Arrange
        var imageSource = "embedded://Mapsui.Resources.Images.Pin.svg";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.RegisterAsync(imageSource);

        // Act
        imageSourceCache.Unregister(imageSource);

        // Assert
        Assert.That(imageSourceCache.Get(imageSource), Is.Null);
    }

    [Test]
    public static async Task AddUriResourceEmbeddedRegisterAsync()
    {
        // Arrange
        var imageSource = "embedded://Mapsui.Resources.Images.Pin.svg";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.RegisterAsync(imageSource);

        // Act
        var stream = imageSourceCache.Get(imageSource);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = $"file://{AppContext.BaseDirectory}/Resources/example.tif";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.RegisterAsync(examplePath);

        // Act
        imageSourceCache.Unregister(examplePath);

        // Assert
        Assert.That(imageSourceCache.Get(examplePath), Is.Null);
    }

    [Test]
    public static async Task AddUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = $"file://{AppContext.BaseDirectory}/Resources/example.tif";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.RegisterAsync(examplePath);

        // Act
        var bytes = imageSourceCache.Get(examplePath);

        // Assert
        Assert.That(bytes is not null);
        Assert.That(bytes?.Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriHttpsRegisterAsync()
    {
        // Arrange
        var urlToMapsuiLogo = "https://mapsui.com/images/logo.svg";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.RegisterAsync(urlToMapsuiLogo);

        // Act
        imageSourceCache.Unregister(urlToMapsuiLogo);

        // Assert
        Assert.That(imageSourceCache.Get(urlToMapsuiLogo), Is.Null);
    }

    [Test]
    public static async Task AddUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = "https://mapsui.com/images/logo.svg";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.RegisterAsync(mapsuiLogo);

        // Act
        var bytes = imageSourceCache.Get(mapsuiLogo);

        // Assert
        Assert.That(bytes is not null);
        Assert.That(bytes?.Length > 0);
    }

    [Test]
    public static async Task AddUriDataRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = $"data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzNiIgaGVpZ2h0PSI1NiI+PHBhdGggZD0iTTE4IC4zNEM4LjMyNS4zNC41IDguMTY4LjUgMTcuODFjMCAzLjMzOS45NjIgNi40NDEgMi41OTQgOS4wOTRIM2w3LjgyIDE1LjExN0wxOCA1NS45MDNsNy4xODctMTMuODk1TDMzIDI2LjkwM2gtLjA2M2MxLjYzMi0yLjY1MyAyLjU5NC01Ljc1NSAyLjU5NC05LjA5NEMzNS41MzEgOC4xNjkgMjcuNjc1LjM0IDE4IC4zNHptMCA5LjQzOGE2LjUgNi41IDAgMSAxIDAgMTMgNi41IDYuNSAwIDAgMSAwLTEzeiIgZmlsbD0iI2ZmZmZmZiIgc3Ryb2tlPSIjMDAwMDAwIi8+PC9zdmc+";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.RegisterAsync(mapsuiLogo);

        // Act
        var bytes = imageSourceCache.Get(mapsuiLogo);

        // Assert
        Assert.That(bytes is not null);
        Assert.That(bytes?.Length > 0);
    }

    [TestCase($"data:image/svg+xml;charset=utf-8,<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"36\" height=\"56\"><path d=\"M18 .34C8.325.34.5 8.168.5 17.81c0 3.339.962 6.441 2.594 9.094H3l7.82 15.117L18 55.903l7.187-13.895L33 26.903h-.063c1.632-2.653 2.594-5.755 2.594-9.094C35.531 8.169 27.675.34 18 .34zm0 9.438a6.5 6.5 0 1 1 0 13 6.5 6.5 0 0 1 0-13z\" fill=\"#ffffff\" stroke=\"#000000\"/></svg>")]
    [TestCase($"data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzNiIgaGVpZ2h0PSI1NiI+PHBhdGggZD0iTTE4IC4zNEM4LjMyNS4zNC41IDguMTY4LjUgMTcuODFjMCAzLjMzOS45NjIgNi40NDEgMi41OTQgOS4wOTRIM2w3LjgyIDE1LjExN0wxOCA1NS45MDNsNy4xODctMTMuODk1TDMzIDI2LjkwM2gtLjA2M2MxLjYzMi0yLjY1MyAyLjU5NC01Ljc1NSAyLjU5NC05LjA5NEMzNS41MzEgOC4xNjkgMjcuNjc1LjM0IDE4IC4zNHptMCA5LjQzOGE2LjUgNi41IDAgMSAxIDAgMTMgNi41IDYuNSAwIDAgMSAwLTEzeiIgZmlsbD0iI2ZmZmZmZiIgc3Ryb2tlPSIjMDAwMDAwIi8+PC9zdmc+")]
    public static async Task CheckUriDataRegisterAsync(string mapsuiData)
    {
        // Arrange
        var mapsuiEmbedded = "embedded://Mapsui.Resources.Images.Pin.svg";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.RegisterAsync(mapsuiEmbedded);

        await imageSourceCache.RegisterAsync(mapsuiData);

        // Act
        var bytesEmbedded = imageSourceCache.Get(mapsuiEmbedded);
        var bytesData = imageSourceCache.Get(mapsuiData);

        // Assert
        Assert.That(bytesEmbedded is not null);
        Assert.That(bytesEmbedded?.Length > 0);
        Assert.That(bytesData is not null);
        Assert.That(bytesData?.Length > 0);

        if (bytesEmbedded is not null && bytesData is not null)
            Assert.That(bytesEmbedded.SequenceEqual(bytesData));
    }

    private static bool StreamEquals(Stream a, Stream b)
    {
        if (a == b)
            return true;

        if (a == null || b == null)
            throw new ArgumentNullException(a == null ? "a" : "b");

        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
        {
            int aByte = a.ReadByte();
            int bByte = b.ReadByte();
            if (aByte.CompareTo(bByte) != 0)
                return false;
        }

        return true;
    }
}
