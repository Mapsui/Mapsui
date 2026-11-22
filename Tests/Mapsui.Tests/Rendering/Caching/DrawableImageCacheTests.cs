using Mapsui.Rendering;
using Mapsui.Rendering.Caching;
using NUnit.Framework;

namespace Mapsui.Tests.Rendering.Caching;

[TestFixture]
public class DrawableImageCacheTests
{
    [Test]
    public void GetIfCreatedReturnsNullAndDoesNotBlockLaterCreation()
    {
        // Arrange
        using var cache = new DrawableImageCache();

#pragma warning disable IDISP001, IDISP004 // The cache owns and disposes the images it holds.
        // Act
        var before = cache.GetIfCreated("key");
        var created = cache.GetOrCreate("key", () => new FakeDrawableImage());
        var after = cache.GetIfCreated("key");
#pragma warning restore IDISP001, IDISP004

        // Assert
        Assert.That(before, Is.Null);
        Assert.That(created, Is.Not.Null);
        Assert.That(after, Is.SameAs(created));
    }

    [Test]
    public void GetIfCreatedReturnsNullAfterAFailedCreation()
    {
        // Arrange
        using var cache = new DrawableImageCache();

#pragma warning disable IDISP001, IDISP004 // The cache owns and disposes the images it holds.
        // Act
        var failed = cache.GetOrCreate("key", () => null);
        var afterFailure = cache.GetIfCreated("key");
        var retried = cache.GetOrCreate("key", () => new FakeDrawableImage());
        var afterRetry = cache.GetIfCreated("key");
#pragma warning restore IDISP001, IDISP004

        // Assert
        Assert.That(failed, Is.Null);
        Assert.That(afterFailure, Is.Null);
        Assert.That(retried, Is.Not.Null);
        Assert.That(afterRetry, Is.SameAs(retried));
    }

    private sealed class FakeDrawableImage : IDrawableImage
    {
        public float Width => 1;
        public float Height => 1;
        public void Dispose() { }
    }
}
