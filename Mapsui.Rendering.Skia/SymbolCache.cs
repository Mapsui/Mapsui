using System;
using System.Collections.Generic;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia;

public class SymbolCache : ISymbolCache
{
    private readonly IDictionary<int, BitmapInfo> _cache = new Dictionary<int, BitmapInfo>();

    public IBitmapInfo GetOrCreate(int bitmapId)
    {
        if (_cache.Keys.Contains(bitmapId)) return _cache[bitmapId];
        return _cache[bitmapId] = BitmapHelper.LoadBitmap(BitmapRegistry.Instance.Get(bitmapId)) ?? throw new ArgumentException(nameof(bitmapId));
    }

    public Size? GetSize(int bitmapId)
    {
        var bitmap = (BitmapInfo?)GetOrCreate(bitmapId);
        if (bitmap == null)
            return null;

        return new Size(bitmap.Width, bitmap.Height);
    }
}
