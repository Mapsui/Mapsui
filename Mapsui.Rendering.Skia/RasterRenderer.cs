using System;
using System.Collections.Generic;
using System.Diagnostics;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class RasterRenderer
    {
        public static void Draw(SKCanvas canvas, IViewport viewport, IStyle style, IFeature feature,
            IDictionary<object, SKBitmapInfo> skBitmapCache, long currentIteration)
        {
            try
            {
                var raster = (IRaster)feature.Geometry;
                var bitmap = TextureHelper.LoadTexture(raster.Data);
                var destination = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());

                TextureHelper.RenderTexture(canvas, bitmap, RoundToPixel(destination).ToSkia());
                bitmap.UnlockPixels();
                bitmap.Dispose();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private static BoundingBox WorldToScreen(IViewport viewport, BoundingBox boundingBox)
        {
            var first = viewport.WorldToScreen(boundingBox.Min);
            var second = viewport.WorldToScreen(boundingBox.Max);
            return new BoundingBox
            (
                Math.Min(first.X, second.X),
                Math.Min(first.Y, second.Y),
                Math.Max(first.X, second.X),
                Math.Max(first.Y, second.Y)
            );
        }

        private static BoundingBox RoundToPixel(BoundingBox boundingBox)
        {
            return new BoundingBox(
                (float)Math.Round(boundingBox.Left),
                (float)Math.Round(Math.Min(boundingBox.Top, boundingBox.Bottom)),
                (float)Math.Round(boundingBox.Right),
                (float)Math.Round(Math.Max(boundingBox.Top, boundingBox.Bottom)));
        }
    }
}
