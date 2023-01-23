using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
using SkiaSharp;
using Svg.Skia;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class ButtonWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget, float layerOpacity)
    {
        var button = (ButtonWidget)widget;

        if (button.Picture == null && string.IsNullOrEmpty(button.SvgImage))
            return;

        button.Picture ??= button.SvgImage?.LoadSvg();

        var picture = button.Picture as SKPicture;

        if (picture == null)
            return;

        if (button.Envelope == null)
            return;

        // Get the scale for picture in each direction
        var scaleX = (float)(button.Envelope.Width / picture.CullRect.Width);
        var scaleY = (float)(button.Envelope.Height / picture.CullRect.Height);

        // Rotate picture
        var matrix = SKMatrix.CreateRotationDegrees(button.Rotation, picture.CullRect.Width / 2f, picture.CullRect.Height / 2f);

        // Create a scale matrix
        matrix = matrix.PostConcat(SKMatrix.CreateScale(scaleX, scaleY));

        // Translate picture to right place
        matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)button.Envelope.MinX, (float)button.Envelope.MinY));

        using var skPaint = new SKPaint { IsAntialias = true };
        canvas.DrawPicture(picture, ref matrix, skPaint);
    }
}
