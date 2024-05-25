using System;
using System.Collections.Concurrent;
using Mapsui.Rendering.Skia.Images;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class SymbolCache : IDisposable
{
    private readonly ConcurrentDictionary<string, IDrawableImage> _cache = new();

    public IDrawableImage? GetOrCreate(string key)
    {
        if (_cache.TryGetValue(key, out var value))
            return value;

        var imageBytes = ImageSourceCache.Instance.Get(key);
        if (imageBytes == null)
            return null;
        return _cache[key] = ImageHelper.LoadBitmap(imageBytes);
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
