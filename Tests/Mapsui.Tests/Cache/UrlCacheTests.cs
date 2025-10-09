using BruTile;
using Mapsui.Extensions.Cache;
using NUnit.Framework;
using System;

namespace Mapsui.Tests.Cache;

[TestFixture]
public class UrlCacheTests
{
    [Test]
    public void PostDataCacheKeyWorks()
    {
        var cache = new SqlitePersistentCache("testCache");
        cache.Add("https://test.com", new byte[] { 1, 2, 3 }, new byte[] { 1, 1, 1 });

        var found = cache.Find("https://test.com", new byte[] { 1, 2, 3 });
        Assert.That(found != null);
        Assert.That(found!.Length == 3);
        Assert.That(found[0] == 1);
        Assert.That(found[1] == 1);
        Assert.That(found[2] == 1);

        var notfound = cache.Find("https://test.com", null);
        Assert.That(notfound == null);
    }

    [Test]
    public void CacheKeyWorks()
    {
        var cache = new SqlitePersistentCache("testCache");
        cache.Add("https://test.com", null, new byte[] { 1, 1, 1 });

        var found = cache.Find("https://test.com", null);
        Assert.That(found != null);
        Assert.That(found!.Length == 3);
        Assert.That(found[0] == 1);
        Assert.That(found[1] == 1);
        Assert.That(found[2] == 1);

        var notfound = cache.Find("https://test.com", new byte[] { 1, 2, 3 });
        Assert.That(notfound == null);
    }

    [Test]
    public void FindWithExpireTimeReturnsNullForNonExistentUrlEntry()
    {
        // This test verifies the fix for NullReferenceException when cache expire time is set
        var cache = new SqlitePersistentCache("testCacheWithExpiry", TimeSpan.FromMinutes(5));

        // Try to find a non-existent entry - should return null without throwing NullReferenceException
        var result = cache.Find("https://nonexistent.com", null);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void FindWithExpireTimeReturnsNullForNonExistentTileEntry()
    {
        // This test verifies the fix for NullReferenceException when cache expire time is set
        var cache = new SqlitePersistentCache("testCacheWithExpiry2", TimeSpan.FromMinutes(5));
        var tileIndex = new TileIndex(0, 0, 0);

        // Try to find a non-existent entry - should return null without throwing NullReferenceException
        var result = cache.Find(tileIndex);

        Assert.That(result, Is.Null);
    }
}
