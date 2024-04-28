using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BruTile.Wms;
using Mapsui.Extensions;
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
        var bitmapId = await BitmapRegistry.Instance.RegisterAsync(svgTigerPath);

        // Act
        BitmapRegistry.Instance.Unregister(bitmapId);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => BitmapRegistry.Instance.Get(bitmapId));
    }

    [Test]
    public static async Task AddUriResourceEmbeddedRegisterAsync()
    {
        // Arrange
        var svgTigerPath = typeof(BitmapAtlasSample).LoadSvgPath("Resources.Images.Ghostscript_Tiger.svg");
        var bitmapId = await BitmapRegistry.Instance.RegisterAsync(svgTigerPath);

        // Act
        var bitmap = BitmapRegistry.Instance.Get(bitmapId);

        // Assert
        var stream = bitmap as Stream;
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = new Uri($"file://{AppContext.BaseDirectory}/Resources/example.tif");
        var bitmapId = await BitmapRegistry.Instance.RegisterAsync(examplePath);

        // Act
        BitmapRegistry.Instance.Unregister(bitmapId);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => BitmapRegistry.Instance.Get(bitmapId));
    }

    [Test]
    public static async Task AddUriFileRegisterAsync()
    {
        // Arrange
        var examplePath = new Uri($"file://{AppContext.BaseDirectory}/Resources/example.tif");
        var bitmapId = await BitmapRegistry.Instance.RegisterAsync(examplePath);

        // Act
        var bitmap = BitmapRegistry.Instance.Get(bitmapId);

        // Assert
        var stream = bitmap as Stream;
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static async Task AddAndRemoveUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = new Uri("https://mapsui.com/images/logo.svg");
        var bitmapId = await BitmapRegistry.Instance.RegisterAsync(mapsuiLogo);

        // Act
        BitmapRegistry.Instance.Unregister(bitmapId);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => BitmapRegistry.Instance.Get(bitmapId));
    }

    [Test]
    public static async Task AddUriHttpsRegisterAsync()
    {
        // Arrange
        var mapsuiLogo = new Uri("https://mapsui.com/images/logo.svg");
        var bitmapId = await BitmapRegistry.Instance.RegisterAsync(mapsuiLogo);

        // Act
        var bitmap = BitmapRegistry.Instance.Get(bitmapId);

        // Assert
        var stream = bitmap as Stream;
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
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
