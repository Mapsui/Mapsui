using System.Collections.Generic;
using Android.Graphics;
using Java.Lang;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

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
        }

        private void RenderFeature(IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is IRaster)
            {
                var raster = (feature.Geometry as IRaster);
                var rasterData = raster.Data.ToArray();
                var bmp = BitmapFactory.DecodeByteArray(rasterData, 0, rasterData.Length);
                var destination = RoundToPixel(WorldToScreen(viewport, raster.GetBoundingBox()));

                Canvas.DrawBitmap(bmp, null, destination, null);
            }
        }
    }
}
