using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
using SkiaSharp;
using Svg.Skia;

namespace Mapsui.Rendering.Skia.SkiaWidgets
{
    public class ButtonWidgetRenderer : ISkiaWidgetRenderer
    {
        public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget, float layerOpacity)
        {
            var button = (ButtonWidget)widget;

            if (button.Picture == null && string.IsNullOrEmpty(button.SvgImage))
                return;

            if (button.Picture == null)
                button.Picture = new SKSvg().FromSvg(button.SvgImage);

            var picture = button.Picture as SKPicture;

            if (picture == null)
                return;

            // Get the scale for picture in each direction
            float scaleX = (float)(button.Envelope.Width / picture.CullRect.Width);
            float scaleY = (float)(button.Envelope.Height / picture.CullRect.Height);

            // Rotate picture
            var matrix = SKMatrix.CreateRotationDegrees(button.Rotation, picture.CullRect.Width / 2f, picture.CullRect.Height / 2f);

            // Create a scale matrix
            matrix = matrix.PostConcat(SKMatrix.CreateScale(scaleX, scaleY));

            // Translate picture to right place
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)button.Envelope.MinX, (float)button.Envelope.MinY));

            canvas.DrawPicture(picture, ref matrix, new SKPaint() { IsAntialias = true });
        }
    }
}