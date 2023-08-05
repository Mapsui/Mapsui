using System;
using System.Diagnostics.CodeAnalysis;
using Mapsui.Cache;
using NUnit.Framework;

namespace Mapsui.Tests.Cache;

[TestFixture]
public class LruCacheUnitTest
{
    [Test]
    public void LruCacheTest()
    {
        var cache = new LruCache<int, string>(2);

        cache.Put(1, "One");
        cache.Put(2, "Two");
        Assert.AreEqual("One", cache.Get(1));
        cache.Put(3, "Three");
        Assert.IsNull(cache.Get(2));
        cache.Put(4, "Four");
        Assert.IsNull(cache.Get(1));
        Assert.AreEqual("Three", cache.Get(3));
        Assert.AreEqual("Four", cache.Get(4));
    }
}
