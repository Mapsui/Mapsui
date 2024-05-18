using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Styles;
using NUnit.Framework;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class BitmapPathRegistryTests
{
    [Test]
    public static async Task AddAndRemoveEntryAsync()
    {
        // Arrange
        var imagePath = new Uri("embeddedresource://Mapsui.Resources.Images.Pin.svg");
        await ImagePathCache.Instance.RegisterAsync(imagePath);

        // Act
        ImagePathCache.Instance.Unregister(imagePath);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => ImagePathCache.Instance.Get(imagePath));
    }

    [Test]
    public static async Task AddAndRemoveUriResourceEmbeddedRegisterAsync()
    {
        // Arrange
        var imagePath = new Uri("embeddedresource://Mapsui.Resources.Images.Pin.svg");
        await ImagePathCache.Instance.RegisterAsync(imagePath);

        // Act
        ImagePathCache.Instance.Unregister(imagePath);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => ImagePathCache.Instance.Get(imagePath));
    }

    [Test]
    public static async Task AddUriResourceEmbeddedRegisterAsync()
    {
        // Arrange
        var imagePath = new Uri("embeddedresource://Mapsui.Resources.Images.Pin.svg");
        await ImagePathCache.Instance.RegisterAsync(imagePath);

        // Act
        var stream = ImagePathCache.Instance.Get(imagePath);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = new Uri($"file://{AppContext.BaseDirectory}/Resources/example.tif");
        await ImagePathCache.Instance.RegisterAsync(examplePath);

        // Act
        ImagePathCache.Instance.Unregister(examplePath);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => ImagePathCache.Instance.Get(examplePath));
    }

    [Test]
    public static async Task AddUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = new Uri($"file://{AppContext.BaseDirectory}/Resources/example.tif");
        await ImagePathCache.Instance.RegisterAsync(examplePath);

        // Act
        var stream = ImagePathCache.Instance.Get(examplePath);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = new Uri("https://mapsui.com/images/logo.svg");
        await ImagePathCache.Instance.RegisterAsync(mapsuiLogo);

        // Act
        ImagePathCache.Instance.Unregister(mapsuiLogo);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => ImagePathCache.Instance.Get(mapsuiLogo));
    }

    [Test]
    public static async Task AddUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = new Uri("https://mapsui.com/images/logo.svg");
        await ImagePathCache.Instance.RegisterAsync(mapsuiLogo);

        // Act
        var stream = ImagePathCache.Instance.Get(mapsuiLogo);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public async static Task RenderBitmapRegistryDisposeShouldRemoveBitmapAsync()
    {
        // Arrange
        var imagePath = new Uri("embeddedresource://Mapsui.Resources.Images.Pin.svg");
        var bitmapPathRegistry = ImagePathCache.Instance;
        await bitmapPathRegistry.RegisterAsync(imagePath);

        // Act
#pragma warning disable IDISP016 // Don't use disposed instance. In this case we want to specifically test dispose.
        bitmapPathRegistry.Dispose();

        // Assert
        // Todo: fix test before merging to main: Assert.Throws<ObjectDisposedException>(() => bitmapPathRegistry.Get(imagePath));
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
