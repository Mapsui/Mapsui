using System;
using BruTile;
using Mapsui.Extensions.Cache;
using NUnit.Framework;

namespace Mapsui.Tests.Cache;

[TestFixture]
public class TileCacheTests
{
    [Test]
    public void Find_TileIndex_NotFound_WithExpire_ReturnsNull_NoException()
    {
        // Arrange: use a non-zero expire time to hit the (previous) NRE path when no tile exists
        var cacheName = $"tileCache-{Guid.NewGuid()}";
        var cache = new SqlitePersistentCache(cacheName, TimeSpan.FromMinutes(1));
        var index = new TileIndex(0, 0, 0);

        // Act: do not insert anything; directly query a non-existing tile
        byte[]? result = null;
        Assert.DoesNotThrow(() => result = cache.Find(index));

        // Assert
        Assert.That(result, Is.Null);
    }
}
