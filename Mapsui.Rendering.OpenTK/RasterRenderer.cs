using System;
using System.Diagnostics;
using Mapsui.Geometries;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Rendering.OpenTK
{
    struct RectF
    {
        public RectF(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public float MinX;
        public float MinY;
        public float MaxX;
        public float MaxY;
    }

    public static class RasterRenderer
    {
        public static void Draw(IViewport viewport, IStyle style, IFeature feature)
        {
            try
            {
                var raster = (IRaster)feature.Geometry;
                if (!feature.RenderedGeometry.ContainsKey(style))
                {
                    var textureId = PointRenderer.LoadTexture(raster.Data);
                    feature.RenderedGeometry[style] = textureId;
                }
                
                var dest = WorldToScreen(viewport, feature.Geometry.GetBoundingBox());
                dest = new BoundingBox(
                    dest.MinX,
                    dest.MinY,
                    dest.MaxX,
                    dest.MaxY);

                var destination = RoundToPixel(dest);

                PointRenderer.RenderTexture((int)feature.RenderedGeometry[style], ToVertexArray(destination));

                //DrawOutline(style, destination);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
        }

        private static object LoadBitmap()
        {
            throw new NotImplementedException();
        }

        //private static void DrawOutline(Canvas canvas, IStyle style, RectF destination)
        //{
        //    var vectorStyle = (style as VectorStyle);
        //    if (vectorStyle == null) return;
        //    if (vectorStyle.Outline == null) return;
        //    if (vectorStyle.Outline.Color == null) return;
        //    DrawRectangle(canvas, destination, vectorStyle.Outline.Color);
        //}

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

        private static RectF RoundToPixel(BoundingBox dest)
        {
            return new RectF(
                (float)Math.Round(dest.Left),
                (float)Math.Round(Math.Min(dest.Top, dest.Bottom)),
                (float)Math.Round(dest.Right),
                (float)Math.Round(Math.Max(dest.Top, dest.Bottom)));
        }

        private static float[] ToVertexArray(RectF rect)
        {
            return new[]
            {
                rect.MinX, rect.MinY,
                rect.MaxX, rect.MinY,
                rect.MaxX, rect.MaxY,
                rect.MinX, rect.MaxY
            };
        }

        //private static AndroidBitmap ToAndroidBitmap(IGeometry geometry)
        //{
        //    var raster = (IRaster)geometry;
        //    var rasterData = raster.Data.ToArray();
        //    var bitmap = BitmapFactory.DecodeByteArray(rasterData, 0, rasterData.Length);
        //    return bitmap;
        //}
    }
}
