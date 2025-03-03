﻿using Mapsui.Styles;
using NUnit.Framework;
using System;
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
        await imageSourceCache.TryRegisterAsync(imageSource);

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
        await imageSourceCache.TryRegisterAsync(imageSource);

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
        await imageSourceCache.TryRegisterAsync(imageSource);

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
        Image examplePath = $"file://{AppContext.BaseDirectory}/Resources/example.tif";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.TryRegisterAsync(examplePath);

        // Act
        imageSourceCache.Unregister(examplePath);

        // Assert
        Assert.That(imageSourceCache.Get(examplePath), Is.Null);
    }

    [Test]
    public static async Task AddUriFileRegisterAsync()
    {
        // Arrange
        Image examplePath = $"file://{AppContext.BaseDirectory}/Resources/example.tif";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.TryRegisterAsync(examplePath);

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
        Image urlToMapsuiLogo = "https://mapsui.com/images/logo.svg";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.TryRegisterAsync(urlToMapsuiLogo);

        // Act
        imageSourceCache.Unregister(urlToMapsuiLogo);

        // Assert
        Assert.That(imageSourceCache.Get(urlToMapsuiLogo), Is.Null);
    }

    [Test]
    public static async Task AddUriHttpsRegisterAsync()
    {
        // Arrange
        Image mapsuiLogo = "https://mapsui.com/images/logo.svg";
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.TryRegisterAsync(mapsuiLogo);

        // Act
        var bytes = imageSourceCache.Get(mapsuiLogo);

        // Assert
        Assert.That(bytes is not null);
        Assert.That(bytes?.Length > 0);
    }

    [TestCase("svg-content://<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"36\" height=\"56\"><path d=\"M18 .34C8.325.34.5 8.168.5 17.81c0 3.339.962 6.441 2.594 9.094H3l7.82 15.117L18 55.903l7.187-13.895L33 26.903h-.063c1.632-2.653 2.594-5.755 2.594-9.094C35.531 8.169 27.675.34 18 .34zm0 9.438a6.5 6.5 0 1 1 0 13 6.5 6.5 0 0 1 0-13z\" fill=\"#ffffff\" stroke=\"#000000\"/></svg>")]
    [TestCase("base64-content://PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzNiIgaGVpZ2h0PSI1NiI+PHBhdGggZD0iTTE4IC4zNEM4LjMyNS4zNC41IDguMTY4LjUgMTcuODFjMCAzLjMzOS45NjIgNi40NDEgMi41OTQgOS4wOTRIM2w3LjgyIDE1LjExN0wxOCA1NS45MDNsNy4xODctMTMuODk1TDMzIDI2LjkwM2gtLjA2M2MxLjYzMi0yLjY1MyAyLjU5NC01Ljc1NSAyLjU5NC05LjA5NEMzNS41MzEgOC4xNjkgMjcuNjc1LjM0IDE4IC4zNHptMCA5LjQzOGE2LjUgNi41IDAgMSAxIDAgMTMgNi41IDYuNSAwIDAgMSAwLTEzeiIgZmlsbD0iI2ZmZmZmZiIgc3Ryb2tlPSIjMDAwMDAwIi8+PC9zdmc+")]
    public static async Task CheckUriSvgRegisterAsync(string uriSource)
    {
        // Arrange
        var imageSourceCache = new ImageSourceCache();

        Image referenceImageSource = "embedded://Mapsui.Resources.Images.Pin.svg";
        Image uriImage = uriSource;
        await imageSourceCache.TryRegisterAsync(referenceImageSource);
        var referenceBytes = imageSourceCache.Get(referenceImageSource);

        // Act
        await imageSourceCache.TryRegisterAsync(uriImage);
        var bytes = imageSourceCache.Get(uriImage);

        // Assert
        Assert.That(referenceBytes is not null);
        Assert.That(referenceBytes?.Length > 0);

        Assert.That(bytes is not null);
        Assert.That(bytes?.Length > 0);

        if (referenceBytes is not null && bytes is not null)
            Assert.That(referenceBytes.SequenceEqual(bytes));
    }

    [TestCase("base64-content://iVBORw0KGgoAAAANSUhEUgAAABQAAAAUCAIAAAAC64paAAAACXBIWXMAAC4jAAAuIwF4pT92AAAAcUlEQVQ4y+VUyw7AIAgrxgtf4v9/HZ5kF90M6JK9siVruGGlNFVSVZxFwAXMyURrlZwPTy4i2F3qIdmfJsfNW4/mVmAetqI/alV5w9uku3buUlGzIQJAU7ItS1a11cmraTHdf4dkeDEzAAJmL4te+0kWaRI0VGH3VHwAAAAASUVORK5CYII=")]
    public static async Task CheckUriImageRegisterAsync(string uri)
    {
        // Arrange
        Image fileImage = $"file://{AppContext.BaseDirectory}/Resources/Images/image.png";
        Image uriImage = uri;
        var imageSourceCache = new ImageSourceCache();
        await imageSourceCache.TryRegisterAsync(fileImage);

        await imageSourceCache.TryRegisterAsync(uriImage);

        // Act
        var bytesFile = imageSourceCache.Get(fileImage);
        var bytesUri = imageSourceCache.Get(uriImage);

        // Assert
        Assert.That(bytesFile is not null);
        Assert.That(bytesFile?.Length > 0);
        Assert.That(bytesUri is not null);
        Assert.That(bytesUri?.Length > 0);

        if (bytesFile is not null && bytesUri is not null)
            Assert.That(bytesFile.SequenceEqual(bytesUri));
    }
}
