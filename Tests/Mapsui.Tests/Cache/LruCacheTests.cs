﻿using System;
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
        Assert.That(cache.Get(1), Is.EqualTo("One"));
        cache.Put(3, "Three");
        Assert.That(cache.Get(2), Is.Null);
        cache.Put(4, "Four");
        Assert.That(cache.Get(1), Is.Null);
        Assert.That(cache.Get(3), Is.EqualTo("Three"));
        Assert.That(cache.Get(4), Is.EqualTo("Four"));
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
        Assert.That(item1.Disposed, Is.True);
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
