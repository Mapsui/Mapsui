using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using SkiaSharp;

namespace Mapsui.Experimental.VectorTiles.VexTileCopies;

public static class SKColorFactory
{
    private static readonly ConcurrentDictionary<uint, SKColor> Colours = new();

    public static SKColor MakeColor(byte red, byte green, byte blue, byte alpha = byte.MaxValue, [CallerMemberName] string callerName = "<unknown>")
    {
        var key = MakeKey(red, green, blue, alpha);
        return Colours.GetOrAdd(key, _ => new SKColor(red, green, blue, alpha));
    }

    private static uint MakeKey(byte red, byte green, byte blue, byte alpha) =>
        (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);

    public static SKColor LogColor(SKColor color, [CallerMemberName] string callerName = "<unknown>")
    {
        var key = (uint)color;
        Colours.TryAdd(key, color);
        return color;
    }
}
