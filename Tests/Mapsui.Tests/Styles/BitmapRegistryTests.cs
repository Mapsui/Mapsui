using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
    public static async Task UriResourceEmbeddedRegisterAsyncAddAndRemove()
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
    public static async Task UriResourceEmbeddedRegisterAsyncAdd()
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
    public static async Task UriFileRegisterAsyncAddAndRemove()
    {
        // Arrange
        var examplePath = new Uri($"file://{System.AppContext.BaseDirectory}/Resources/example.tif");
        var bitmapId = await BitmapRegistry.Instance.RegisterAsync(examplePath);

        // Act
        BitmapRegistry.Instance.Unregister(bitmapId);

        // Assert
        Assert.Throws<KeyNotFoundException>(() => BitmapRegistry.Instance.Get(bitmapId));
    }

    [Test]
    public static async Task UriFileRegisterAsyncAdd()
    {
        // Arrange
        var examplePath = new Uri($"file://{System.AppContext.BaseDirectory}/Resources/example.tif");
        var bitmapId = await BitmapRegistry.Instance.RegisterAsync(examplePath);

        // Act
        var bitmap = BitmapRegistry.Instance.Get(bitmapId);

        // Assert
        var stream = bitmap as Stream;
        Assert.That(stream is not null);
        Assert.That(stream?.ToBytes().Length > 0);
    }

    [Test]
    public static async Task UriHttpsRegisterAsyncAddAndRemove()
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
    public static async Task UriHttpsRegisterAsyncAdd()
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
}
