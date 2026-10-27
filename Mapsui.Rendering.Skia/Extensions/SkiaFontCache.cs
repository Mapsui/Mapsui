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
        // Pass a null FontFamily straight to FromFamilyName instead of leaving the typeface null. On SkiaSharp 3 a
        // null typeface resolved to the platform default at draw time, but SkiaSharp 4 stores SKTypeface.Empty, which
        // has no glyphs, so every string measures zero width and nothing is drawn. FromFamilyName falls back to
        // SKTypeface.Default itself, and also honours Bold and Italic for the default family.
        var typeface = SKTypeface.FromFamilyName(key.FontFamily,
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
