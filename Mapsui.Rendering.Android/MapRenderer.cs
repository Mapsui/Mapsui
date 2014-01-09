using System.Collections.Generic;
using System.Globalization;
using Android.Graphics;
using Java.Lang;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Bitmap = Android.Graphics.Bitmap;

namespace Mapsui.Rendering.Android
{
    public class MapRenderer : IRenderer
    {
        public Canvas Canvas { get; set; }

        private static BoundingBox WorldToScreen(IViewport viewport, BoundingBox boundingBox)
        {
            var box = new BoundingBox
                {
                    Min = viewport.WorldToScreen(boundingBox.Min),
                    Max = viewport.WorldToScreen(boundingBox.Max)
                };
            return box;
        }

        public static RectF RoundToPixel(BoundingBox dest)
        {
            return new RectF(
                Math.Round(dest.Left),
                Math.Round(dest.Top),
                Math.Round(dest.Right),
                Math.Round(dest.Bottom));
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
                var destination = RoundToPixel(WorldToScreen(viewport, feature.Geometry.GetBoundingBox()));
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
