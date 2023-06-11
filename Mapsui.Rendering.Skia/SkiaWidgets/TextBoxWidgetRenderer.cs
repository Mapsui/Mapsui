using System;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using SkiaSharp;
using Topten.RichTextKit;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class TextBoxWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        DrawText(canvas, viewport, widget, layerOpacity);
    }

    public static void DrawText(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        var textBox = (TextBox)widget;
        if (string.IsNullOrEmpty(textBox.Text)) return;
        using var textPaint = new SKPaint { Color = textBox.TextColor.ToSkia(layerOpacity), IsAntialias = true };
        using var backPaint = new SKPaint { Color = textBox.BackColor.ToSkia(layerOpacity) };
        // The textRect has an offset which can be confusing. 
        // This is because DrawText's origin is the baseline of the text, not the bottom.
        // Read more here: https://developer.xamarin.com/guides/xamarin-forms/advanced/skiasharp/basics/text/
        var textRect = new SKRect();
        textPaint.MeasureText(textBox.Text, ref textRect);
        // The backRect is straight forward. It is leading for our purpose.

        float paddingX = textBox.PaddingX;
        float paddingY = textBox.PaddingY;

        if (textBox.Width != null)
        {
            paddingX = (textBox.Width.Value - textRect.Width) / 2.0f;
        }

        if (textBox.Height != null)
        {
            paddingY = (textBox.Height.Value - textPaint.TextSize) / 2.0f;
        }

        var backRect = new SKRect(0, 0,
            textRect.Width + paddingX * 2,
            textPaint.TextSize + paddingY * 2); // Use the font's TextSize for consistency
        var offsetX = GetOffsetX(backRect.Width, textBox.MarginX, textBox.HorizontalAlignment, viewport.Width);
        var offsetY = GetOffsetY(backRect.Height, textBox.MarginY, textBox.VerticalAlignment, viewport.Height);
        backRect.Offset(offsetX, offsetY);
        canvas.DrawRoundRect(backRect, textBox.CornerRadius, textBox.CornerRadius, backPaint);
        textBox.Envelope = backRect.ToMRect();
        // To position the text within the backRect correct using the textRect's offset.
        canvas.DrawText(textBox.Text,
            offsetX - textRect.Left + paddingX,
            offsetY - textRect.Top + paddingY, textPaint);
    }

    public static float GetOffsetX(float width, float offsetX, HorizontalAlignment horizontalAlignment, double screenWidth)
    {
        if (horizontalAlignment == HorizontalAlignment.Left) return offsetX;
        if (horizontalAlignment == HorizontalAlignment.Right) return (float)(screenWidth - width - offsetX);
        if (horizontalAlignment == HorizontalAlignment.Center) return (float)(screenWidth * 0.5 - width * 0.5); // ignore offset
        throw new Exception($"Unknown {nameof(HorizontalAlignment)} type");
    }

    public static float GetOffsetY(float height, float offsetY, VerticalAlignment verticalAlignment, double screenHeight)
    {
        if (verticalAlignment == VerticalAlignment.Top) return offsetY;
        if (verticalAlignment == VerticalAlignment.Bottom) return (float)(screenHeight - height - offsetY);
        if (verticalAlignment == VerticalAlignment.Center) return (float)(screenHeight * 0.5 - height * 0.5); // ignore offset
        throw new Exception($"Unknown {nameof(VerticalAlignment)} type");
    }
}
