using Mapsui.Widgets;
using Mapsui.Widgets.Button;
using SkiaSharp;
using Svg.Skia;
using System;

namespace Mapsui.Rendering.Skia.SkiaWidgets
{
    public class ButtonWidgetRenderer : ISkiaWidgetRenderer
    {
        public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget, float layerOpacity)
        {
            var button = (ButtonWidget)widget;

            if (button.Picture == null && string.IsNullOrEmpty(button.SVGImage))
                return;

            if (button.Picture == null)
                button.Picture = new SKSvg().FromSvg(button.SVGImage);

            var picture = (SKPicture)button.Picture;

            // Get the scale for picture in each direction
            float scaleX = (float)(button.Envelope.Width / picture.CullRect.Width);
            float scaleY = (float)(button.Envelope.Height / picture.CullRect.Height);

            // Rotate picture
            var matrix = SKMatrix.CreateRotationDegrees((float)button.Rotation, picture.CullRect.Width / 2f, picture.CullRect.Height / 2f);

            // Create a scale matrix
            matrix = matrix.PostConcat(SKMatrix.CreateScale(scaleX, scaleY));

            // Translate picture to right place
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)button.Envelope.MinX, (float)button.Envelope.MinY));

            canvas.DrawPicture(picture, ref matrix, new SKPaint() { IsAntialias = true });
        }
    }
}