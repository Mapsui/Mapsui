using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.Images;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class ImageButtonWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService, float layerOpacity)
    {
        var button = (ImageButtonWidget)widget;

        if (button.ImageSource == null)
            throw new InvalidOperationException("ImageSource is not set");

        var drawableImage = renderService.DrawableImageCache.GetOrCreate(button.ImageSource,
            () => SymbolStyleRenderer.TryCreateDrawableImage(button.ImageSource, renderService.ImageSourceCache));
        if (drawableImage == null)
            return;

        button.UpdateEnvelope(
            button.Width != 0 ? button.Width : drawableImage.Width + button.Padding.Left + button.Padding.Right,
            button.Height != 0 ? button.Height : drawableImage.Height + button.Padding.Top + button.Padding.Bottom,
            viewport.Width,

            viewport.Height);

        if (button.Envelope == null)
            return;

        using var backPaint = new SKPaint { Color = button.BackColor.ToSkia(layerOpacity), IsAntialias = true };
        canvas.DrawRoundRect(button.Envelope.ToSkia(), (float)button.CornerRadius, (float)button.CornerRadius, backPaint);

        // Get the scale for picture in each direction
        var scaleX = (button.Envelope.Width - button.Padding.Left - button.Padding.Right) / drawableImage.Width;
        var scaleY = (button.Envelope.Height - button.Padding.Top - button.Padding.Bottom) / drawableImage.Height;


        using var skPaint = new SKPaint { IsAntialias = true };
        if (drawableImage is BitmapImage bitmapImage)
        {
            throw new Exception($"BitmapImage is not supported as {nameof(button.ImageSource)}  or {nameof(ImageButtonWidget)}");
            // Todo: Implement this. It should have a tested sample. Perhaps in a separate ImageButtonWidgetSample. Things like scale and
            // rotation should be tested. Could be something like this:
            // BitmapRenderer.Draw(canvas, bitmapImage.Image,
            //    (float)button.Envelope.Centroid.X, (float)button.Envelope.Centroid.Y, (float)button.Rotation);
        }
        else if (drawableImage is SvgImage svgImage)
        {
            // Rotate picture
            var matrix = SKMatrix.CreateRotationDegrees((float)button.Rotation, drawableImage.Width / 2f, drawableImage.Height / 2f);
            // Create a scale matrix
            matrix = matrix.PostConcat(SKMatrix.CreateScale((float)scaleX, (float)scaleY));
            // Translate picture to right place
            matrix = matrix.PostConcat(SKMatrix.CreateTranslation((float)(button.Envelope.MinX + button.Padding.Left), (float)(button.Envelope.MinY + button.Padding.Top)));
            // Draw picture
            canvas.DrawPicture(svgImage.Picture, ref matrix, skPaint);
        }
        else
            throw new NotSupportedException("DrawableImage type not supported");
    }
}
