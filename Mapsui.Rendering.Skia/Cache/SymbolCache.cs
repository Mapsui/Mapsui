using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class SymbolCache : ISymbolCache
{
    private readonly IDictionary<int, BitmapInfo> _cache = new ConcurrentDictionary<int, BitmapInfo>();

    public IBitmapInfo GetOrCreate(int bitmapId)
    {
        if (_cache.ContainsKey(bitmapId))
        {
            var result = _cache[bitmapId];
            if (!BitmapHelper.InvalidBitmapInfo(result))
            {
                return result;
            }
        }

        var bitmapStream = BitmapRegistry.Instance.Get(bitmapId);
        bool ownsBitmap = bitmapStream is not IDisposable;
        var loadBitmap = BitmapHelper.LoadBitmap(bitmapStream, ownsBitmap) ?? throw new ArgumentNullException(nameof(bitmapId));
        return _cache[bitmapId] = loadBitmap;
    }

    public Size? GetSize(int bitmapId)
    {
        var bitmap = (BitmapInfo?)GetOrCreate(bitmapId);
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
