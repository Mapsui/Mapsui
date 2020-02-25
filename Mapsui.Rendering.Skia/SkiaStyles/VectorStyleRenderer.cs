using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;
using System.Collections.Generic;
using System.Reflection;

namespace Mapsui.Rendering.Skia.SkiaStyles
{
    /// <summary>
    /// Demo VectorStyleRenderer
    /// </summary>
    public class VectorStyleRenderer : ISkiaStyleRenderer
    {
        private MapRenderer mapRenderer;
        private static bool flag = false;

        public VectorStyleRenderer(MapRenderer renderer)
        {
            mapRenderer = renderer;
        }

        public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache)
        {
            if (feature.Geometry is Point destination)
            {
                // Draws a dounat at point
                DrawPoint(canvas, viewport.WorldToScreen(destination), (VectorStyle)style, (float)layer.Opacity);
                return true;
            }
            else if (feature.Geometry is Raster raster)
            {
                // Draws a pink frame and cross over raster image
                DrawRaster(canvas, viewport, layer, feature, style);
                return true;
            }

            return false;
        }

        public static void DrawPoint(SKCanvas canvas, Point destination, VectorStyle vectorStyle, float opacity)
        {
            var width = (float)SymbolStyle.DefaultWidth;
            var halfWidth = width / 2;

            var linePaint = CreateLinePaint(vectorStyle.Outline, opacity);
            var fillPaint = CreateLinePaint(vectorStyle.Outline, opacity);

            linePaint.StrokeWidth = halfWidth / 2;
            fillPaint.StrokeWidth = halfWidth / 4;
            fillPaint.Color = SKColors.Pink;

            canvas.Translate((float)destination.X, (float)destination.Y);

            if (linePaint != null && linePaint.Color.Alpha != 0) canvas.DrawCircle(0, 0, halfWidth, linePaint);
            if (fillPaint != null && fillPaint.Color.Alpha != 0) canvas.DrawCircle(0, 0, halfWidth, fillPaint);
        }

        public void DrawRaster(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style)
        {
            // Get private fields
            var tileCache = (IDictionary<object, BitmapInfo>)typeof(MapRenderer).GetField("_tileCache", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mapRenderer);
            var currentIteration = (long)typeof(MapRenderer).GetField("_currentIteration", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(mapRenderer);
            // Call original renderer
            RasterRenderer.Draw(canvas, viewport, style, feature, (float)layer.Opacity * style.Opacity, tileCache, currentIteration);

            var raster = feature.Geometry as Raster;
            var destination = new BoundingBox(viewport.WorldToScreen(raster.BoundingBox.TopLeft), viewport.WorldToScreen(raster.BoundingBox.BottomRight)).ToSkia();
            var paint = new SKPaint() { Color = SKColors.Pink, Style = SKPaintStyle.Stroke, StrokeWidth = 3 };
            var boundingBox = raster.BoundingBox;

            if (viewport.IsRotated)
            {
                var priorMatrix = canvas.TotalMatrix;
                var matrix = CreateRotationMatrix(viewport, boundingBox, priorMatrix);

                canvas.SetMatrix(matrix);

                destination = new BoundingBox(0.0, 0.0, boundingBox.Width, boundingBox.Height).ToSkia();
            }

            if (flag)
            {
                canvas.DrawRect(destination, paint);
                canvas.DrawLine(new SKPoint(destination.Left, destination.Top), new SKPoint(destination.Right, destination.Bottom), paint);
                canvas.DrawLine(new SKPoint(destination.Left, destination.Bottom), new SKPoint(destination.Right, destination.Top), paint);
            }

            // Next time do it the other way 
            flag = !flag;
        }

        private static SKPaint CreateLinePaint(Pen outline, float opacity)
        {
            if (outline == null) return null;

            return new SKPaint
            {
                Color = outline.Color.ToSkia(opacity),
                StrokeWidth = (float)outline.Width,
                StrokeCap = outline.PenStrokeCap.ToSkia(),
                PathEffect = outline.PenStyle.ToSkia((float)outline.Width),
                Style = SKPaintStyle.Stroke,
                IsAntialias = true
            };
        }

        private static SKPaint CreateFillPaint(Brush fill, float opacity)
        {
            if (fill == null) return null;

            return new SKPaint
            {
                Color = fill.Color.ToSkia(opacity),
                Style = SKPaintStyle.Fill,
                IsAntialias = true
            };
        }

        private static SKMatrix CreateRotationMatrix(IReadOnlyViewport viewport, BoundingBox boundingBox, SKMatrix priorMatrix)
        {
            SKMatrix matrix = SKMatrix.MakeIdentity();

            // The front-end sets up the canvas with a matrix based on screen scaling (e.g. retina).
            // We need to retain that effect by combining our matrix with the incoming matrix.

            // We'll create four matrices in addition to the incoming matrix. They perform the
            // zoom scale, focal point offset, user rotation and finally, centering in the screen.

            var userRotation = SKMatrix.MakeRotationDegrees((float)viewport.Rotation);
            var focalPointOffset = SKMatrix.MakeTranslation(
                (float)(boundingBox.Left - viewport.Center.X),
                (float)(viewport.Center.Y - boundingBox.Top));
            var zoomScale = SKMatrix.MakeScale((float)(1.0 / viewport.Resolution), (float)(1.0 / viewport.Resolution));
            var centerInScreen = SKMatrix.MakeTranslation((float)(viewport.Width / 2.0), (float)(viewport.Height / 2.0));

            // We'll concatenate them like so: incomingMatrix * centerInScreen * userRotation * zoomScale * focalPointOffset

            SKMatrix.Concat(ref matrix, zoomScale, focalPointOffset);
            SKMatrix.Concat(ref matrix, userRotation, matrix);
            SKMatrix.Concat(ref matrix, centerInScreen, matrix);
            SKMatrix.Concat(ref matrix, priorMatrix, matrix);

            return matrix;
        }
    }
}
