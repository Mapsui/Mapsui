using Mapsui.Rendering.Skia.Images;
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Rendering.Skia.Cache;
public sealed class SpriteCache : IDisposable
{
    private bool _disposed;
    private readonly ConcurrentDictionary<string, IDrawableImage> _cache = new();

    [return: NotNullIfNotNull(nameof(key))]
    public IDrawableImage GetOrCreate(string key, Func<IDrawableImage> toDrawableImage)
    {
        ThrowIfDisposed();

        if (!_cache.TryGetValue(key, out var image))
        {
            image = toDrawableImage();
            _cache[key] = image;
        }

        return image;
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
