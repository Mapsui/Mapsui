using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class TextBoxWidgetRenderer : ISkiaWidgetRenderer
{
    public virtual void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService,
        float layerOpacity)
    {
        DrawText(canvas, viewport, widget, layerOpacity);
    }

    public static void DrawText(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        var textBox = (TextBoxWidget)widget;

        if (string.IsNullOrEmpty(textBox.Text)) return;

        using var textPaint = new SKPaint { TextSize = (float)textBox.TextSize, Color = textBox.TextColor.ToSkia(layerOpacity), IsAntialias = true };
        using var backPaint = new SKPaint { Color = textBox.BackColor.ToSkia(layerOpacity), IsAntialias = true };
        // The textRect has an offset which can be confusing. 
        // This is because DrawText's origin is the baseline of the text, not the bottom.
        // Read more here: https://developer.xamarin.com/guides/xamarin-forms/advanced/skiasharp/basics/text/
        var textRect = new SKRect();
        textPaint.MeasureText(textBox.Text, ref textRect);
        // The backRect is straight forward. It is leading for our purpose.

        var paddingX = textBox.Padding.Left;
        var paddingY = textBox.Padding.Top;

        if (textBox.Width != 0)
        {
            // TextBox has a width, so use this
            paddingX = (textBox.Width - textRect.Width) / 2.0f;
            textRect = new SKRect(textRect.Left, textRect.Top, (float)(textRect.Left + textBox.Width - paddingX * 2), textRect.Bottom);
        }

        if (textBox.Height != 0)
        {
            // TextBox has a height, so use this
            paddingY = (textBox.Height - textPaint.TextSize) / 2.0f;
            textRect = new SKRect(textRect.Left, textRect.Top, textRect.Right, (float)(textRect.Top + textBox.Height - paddingY * 2));
        }

        // Calc Envelope by Width/Height or, if not set, by size of content
        textBox.UpdateEnvelope(
            textBox.Width != 0 ? textBox.Width : textRect.Width + textBox.Padding.Left + textBox.Padding.Right,
            textBox.Height != 0 ? textBox.Height : textRect.Height + textBox.Padding.Top + textBox.Padding.Bottom,
            viewport.Width,
            viewport.Height);

        if (textBox.Envelope == null)
            return;

        canvas.DrawRoundRect(textBox.Envelope.ToSkia(), (float)textBox.CornerRadius, (float)textBox.CornerRadius, backPaint);

        // To position the text within the backRect correct using the textRect's offset.
        canvas.DrawText(textBox.Text,
            (float)(textBox.Envelope.MinX - textRect.Left + paddingX),
            (float)(textBox.Envelope.MinY - textRect.Top + paddingY), textPaint);
    }
}
