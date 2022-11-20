using System;
using Mapsui.Styles;

namespace Mapsui.Rendering.Skia.Cache
{
    public class RenderCache : IRenderCache
    {
        public RenderCache()
        {
            SymbolCache = new SymbolCache();
            VectorCache = new VectorCache(SymbolCache);
        }
        
        public ILabelCache LabelCache { get; set; } = new LabelCache();
        
        public ISymbolCache SymbolCache { get; set; }

        public IVectorCache VectorCache { get; set; }

        public Size? GetSize(int bitmapId)
        {
            return SymbolCache.GetSize(bitmapId);
        }

        public IBitmapInfo GetOrCreate(int bitmapID)
        {
            return SymbolCache.GetOrCreate(bitmapID);
        }

        public T GetOrCreateTypeface<T>(Font font, Func<Font, T> createTypeFace) where T : class
        {
            return LabelCache.GetOrCreateTypeface(font, createTypeFace);
        }

        public T GetOrCreateLabel<T>(string? text, LabelStyle style, float opacity, Func<LabelStyle, string?, float, ILabelCache, T> createLabelAsBitmap) where T : IBitmapInfo
        {
            return LabelCache.GetOrCreateLabel(text, style, opacity, createLabelAsBitmap);
        }

        public T GetOrCreatePaint<T>(Pen? pen, float opacity, Func<Pen?, float, T> toPaint) where T : class
        {
            return VectorCache.GetOrCreatePaint(pen, opacity, toPaint);
        }

        public T GetOrCreatePaint<T>(Brush? brush, float opacity, double rotation, Func<Brush?, float, double, ISymbolCache, T> toPaint) where T : class
        {
            return VectorCache.GetOrCreatePaint(brush, opacity, rotation, toPaint);
        }

        public TPath GetOrCreatePath<TPath, TGeometry>(IReadOnlyViewport viewport, TGeometry geometry, float lineWidth, Func<TGeometry, IReadOnlyViewport, float, TPath> toPath) where TPath : class where TGeometry : class
        {
            return VectorCache.GetOrCreatePath(viewport, geometry, lineWidth, toPath);
        }
    }
}