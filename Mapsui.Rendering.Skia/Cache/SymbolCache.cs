using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mapsui.Rendering.Skia.Images;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class SymbolCache : IDisposable
{
    private readonly IDictionary<string, IDrawableImage> _cache = new ConcurrentDictionary<string, IDrawableImage>();

    public IDrawableImage? GetOrCreate(string key)
    {
        if (_cache.ContainsKey(key))
        {
            IDrawableImage result = _cache[key];
            if (!result.IsDisposed())
            {
                return result;
            }
        }

        var imageStream = ImageSourceCache.Instance.Get(key);
        if (imageStream == null)
        {
            return null;
        }
        var loadBitmap = ImageHelper.LoadBitmap(imageStream) ?? throw new ArgumentNullException(nameof(key));
        return _cache[key] = loadBitmap;
    }

    public Size? GetSize(string key)
    {
        var bitmap = GetOrCreate(key);
        if (bitmap == null)
            return null;

        return new Size(bitmap.Width, bitmap.Height);
    }

    public void Dispose()
    {
        foreach (var value in _cache.Values)
        {
            value.Dispose();
        }

        _cache.Clear();
    }
}
