using System;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache
{
    public class RenderCache : IRenderCache
    {
        private readonly ISymbolCache _symbolCache = new SymbolCache();
        private readonly IVectorCache _vectorCache = new VectorCache();
        private readonly ISkiaCache _skiaCache = new SkiaCache();
        private readonly ILabelCache _labelCache = new LabelCache();
        
        public Size? GetSize(int bitmapId)
        {
            return _symbolCache.GetSize(bitmapId);
        }

        public IBitmapInfo GetOrCreate(int bitmapID)
        {
            return _symbolCache.GetOrCreate(bitmapID);
        }

        public object GetOrCreateTypeface(Font font)
        {
            return _labelCache.GetOrCreateTypeface(font);
        }

        public IBitmapInfo GetOrCreateLabel(string? text, LabelStyle style, float opacity, Func<LabelStyle, string?, float, ILabelCache, object> createLabelAsBitmap)
        {
            return _labelCache.GetOrCreateLabel(text, style, opacity, createLabelAsBitmap);
        }
    }
}