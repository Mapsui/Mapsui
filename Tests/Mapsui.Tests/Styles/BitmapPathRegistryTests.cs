using Mapsui.Styles;
using NUnit.Framework;
using System;
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
}
