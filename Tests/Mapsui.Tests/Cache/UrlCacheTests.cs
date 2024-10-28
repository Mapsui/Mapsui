using Mapsui.Extensions.Cache;
using NUnit.Framework;

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
}
