using System;
using System.Collections.Concurrent;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Images;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class LabelCache : IDisposable
{
    private readonly ConcurrentDictionary<Font, object> _cacheTypeface = new();

    private readonly ConcurrentDictionary<(string? Text, Font Font, Brush? BackColor, Color ForeColor, float Opacity), IDrawableImage> _labelCache = new();

    public T GetOrCreateTypeface<T>(Font font, Func<Font, T> createTypeFace)
        where T : class
    {
        if (!_cacheTypeface.TryGetValue(font, out var typeface))
        {
            typeface = createTypeFace(font);
            _cacheTypeface[font] = typeface;
        }

        return (T)typeface;
    }

    public IDrawableImage GetOrCreateLabel(string? text, LabelStyle style, float layerOpacity, Func<LabelStyle, string?, float, LabelCache, IDrawableImage> createLabelAsBitmap)
    {
        var key = (text, style.Font, style.BackColor, style.ForeColor, layerOpacity);
        if (!_labelCache.TryGetValue(key, out var info))
        {
            info = createLabelAsBitmap(style, text, layerOpacity, this);
            _labelCache[key] = info;
        }

        return info;
    }

    public void Dispose()
    {
        foreach (var item in _labelCache.Values)
        {
            item?.Dispose();
        }

        _labelCache.Clear();

        foreach (var item in _cacheTypeface.Values)
        {
            item.DisposeIfDisposable();
        }
        _cacheTypeface.Clear();
    }
}
