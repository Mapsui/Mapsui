using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Styles;
using Mapsui.Tests.Common.Maps;
using Mapsui.Utilities;
using NUnit.Framework;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class BitmapRegistryTests
{
    [Test]
    public static void AddAndRemoveEntry()
    {
        // Arrange
        using var stream = new MemoryStream();
        var bitmapId = BitmapRegistry.Instance.Register(stream);

        // Act
        BitmapRegistry.Instance.Unregister(bitmapId);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => BitmapRegistry.Instance.Get(bitmapId));
    }

    [Test]
    public static async Task AddAndRemoveUriResourceEmbeddedRegisterAsync()
    {
        // Arrange
        var svgTigerPath = typeof(BitmapAtlasSample).LoadSvgPath("Resources.Images.Ghostscript_Tiger.svg");
        await BitmapPathRegistry.Instance.RegisterAsync(svgTigerPath);

        // Act
        BitmapPathRegistry.Instance.Unregister(svgTigerPath);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => BitmapPathRegistry.Instance.Get(svgTigerPath));
    }

    [Test]
    public static async Task AddUriResourceEmbeddedRegisterAsync()
    {
        // Arrange
        var svgTigerPath = typeof(BitmapAtlasSample).LoadSvgPath("Resources.Images.Ghostscript_Tiger.svg");
        await BitmapPathRegistry.Instance.RegisterAsync(svgTigerPath);

        // Act
        var stream = BitmapPathRegistry.Instance.Get(svgTigerPath);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = new Uri($"file://{AppContext.BaseDirectory}/Resources/example.tif");
        await BitmapPathRegistry.Instance.RegisterAsync(examplePath);

        // Act
        BitmapPathRegistry.Instance.Unregister(examplePath);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => BitmapPathRegistry.Instance.Get(examplePath));
    }

    [Test]
    public static async Task AddUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = new Uri($"file://{AppContext.BaseDirectory}/Resources/example.tif");
        await BitmapPathRegistry.Instance.RegisterAsync(examplePath);

        // Act
        var stream = BitmapPathRegistry.Instance.Get(examplePath);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = new Uri("https://mapsui.com/images/logo.svg");
        await BitmapPathRegistry.Instance.RegisterAsync(mapsuiLogo);

        // Act
        BitmapPathRegistry.Instance.Unregister(mapsuiLogo);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => BitmapPathRegistry.Instance.Get(mapsuiLogo));
    }

    [Test]
    public static async Task AddUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = new Uri("https://mapsui.com/images/logo.svg");
        await BitmapPathRegistry.Instance.RegisterAsync(mapsuiLogo);

        // Act
        var stream = BitmapPathRegistry.Instance.Get(mapsuiLogo);

        // Assert
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static void RenderBitmapRegistryGet()
    {
        // Arrange
        using var stream = new MemoryStream();
        var bitmapId = BitmapRegistry.Instance.Register(stream);

        // Act
        using var renderRegistry = new RenderBitmapRegistry(BitmapRegistry.Instance, BitmapPathRegistry.Instance);

        // Assert
        Assert.That(() => renderRegistry.Get(bitmapId) != null);
    }

    [Test]
    public static void RenderBitmapRegistryRemoveEntry()
    {
        // Arrange
        using var stream = new MemoryStream();
        var bitmapId = BitmapRegistry.Instance.Register(stream);

        // Act
        using var renderRegistry = new RenderBitmapRegistry(BitmapRegistry.Instance, BitmapPathRegistry.Instance);
        renderRegistry.Unregister(bitmapId);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => renderRegistry.Get(bitmapId));
    }

    [Test]
    public static void RenderBitmapRegistryDispose_RemovesBitmap()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        var renderRegistry = new RenderBitmapRegistry(BitmapRegistry.Instance, BitmapPathRegistry.Instance);
        var bitmapId = renderRegistry.Register(stream);
        renderRegistry.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => renderRegistry.Get(bitmapId));
        Assert.Throws<KeyNotFoundException>(() => BitmapRegistry.Instance.Get(bitmapId));
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
