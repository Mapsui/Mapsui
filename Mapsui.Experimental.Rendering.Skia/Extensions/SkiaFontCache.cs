using Mapsui.Rendering;
using Mapsui.Rendering.Caching;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.Extensions;

internal static class SkiaFontCache
{
    public static CacheTracker<SKFont> GetOrCreate(Font font, RenderService renderService)
        => GetOrCreate(font, (float)font.Size, renderService);

    public static CacheTracker<SKFont> GetOrCreate(Font? font, float fallbackSize, RenderService renderService,
        bool? boldOverride = null, SKFontEdging? edging = null)
    {
        var key = new FontCacheKey(
            font?.FontSource,
            font?.FontFamily,
            font?.Size > 0 ? (float)font.Size : fallbackSize,
            boldOverride ?? (font?.Bold == true),
            font?.Italic == true,
            edging);

        // Do not cache a fallback while asynchronously supplied font data is unavailable.
        if (key.FontSource is not null && renderService.FontSourceCache.Get(key.FontSource) is null)
#pragma warning disable IDISP004 // CacheTracker owns and disposes the uncached SKFont.
            return new CacheTracker<SKFont>(CreateFont(key, renderService));
#pragma warning restore IDISP004

        return renderService.VectorCache.GetOrCreate(key, CreateFont);
    }

    private static SKFont CreateFont(FontCacheKey key, RenderService renderService)
    {
        var font = new Font
        {
            FontSource = key.FontSource,
            FontFamily = key.FontFamily,
            Size = key.Size,
            Bold = key.Bold,
            Italic = key.Italic,
        };

        var skFont = SkiaTextLayoutHelper.CreateSkFont(font, renderService);
        if (key.Edging is not null)
            skFont.Edging = key.Edging.Value;
        return skFont;
    }

    private readonly record struct FontCacheKey(FontSource? FontSource, string? FontFamily, float Size, bool Bold,
        bool Italic, SKFontEdging? Edging);
}
