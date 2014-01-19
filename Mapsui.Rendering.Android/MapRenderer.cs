using Android.Graphics;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Collections.Generic;
using System.Globalization;
using Bitmap = Android.Graphics.Bitmap;
using Math = Java.Lang.Math;

namespace Mapsui.Rendering.Android
{
    public class MapRenderer : IRenderer
    {
        public Canvas Canvas { get; set; }
        public float OutputMultiplier { get; set; }

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

        public static RectF RoundToPixel(BoundingBox dest)
        {
            return new RectF(
                Math.Round(dest.Left),
                Math.Round(Math.Min(dest.Top, dest.Bottom)),
                Math.Round(dest.Right),
                Math.Round(Math.Max(dest.Top, dest.Bottom)));
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            VisibleFeatureIterator.IterateLayers(viewport, layers, RenderFeature);

            foreach (var layer in layers)
            {
                if (layer is ITileLayer)
                {
                    var text = (layer as ITileLayer).MemoryCache.TileCount.ToString(CultureInfo.InvariantCulture);
                    var paint = new Paint { TextSize = 30 };
                    Canvas.DrawText(text, 20f, 20f, paint);
                }
            }
        }

        private void RenderFeature(IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is IRaster)
            {
                if (!feature.RenderedGeometry.ContainsKey(style)) feature.RenderedGeometry[style] = ToAndroidBitmap(feature);
                var bitmap = (Bitmap)feature.RenderedGeometry[style];
                var dest = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                dest = new BoundingBox(
                    dest.MinX * OutputMultiplier,
                    dest.MinY * OutputMultiplier,
                    dest.MaxX * OutputMultiplier,
                    dest.MaxY * OutputMultiplier);
               
                var destination = RoundToPixel(dest);
                Canvas.DrawBitmap(bitmap, null, destination, null);
            }
        }

        private static Bitmap ToAndroidBitmap(IFeature feature)
        {
            var raster = (IRaster)feature.Geometry;
            var rasterData = raster.Data.ToArray();
            var bitmap = BitmapFactory.DecodeByteArray(rasterData, 0, rasterData.Length);
            return bitmap;
        }
    }
}
