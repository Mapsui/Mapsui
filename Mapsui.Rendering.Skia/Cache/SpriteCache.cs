using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Rendering;
public sealed class SpriteCache() : ISpriteCache
{
    private bool _disposed;
    private readonly ConcurrentDictionary<string, SKImage> _cache = new();

    [return: NotNullIfNotNull(nameof(key))]
    public SKImage GetOrCreatePaint(string key, Func<SKImage> toSKImage)
    {
        ThrowIfDisposed();

        if (!_cache.TryGetValue(key, out var skImage))
        {
            skImage = toSKImage();
            _cache[key] = skImage;
        }

        return skImage;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }
    }
}
