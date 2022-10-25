using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Mapsui.Styles;
using NetTopologySuite.GeometriesGraph;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Cache
{
    public class LabelCache : ILabelCache
    {
        private readonly Dictionary<string, SKTypeface> _cacheTypeface = new();
        
        private readonly IDictionary<string, BitmapInfo> _labelCache = new Dictionary<string, BitmapInfo>();
        
        public object GetOrCreateTypeface(Font font)
        {
            if (!_cacheTypeface.TryGetValue(font.ToString(), out var typeface))
            {
                typeface = SKTypeface.FromFamilyName(font.FontFamily,
                    font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                    SKFontStyleWidth.Normal,
                    font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
                _cacheTypeface[font.ToString()] = typeface;
            }

            return typeface;
        }

        public IBitmapInfo GetOrCreateLabel(string? text, LabelStyle style, float layerOpacity, Func<LabelStyle, string?, float, ILabelCache, object> createLabelAsBitmap)
        {
            var key = text + "_" + style.Font.FontFamily + "_" + style.Font.Size + "_" + (float)style.Font.Size + "_" +
                      style.BackColor + "_" + style.ForeColor + layerOpacity;

            if (!_labelCache.TryGetValue(key, out var info))
            {
                info = new BitmapInfo { Bitmap = (SKImage?)createLabelAsBitmap(style, text, layerOpacity, this) };
                _labelCache[key] = info;
            }

            return info;
        }
    }
}