using System;
using System.Collections.Concurrent;
using Mapsui.Rendering.Skia.Images;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class DrawableImageCache : IDisposable
{
    private readonly ConcurrentDictionary<string, IDrawableImage> _cache = new();

    public IDrawableImage? GetOrCreate(string key, Func<IDrawableImage?> tryCreateDrawableImage)
    {
        if (_cache.TryGetValue(key, out var value))
            return value;

        var drawableImage = tryCreateDrawableImage();
        if (drawableImage == null)
            return null;
        return _cache[key] = drawableImage;
    }

    public void Dispose()
    {
        foreach (var key in _cache.Keys)
        {
            if (_cache.TryRemove(key, out var value)) // Remove before disposing so that we never have a disposed object in the cache
                value.Dispose();
        }
    }
}
