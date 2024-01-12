using System;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class TextBoxWidgetRenderer : ISkiaWidgetRenderer
{
    public virtual void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        DrawText(canvas, viewport, widget, layerOpacity);
    }

    public static void DrawText(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        var textBox = (TextBox)widget;
        if (string.IsNullOrEmpty(textBox.Text)) return;
        using var textPaint = new SKPaint { Color = textBox.TextColor.ToSkia(layerOpacity), IsAntialias = true };
        using var backPaint = new SKPaint { Color = textBox.BackColor.ToSkia(layerOpacity), IsAntialias = true };
        // The textRect has an offset which can be confusing. 
        // This is because DrawText's origin is the baseline of the text, not the bottom.
        // Read more here: https://developer.xamarin.com/guides/xamarin-forms/advanced/skiasharp/basics/text/
        var textRect = new SKRect();
        textPaint.MeasureText(textBox.Text, ref textRect);
        // The backRect is straight forward. It is leading for our purpose.

        var paddingX = textBox.PaddingX;
        var paddingY = textBox.PaddingY;

        if (textBox.Width != null)
        {
            paddingX = (textBox.Width.Value - textRect.Width) / 2.0f;
        }

        if (textBox.Height != null)
        {
            paddingY = (textBox.Height.Value - textPaint.TextSize) / 2.0f;
        }

        var backRect = new SKRect(0, 0,
            (float)(textRect.Width + paddingX * 2),
            (float)(textPaint.TextSize + paddingY * 2)); // Use the font's TextSize for consistency
        var offsetX = GetOffsetX(backRect.Width, (float)textBox.MarginX, textBox.HorizontalAlignment, viewport.Width);
        var offsetY = GetOffsetY(backRect.Height, (float)textBox.MarginY, textBox.VerticalAlignment, viewport.Height);
        backRect.Offset((float)offsetX, (float)offsetY);
        canvas.DrawRoundRect(backRect, (float)textBox.CornerRadius, (float)textBox.CornerRadius, backPaint);
        textBox.Envelope = backRect.ToMRect();
        // To position the text within the backRect correct using the textRect's offset.
        canvas.DrawText(textBox.Text,
            (float)(offsetX - textRect.Left + paddingX),
            (float)(offsetY - textRect.Top + paddingY), textPaint);
    }

    public static double GetOffsetX(double width, double offsetX, HorizontalAlignment horizontalAlignment, double screenWidth)
    {
        if (horizontalAlignment == HorizontalAlignment.Left) return offsetX;
        if (horizontalAlignment == HorizontalAlignment.Right) return screenWidth - width - offsetX;
        if (horizontalAlignment == HorizontalAlignment.Center) return screenWidth * 0.5 - width * 0.5; // ignore offset
        throw new Exception($"Unknown {nameof(HorizontalAlignment)} type");
    }

    public static double GetOffsetY(double height, double offsetY, VerticalAlignment verticalAlignment, double screenHeight)
    {
        if (verticalAlignment == VerticalAlignment.Top) return offsetY;
        if (verticalAlignment == VerticalAlignment.Bottom) return screenHeight - height - offsetY;
        if (verticalAlignment == VerticalAlignment.Center) return screenHeight * 0.5 - height * 0.5; // ignore offset
        throw new Exception($"Unknown {nameof(VerticalAlignment)} type");
    }
}
