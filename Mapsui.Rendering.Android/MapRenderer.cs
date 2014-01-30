using Android.Graphics;
using Android.Media;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using Bitmap = Android.Graphics.Bitmap;
using Math = Java.Lang.Math;

namespace Mapsui.Rendering.Android
{
    public class MapRenderer : IRenderer
    {
        public Canvas Canvas { get; set; }

        public MapRenderer()
        {
            RendererFactory.Get = (() => this);
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
            Render(Canvas, viewport, layers);
        }

        private void Render(Canvas canvas, IViewport viewport, IEnumerable<ILayer> layers)
        {
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
                if (!feature.RenderedGeometry.ContainsKey(style)) feature.RenderedGeometry[style] = ToAndroidBitmap(feature.Geometry);
                var bitmap = (Bitmap)feature.RenderedGeometry[style];

                var dest = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                dest = new BoundingBox(
                    dest.MinX,
                    dest.MinY,
                    dest.MaxX,
                    dest.MaxY);
               
                var destination = RoundToPixel(dest);
                canvas.DrawBitmap(bitmap, null, destination, null);

                //!!!DrawRectangle(destination);
            }
        }

        private void DrawRectangle(RectF destination)
        {
            var paint = new Paint();
            paint.SetStyle(Paint.Style.Stroke);
            paint.Color = global::Android.Graphics.Color.Red;
            paint.StrokeWidth = 4;
            Canvas.DrawRect(destination, paint);
        }

        private static Bitmap ToAndroidBitmap(IGeometry geometry)
        {
            var raster = (IRaster)geometry;
            var rasterData = raster.Data.ToArray();
            var bitmap = BitmapFactory.DecodeByteArray(rasterData, 0, rasterData.Length);
            return bitmap;
        }
    }
}
