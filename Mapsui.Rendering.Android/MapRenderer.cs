using Android.Graphics;
using Java.Lang;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Rendering.Android
{
    public class MapRenderer
    {
        private Map _map;
        private IViewport _viewport;
        private Canvas _canvas;

        public MapRenderer(Map map, IViewport viewport) 
        {
            _map = map;
            _viewport = viewport;
        }

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

        public void Render(Canvas canvas)
        {
            _canvas = canvas;
            VisibleFeatureIterator.IterateLayers(_viewport, _map.Layers, RenderFeature);
        }

        private void RenderFeature(IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is IRaster)
            {
                var raster = (feature.Geometry as IRaster);
                var rasterData = raster.Data.ToArray();
                var bmp = BitmapFactory.DecodeByteArray(rasterData, 0, rasterData.Length);
                var destination = RoundToPixel(WorldToScreen(viewport, raster.GetBoundingBox()));

                _canvas.DrawBitmap(bmp, null, destination, null);
            }
        }
    }
}
