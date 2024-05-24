using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Rendering.Skia.Cache;
public sealed class SpriteCache : IDisposable
{
    private bool _disposed;
    private readonly ConcurrentDictionary<string, SKObject> _cache = new();

    [return: NotNullIfNotNull(nameof(key))]
    public T GetOrCreateSKObject<T>(string key, Func<T> toSKObject) where T : SKObject
    {
        ThrowIfDisposed();

        if (!_cache.TryGetValue(key, out var skObject))
        {
            skObject = toSKObject();
            _cache[key] = skObject;
        }

        return (T)skObject;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);
    }
}
