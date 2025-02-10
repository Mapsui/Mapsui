using Mapsui.Logging;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using SkiaSharp;
using Topten.RichTextKit;

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

        using var skFont = new SKFont() { Size = (float)textBox.TextSize };
        using var textPaint = new SKPaint { Color = textBox.TextColor.ToSkia(layerOpacity), IsAntialias = true };
        using var backPaint = new SKPaint { Color = textBox.BackColor.ToSkia(layerOpacity), IsAntialias = true };
        // The textRect has an offset which can be confusing. 
        // This is because DrawText's origin is the baseline of the text, not the bottom.
        // Read more here: https://developer.xamarin.com/guides/xamarin-forms/advanced/skiasharp/basics/text/
        skFont.MeasureText(textBox.Text, out var textRect, textPaint);
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
            paddingY = (textBox.Height - skFont.Size) / 2.0f;
            textRect = new SKRect(textRect.Left, textRect.Top, textRect.Right, (float)(textRect.Top + textBox.Height - paddingY * 2));
        }

        var width = textBox.Width != 0 ? textBox.Width : textRect.Width + textBox.Padding.Left + textBox.Padding.Right;
        var height = textBox.Height != 0 ? textBox.Height : textRect.Height + textBox.Padding.Top + textBox.Padding.Bottom;
        // Calc Envelope by Width/Height or, if not set, by size of content
        textBox.UpdateEnvelope(width, height, viewport.Width, viewport.Height);

        if (textBox.Envelope == null)
            return;

        canvas.DrawRoundRect(textBox.Envelope.ToSkia(), (float)textBox.CornerRadius, (float)textBox.CornerRadius, backPaint);

        // To position the text within the backRect correct using the textRect's offset.
        var x = (float)(textBox.Envelope.MinX + paddingX);
        var y = (float)(((textBox.Envelope.MinY + textBox.Envelope.MaxY) / 2) - paddingY);

        Logger.Log(LogLevel.Information, $"TextBoxWidgetRenderer.DrawText: x={x}, y={y}");


        //!!!canvas.DrawText(textBox.Text, x, y, skFont, textPaint);

        var style = new Style
        {
            FontSize = (float)textBox.TextSize,
            TextColor = textBox.TextColor.ToSkia(layerOpacity)
        };
        var textBlock = new TextBlock();
        textBlock.AddText(textBox.Text, style);
        textBlock.Paint(canvas, new SKPoint(x, y), new TextPaintOptions() { Edging = SKFontEdging.Antialias });
    }
}
