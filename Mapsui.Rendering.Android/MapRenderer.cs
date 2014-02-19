using System.Linq;
using Android.Graphics;
using Android.Views;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Bitmap = Android.Graphics.Bitmap;
using Color = Android.Graphics.Color;
using Math = Java.Lang.Math;
using Point = Mapsui.Geometries.Point;

namespace Mapsui.Rendering.Android
{
    public class MapRenderer : IRenderer
    {
        public Canvas Canvas { get; set; }

        public MapRenderer()
        {
            RendererFactory.Get = (() => this);
        }

        public void Render(IViewport viewport, IEnumerable<ILayer> layers)
        {
            Render(Canvas, viewport, layers);
        }

        private void Render(Canvas canvas, IViewport viewport, IEnumerable<ILayer> layers)
        {
            layers = layers.ToList();
            VisibleFeatureIterator.IterateLayers(viewport, layers, (v, s, f) => RenderFeature(canvas, v, s, f));

            foreach (var layer in layers)
            {
                if (layer is ITileLayer)
                {
                    var text = (layer as ITileLayer).MemoryCache.TileCount.ToString(CultureInfo.InvariantCulture);
                    var paint = new Paint { TextSize = 60 };
                    canvas.DrawText(text, 40f, 40f, paint);
                    paint.Dispose();
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
            Render(canvas, viewport, layers);
            var stream = new MemoryStream();
            target.Compress(Bitmap.CompressFormat.Png, 100, stream);
            target.Dispose();
            canvas.Dispose();
            return stream;
        }

        private static void RenderFeature(Canvas canvas, IViewport viewport, IStyle style, IFeature feature)
        {
            if (feature.Geometry is IRaster)
            {
                RasterRenderer.Draw(canvas, viewport, style, feature);
            }
            else if (feature.Geometry is Point)
            {
                var point = feature.Geometry as Point;
                var dest = viewport.WorldToScreen(point);
                canvas.DrawCircle((int)dest.X, (int)dest.Y, 20, new Paint{ Color = Color.Blue});
            }
        }
    }
}
