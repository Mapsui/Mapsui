using Android.Graphics;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Bitmap = Android.Graphics.Bitmap;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.Android
{
    public class MapRenderer : IRenderer
    {
        public Canvas Canvas { get; set; }
        public bool ShowDebugInfoInMap { get; set; }

        public MapRenderer()
        {
            RendererFactory.Get = (() => this);
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            Render(Canvas, viewport, layers, ShowDebugInfoInMap);
        }

        private static void Render(Canvas canvas, IViewport viewport, IEnumerable<ILayer> layers, bool showDebugInfoInMap)
        {
            layers = layers.ToList();
            VisibleFeatureIterator.IterateLayers(viewport, layers, (v, s, f) => RenderFeature(canvas, v, s, f));
            if (showDebugInfoInMap) DrawDebugInfo(canvas, layers);
        }

        private static void DrawDebugInfo(Canvas canvas, IEnumerable<ILayer> layers)
        {
            using (var paint = new Paint {TextSize = 40})
            {
                var lineCounter = 1;
                const float tabWidth = 40f;
                const float lineHeight = 40f;

                foreach (var layer in layers)
                {
                    canvas.DrawText(layer.ToString(), tabWidth, lineHeight*(lineCounter++), paint);

                    if (layer is ITileLayer)
                    {
                        var text = "Tiles in memory: " + (layer as ITileLayer).MemoryCache.TileCount.ToString(CultureInfo.InvariantCulture);
                        canvas.DrawText(text, tabWidth, lineHeight*(lineCounter++), paint);
                    }
                }
            }
        }

        public MemoryStream RenderToBitmapStream(IViewport viewport, IEnumerable<ILayer> layers)
        {
            return RenderToBitmapStreamPrivate(viewport, layers);
        }
        
        private MemoryStream RenderToBitmapStreamPrivate(IViewport viewport, IEnumerable<ILayer> layers)
        {
            Bitmap target = Bitmap.CreateBitmap((int)viewport.Width, (int)viewport.Height, Bitmap.Config.Argb8888);
            var canvas = new Canvas(target);
            Render(canvas, viewport, layers, ShowDebugInfoInMap);
            var stream = new MemoryStream();
            target.Compress(Bitmap.CompressFormat.Png, 100, stream);
            target.Dispose();
            canvas.Dispose();
            return stream;
        }

        private static void RenderFeature(Canvas canvas, IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is Point)
                PointRenderer.Draw(canvas, viewport, style, feature);
            //else if (feature.Geometry is MultiPoint)
            //    MultiPointRenderer.DrawMultiPoint(graphics, (MultiPoint)feature.Geometry, style, viewport);
            else if (feature.Geometry is LineString)
                LineStringRenderer.Draw(canvas, viewport, style, feature);
            //else if (feature.Geometry is MultiLineString)
            //    MultiLineStringRenderer.Draw(canvas, (MultiLineString)feature.Geometry, style, viewport);
            //else if (feature.Geometry is Polygon)
            //    PolygonRenderer.Draw(canvas, (Polygon)feature.Geometry, style, viewport);
            //else if (feature.Geometry is MultiPolygon)
            //    MultiPolygonRenderer.Draw(canvas, (MultiPolygon)feature.Geometry, style, viewport);
            else if (feature.Geometry is IRaster)
                RasterRenderer.Draw(canvas, viewport, style, feature);
        }

    }
}
