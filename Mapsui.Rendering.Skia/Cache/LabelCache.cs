using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mapsui.Extensions;
using Mapsui.Styles;
using NetTopologySuite.GeometriesGraph;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Cache;

public sealed class LabelCache : ILabelCache
{
    private readonly ConcurrentDictionary<Font, object> _cacheTypeface = new();

    private readonly ConcurrentDictionary<(string? Text, Font Font, Brush? BackColor, Color ForeColor, float Opacity), IBitmapInfo> _labelCache = new();

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

    public T GetOrCreateLabel<T>(string? text, LabelStyle style, float layerOpacity, Func<LabelStyle, string?, float, ILabelCache, T> createLabelAsBitmap)
        where T : IBitmapInfo
    {
        var key = (text, style.Font, style.BackColor, style.ForeColor, layerOpacity);
        if (!_labelCache.TryGetValue(key, out var info))
        {
            info = createLabelAsBitmap(style, text, layerOpacity, this);
            _labelCache[key] = info;
        }

        return (T)info;
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
