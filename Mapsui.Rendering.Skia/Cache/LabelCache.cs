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
        private static readonly Dictionary<string, SKTypeface> CacheTypeface = new();
        
        private readonly IDictionary<string, BitmapInfo> LabelCache = new Dictionary<string, BitmapInfo>();
        
        public object GetOrCreateTypeface(Font font)
        {
            if (!CacheTypeface.TryGetValue(font.ToString(), out var typeface))
            {
                typeface = SKTypeface.FromFamilyName(font.FontFamily,
                    font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                    SKFontStyleWidth.Normal,
                    font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
                CacheTypeface[font.ToString()] = typeface;
            }

            return typeface;
        }

        public IBitmapInfo GetOrCreateLabel(string? text, LabelStyle style, float opacity, Func<IBitmapInfo, LabelStyle, string?, float, ILabelCache> createLabelAsBitmap)
        {
            var key = text + "_" + style.Font.FontFamily + "_" + style.Font.Size + "_" + (float)style.Font.Size + "_" +
                      style.BackColor + "_" + style.ForeColor + opacity;

            if (!LabelCache.TryGetValue(key, out var info))
            {
                info = new BitmapInfo { Bitmap = createLabelAsBitmap(style, text, opacity, this) };
                LabelCache[key] = info;
            }

            return info;
        }
    }
}