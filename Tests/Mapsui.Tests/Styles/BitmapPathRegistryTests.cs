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
        await ImageSourceCache.Instance.RegisterAsync(imageSource);

        // Act
        ImageSourceCache.Instance.Unregister(imageSource);

        // Assert
        Assert.That(ImageSourceCache.Instance.Get(imageSource), Is.Null);
    }

    [Test]
    public static async Task AddAndRemoveUriResourceEmbeddedRegisterAsync()
    {
        // Arrange
        var imageSource = "embedded://Mapsui.Resources.Images.Pin.svg";
        await ImageSourceCache.Instance.RegisterAsync(imageSource);

        // Act
        ImageSourceCache.Instance.Unregister(imageSource);

        // Assert
        Assert.That(ImageSourceCache.Instance.Get(imageSource), Is.Null);
    }

    [Test]
    public static async Task AddUriResourceEmbeddedRegisterAsync()
    {
        // Arrange
        var imageSource = "embedded://Mapsui.Resources.Images.Pin.svg";
        await ImageSourceCache.Instance.RegisterAsync(imageSource);

        // Act
        var stream = ImageSourceCache.Instance.Get(imageSource);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = $"file://{AppContext.BaseDirectory}/Resources/example.tif";
        await ImageSourceCache.Instance.RegisterAsync(examplePath);

        // Act
        ImageSourceCache.Instance.Unregister(examplePath);

        // Assert
        Assert.That(ImageSourceCache.Instance.Get(examplePath), Is.Null);
    }

    [Test]
    public static async Task AddUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = $"file://{AppContext.BaseDirectory}/Resources/example.tif";
        await ImageSourceCache.Instance.RegisterAsync(examplePath);

        // Act
        var bytes = ImageSourceCache.Instance.Get(examplePath);

        // Assert
        Assert.That(bytes is not null);
        Assert.That(bytes?.Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriHttpsRegisterAsync()
    {
        // Arrange
        var urlToMapsuiLogo = "https://mapsui.com/images/logo.svg";
        await ImageSourceCache.Instance.RegisterAsync(urlToMapsuiLogo);

        // Act
        ImageSourceCache.Instance.Unregister(urlToMapsuiLogo);

        // Assert
        Assert.That(ImageSourceCache.Instance.Get(urlToMapsuiLogo), Is.Null);
    }

    [Test]
    public static async Task AddUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = "https://mapsui.com/images/logo.svg";
        await ImageSourceCache.Instance.RegisterAsync(mapsuiLogo);

        // Act
        var bytes = ImageSourceCache.Instance.Get(mapsuiLogo);

        // Assert
        Assert.That(bytes is not null);
        Assert.That(bytes?.Length > 0);
    }
}
