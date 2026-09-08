using Mapsui.Rendering.Caching;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

internal static class SkiaFontCache
{
    public static CacheTracker<SKFont> GetOrCreate(Font font, RenderService renderService)
        => GetOrCreate(font, (float)font.Size, renderService);

    public static CacheTracker<SKFont> GetOrCreate(Font? font, float fallbackSize, RenderService renderService,
        bool? boldOverride = null, SKFontEdging? edging = null)
    {
        var key = new FontCacheKey(
            font?.FontFamily,
            font?.Size > 0 ? (float)font.Size : fallbackSize,
            boldOverride ?? (font?.Bold == true),
            font?.Italic == true,
            edging);

        return renderService.VectorCache.GetOrCreate(key, CreateFont);
    }

    private static SKFont CreateFont(FontCacheKey key)
    {
        var typeface = key.FontFamily is null
            ? null
            : SKTypeface.FromFamilyName(key.FontFamily,
                key.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                SKFontStyleWidth.Normal,
                key.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);

        var font = new SKFont
        {
            Embolden = false,
            Size = key.Size,
            Typeface = typeface,
        };

        if (key.Edging is not null)
            font.Edging = key.Edging.Value;

        return font;
    }

    private readonly record struct FontCacheKey(string? FontFamily, float Size, bool Bold, bool Italic,
        SKFontEdging? Edging);
}
