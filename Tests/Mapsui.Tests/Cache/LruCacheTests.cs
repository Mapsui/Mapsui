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

    [Test]
    [SuppressMessage("IDisposableAnalyzers.Correctness", "IDISP001:Dispose created")]
    public void LruCacheDoesDispose()
    {
        var cache = new LruCache<int, TestDisposable>(2);
        var item1 = new TestDisposable();
        var item2 = new TestDisposable();
        var item3 = new TestDisposable();

        // add 3 items
        cache.Put(1, item1);
        cache.Put(2, item2);
        cache.Put(3, item3);

        // Item 1 should be disposed
        Assert.IsTrue(item1.Disposed);
    }
}

public sealed class TestDisposable : IDisposable
{
    private bool _disposed;

    public bool Disposed => _disposed;

    public void Dispose()
    {
        _disposed = true;
    }
}
