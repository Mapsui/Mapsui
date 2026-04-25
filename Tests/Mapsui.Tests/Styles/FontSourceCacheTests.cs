using Mapsui.Styles;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Mapsui.Tests.Styles;

[TestFixture]
public static class FontSourceCacheTests
{
    private const string EmbeddedFont = "embedded://Mapsui.Tests.Resources.Fonts.OpenSans-Regular.ttf";

    [Test]
    public static async Task RegisterAndRetrieveEmbeddedFontAsync()
    {
        // Arrange
        FontSource fontSource = EmbeddedFont;
        var cache = new FontSourceCache();
        await cache.TryRegisterAsync(fontSource.SourceId, fontSource.Source);

        // Act
        var bytes = cache.Get(fontSource);

        // Assert
        Assert.That(bytes, Is.Not.Null);
        Assert.That(bytes!.Length, Is.GreaterThan(0));
    }

    [Test]
    public static async Task DuplicateRegisterReturnsFalseAsync()
    {
        // Arrange
        FontSource fontSource = EmbeddedFont;
        var cache = new FontSourceCache();
        await cache.TryRegisterAsync(fontSource.SourceId, fontSource.Source);

        // Act
        var result = await cache.TryRegisterAsync(fontSource.SourceId, fontSource.Source);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public static void GetBeforeRegisterReturnsNull()
    {
        // Arrange
        FontSource fontSource = EmbeddedFont;
        var cache = new FontSourceCache();

        // Act
        var bytes = cache.Get(fontSource);

        // Assert
        Assert.That(bytes, Is.Null);
    }

    [Test]
    public static async Task ClearCacheRemovesFontsAsync()
    {
        // Arrange
        FontSource fontSource = EmbeddedFont;
        var cache = new FontSourceCache();
        await cache.TryRegisterAsync(fontSource.SourceId, fontSource.Source);

        // Act
        cache.ClearCache();
        var bytes = cache.Get(fontSource);

        // Assert
        Assert.That(bytes, Is.Null);
    }

    [Test]
    public static async Task RegisterFromFileAsync()
    {
        // Arrange
        FontSource fontSource = $"file://{AppContext.BaseDirectory}/Resources/Fonts/OpenSans-Regular.ttf";
        var cache = new FontSourceCache();
        await cache.TryRegisterAsync(fontSource.SourceId, fontSource.Source);

        // Act
        var bytes = cache.Get(fontSource);

        // Assert
        Assert.That(bytes, Is.Not.Null);
        Assert.That(bytes!.Length, Is.GreaterThan(0));
    }
}
