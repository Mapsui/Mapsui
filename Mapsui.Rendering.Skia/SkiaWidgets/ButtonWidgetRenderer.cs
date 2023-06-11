using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Utilities;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
using SkiaSharp;
using Svg.Skia;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class ButtonWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        var button = (ButtonWidget)widget;
        if (!string.IsNullOrEmpty(button.Text))
        {
            var hyperlink = (TextBox)button;
            if (string.IsNullOrEmpty(hyperlink.Text)) { }
            else
            {
                using var textPaint = new SKPaint { Color = hyperlink.TextColor.ToSkia(layerOpacity), IsAntialias = true };
                using var backPaint = new SKPaint { Color = hyperlink.BackColor.ToSkia(layerOpacity) };
                // The textRect has an offset which can be confusing. 
                // This is because DrawText's origin is the baseline of the text, not the bottom.
                // Read more here: https://developer.xamarin.com/guides/xamarin-forms/advanced/skiasharp/basics/text/
                var textRect = new SKRect();
                textPaint.MeasureText(hyperlink.Text, ref textRect);
                // The backRect is straight forward. It is leading for our purpose.
                var backRect = new SKRect(0, 0,
                    textRect.Width + hyperlink.PaddingX * 2,
                    textPaint.TextSize + hyperlink.PaddingY * 2); // Use the font's TextSize for consistency
                var offsetX = TextBoxWidgetRenderer.GetOffsetX(backRect.Width, hyperlink.MarginX, hyperlink.HorizontalAlignment, viewport.Width);
                var offsetY = TextBoxWidgetRenderer.GetOffsetY(backRect.Height, hyperlink.MarginY, hyperlink.VerticalAlignment, viewport.Height);
                backRect.Offset(offsetX, offsetY);
                canvas.DrawRoundRect(backRect, hyperlink.CornerRadius, hyperlink.CornerRadius, backPaint);
                hyperlink.Envelope = backRect.ToMRect();
                // To position the text within the backRect correct using the textRect's offset.
                canvas.DrawText(hyperlink.Text,
                    offsetX - textRect.Left + hyperlink.PaddingX,
                    offsetY - textRect.Top + hyperlink.PaddingY, textPaint);
            }
        }

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
