using System.Collections.Concurrent;
using System.Collections.Generic;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Cache
{
    public class LabelCache : ILabelCache
    {
        private static readonly ConcurrentDictionary<string, SKTypeface> CacheTypeface = new();
        
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
    }
}