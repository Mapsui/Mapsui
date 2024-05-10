using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class SymbolCache : ISymbolCache
{
    private readonly IDictionary<string, BitmapInfo> _cache = new ConcurrentDictionary<string, BitmapInfo>();

    public Size? GetSize(int bitmapId) => GetSize(ToKey(bitmapId));
    IBitmapInfo ISymbolCache.GetOrCreate(int bitmapId) => GetOrCreate(ToKey(bitmapId));
    public Size? GetSize(SymbolStyle symbolStyle) => GetSize(SymbolCache.GetKey(symbolStyle));
    public IBitmapInfo GetOrCreate(SymbolStyle symbolStyle) => GetOrCreate(SymbolCache.GetKey(symbolStyle));
    public Size? GetSize(Brush brush) => GetSize(SymbolCache.GetKey(brush));
    public IBitmapInfo GetOrCreate(Brush brush) => GetOrCreate(SymbolCache.GetKey(brush));

    public IBitmapInfo GetOrCreate(string key)
    {
        if (_cache.ContainsKey(key))
        {
            var result = _cache[key];
            if (!BitmapHelper.InvalidBitmapInfo(result))
            {
                return result;
            }
        }

        if (key.StartsWith("bitmapId://"))
        {
            //!!! This is a hopeless workaround to get things to work for now.
            var index = "bitmapId://".Length;
            var subString = key.Substring(index);
            var bitmapId = int.Parse(subString);
            object bitmapData = BitmapRegistry.Instance.Get(bitmapId);
            bool ownsBitmap = bitmapData is not IDisposable;
            var loadBitmap = BitmapHelper.LoadBitmap(bitmapData, ownsBitmap) ?? throw new ArgumentNullException(nameof(key));
            return _cache[key] = loadBitmap;
        }
        else
        {
            Stream bitmapStream = BitmapPathRegistry.Instance.Get(new Uri(key));
            bool ownsBitmap = bitmapStream is not IDisposable;
            var loadBitmap = BitmapHelper.LoadBitmap(bitmapStream, ownsBitmap) ?? throw new ArgumentNullException(nameof(key));
            return _cache[key] = loadBitmap;
        }
    }

    private Size? GetSize(string key)
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

    private static string GetKey(Brush brush)
        => brush.BitmapPath is not null ? brush.BitmapPath.ToString() : ToKey(brush.BitmapId);
    static string GetKey(SymbolStyle symbolStyle)
        => symbolStyle.BitmapPath is not null ? symbolStyle.BitmapPath.ToString() : ToKey(symbolStyle.BitmapId);
    private static string ToKey(int bitmapId) => $"bitmapId://{bitmapId}";
}
