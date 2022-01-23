using System;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    internal class PictureRenderer
    {
        // The field below is static for performance. Effect has not been measured.
        // Note that the default FilterQuality is None. Setting it explicitly to Low increases the quality.
        private static readonly SKPaint DefaultPaint = new SKPaint { FilterQuality = SKFilterQuality.Low };

        public static void Draw(SKCanvas canvas, SKPicture picture, SKRect rect, float layerOpacity = 1f)
        {
            var skPaint = GetPaint(layerOpacity, out var dispose);

            var scaleX = rect.Width / picture.CullRect.Width;
            var scaleY = rect.Height / picture.CullRect.Height;
            var matrix = SKMatrix.CreateTranslation(rect.Left, rect.Top);
            matrix.PostConcat(SKMatrix.CreateScale(scaleX, scaleY));

            canvas.DrawPicture(picture, ref matrix, skPaint);
            if (dispose)
            {
                skPaint.Dispose();
            }
        }

        public static void Draw(SKCanvas canvas, SKPicture? picture, float x, float y, float rotation = 0,
            float offsetX = 0, float offsetY = 0,
            LabelStyle.HorizontalAlignmentEnum horizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
            LabelStyle.VerticalAlignmentEnum verticalAlignment = LabelStyle.VerticalAlignmentEnum.Center,
            float opacity = 1f,
            float scale = 1f)
        {
            if (picture == null)
                return;

            canvas.Save();

            canvas.Translate(x, y);
            if (rotation != 0)
                canvas.RotateDegrees(rotation, 0, 0);
            canvas.Scale(scale, scale);

            var width = picture.CullRect.Width;
            var height = picture.CullRect.Height;

            x = offsetX + DetermineHorizontalAlignmentCorrection(horizontalAlignment, width);
            y = -offsetY + DetermineVerticalAlignmentCorrection(verticalAlignment, height);

            var halfWidth = width / 2;
            var halfHeight = height / 2;

            var rect = new SKRect(x - halfWidth, y - halfHeight, x + halfWidth, y + halfHeight);

            Draw(canvas, picture, rect, opacity);

            canvas.Restore();
        }
        private static float DetermineHorizontalAlignmentCorrection(
            LabelStyle.HorizontalAlignmentEnum horizontalAlignment, float width)
        {
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Left) return width / 2;
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right) return -(width / 2);
            return 0; // center
        }

        private static float DetermineVerticalAlignmentCorrection(
            LabelStyle.VerticalAlignmentEnum verticalAlignment, float height)
        {
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Top) return -(height / 2);
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Bottom) return height / 2;
            return 0; // center
        }

        private static SKPaint GetPaint(float layerOpacity, out bool dispose)
        {
            if (Math.Abs(layerOpacity - 1) > Utilities.Constants.Epsilon)
            {
                // Unfortunately for opacity we need to set the Color and the Color
                // is part of the Paint object. So we need to recreate the paint on
                // every draw. 
                dispose = true;
                return new SKPaint
                {
                    FilterQuality = SKFilterQuality.Low,
                    Color = new SKColor(255, 255, 255, (byte)(255 * layerOpacity))
                };
            }
            dispose = false;
            return DefaultPaint;
        }

        public static void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IStyle style, PictureFeature pictureFeature, float layerOpacity)
        {
            var opacity = layerOpacity * style.Opacity;
            if (pictureFeature.Picture != null && pictureFeature.Extent != null)
            {
                var picture = (SKPicture)pictureFeature.Picture;
                var extent = pictureFeature.Extent!;
                if (viewport.IsRotated)
                {
                    var priorMatrix = canvas.TotalMatrix;

                    var matrix = RasterRenderer.CreateRotationMatrix(viewport, extent, priorMatrix);

                    canvas.SetMatrix(matrix);

                    var destination = new BoundingBox(0.0, 0.0, extent.Width, extent.Height);

                    PictureRenderer.Draw(canvas, picture, RasterRenderer.RoundToPixel(destination).ToSkia(), opacity);

                    canvas.SetMatrix(priorMatrix);
                }
                else
                {
                    var destination = RasterRenderer.WorldToScreen(viewport, extent);
                    PictureRenderer.Draw(canvas, picture, RasterRenderer.RoundToPixel(destination).ToSkia(), opacity);
                }
            }
        }
    }
}
