using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class SymbolCache : ISymbolCache
{
    private readonly IDictionary<string, BitmapInfo> _cache = new ConcurrentDictionary<string, BitmapInfo>();

    public IBitmapInfo? GetOrCreate(string key)
    {
        if (_cache.ContainsKey(key))
        {
            var result = _cache[key];
            if (!BitmapHelper.InvalidBitmapInfo(result))
            {
                return result;
            }
        }

        var imageStream = ImageSourceCache.Instance.Get(new Uri(key));
        if (imageStream == null)
        {
            return null;
        }
        bool ownsBitmap = imageStream is not IDisposable;
        var loadBitmap = BitmapHelper.LoadBitmap(imageStream, ownsBitmap) ?? throw new ArgumentNullException(nameof(key));
        return _cache[key] = loadBitmap;
    }

    public Size? GetSize(string key)
    {
        var bitmap = (BitmapInfo?)GetOrCreate(key);
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
