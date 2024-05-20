using System;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class ImageSourceCacheTests
{
    [Test]
    public static async Task AddAndRemoveEntryAsync()
    {
        // Arrange
        var imageSource = new Uri("embeddedresource://Mapsui.Resources.Images.Pin.svg");
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
        var imageSource = new Uri("embeddedresource://Mapsui.Resources.Images.Pin.svg");
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
        var imageSource = new Uri("embeddedresource://Mapsui.Resources.Images.Pin.svg");
        await ImageSourceCache.Instance.RegisterAsync(imageSource);

        // Act
        var stream = ImageSourceCache.Instance.Get(imageSource);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = new Uri($"file://{AppContext.BaseDirectory}/Resources/example.tif");
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
        var examplePath = new Uri($"file://{AppContext.BaseDirectory}/Resources/example.tif");
        await ImageSourceCache.Instance.RegisterAsync(examplePath);

        // Act
        var stream = ImageSourceCache.Instance.Get(examplePath);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = new Uri("https://mapsui.com/images/logo.svg");
        await ImageSourceCache.Instance.RegisterAsync(mapsuiLogo);

        // Act
        ImageSourceCache.Instance.Unregister(mapsuiLogo);

        // Assert
        Assert.That(ImageSourceCache.Instance.Get(mapsuiLogo), Is.Null);
    }

    [Test]
    public static async Task AddUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = new Uri("https://mapsui.com/images/logo.svg");
        await ImageSourceCache.Instance.RegisterAsync(mapsuiLogo);

        // Act
        var stream = ImageSourceCache.Instance.Get(mapsuiLogo);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public async static Task RenderBitmapRegistryDisposeShouldRemoveBitmapAsync()
    {
        // Arrange
        var imageSource = new Uri("embeddedresource://Mapsui.Resources.Images.Pin.svg");
        var imageSourceCache = ImageSourceCache.Instance;
        await imageSourceCache.RegisterAsync(imageSource);

        // Act
#pragma warning disable IDISP016 // Don't use disposed instance. In this case we want to specifically test dispose.
        imageSourceCache.Dispose();

        // Assert
        // Todo: fix test before merging to main: Assert.Throws<ObjectDisposedException>(() => imageSourceCache.Get(imageSource));
    }

    [Test]
    public static void UrlShouldDoValueCompare()
    {
        // This is to be sure that Url implements value compare.
        // They don't mention it in the documentation.
        // https://learn.microsoft.com/en-us/dotnet/api/system.security.policy.url.equals?view=net-8.0#system-security-policy-url-equals(system-object)

        // Arrange
        var logoUrl = new Uri("https://mapsui.com/images/logo.svg");
        var otherInstanceOfSameUrl = new Uri("https://mapsui.com/images/logo.svg");

        // Act and Assert
        Assert.That(logoUrl == otherInstanceOfSameUrl, Is.True);
        Assert.That(logoUrl.Equals(otherInstanceOfSameUrl), Is.True);
    }
}
