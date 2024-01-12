﻿using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidget;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class BoxWidgetRenderer : ISkiaWidgetRenderer
{
    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        var boxWidget = (BoxWidget)widget;
        using var backPaint = new SKPaint { Color = boxWidget.BackColor.ToSkia(layerOpacity), IsAntialias = true };
        // The textRect has an offset which can be confusing. 
        // This is because DrawText's origin is the baseline of the text, not the bottom.
        // Read more here: https://developer.xamarin.com/guides/xamarin-forms/advanced/skiasharp/basics/text/

        // The backRect is straight forward. It is leading for our purpose.
        var backRect = new SKRect(0, 0,
            (float)boxWidget.Width,
            (float)boxWidget.Height); // Use the font's TextSize for consistency
        var offsetX = TextBoxWidgetRenderer.GetOffsetX(backRect.Width, (float)boxWidget.MarginX, boxWidget.HorizontalAlignment, viewport.Width);
        var offsetY = TextBoxWidgetRenderer.GetOffsetY(backRect.Height, (float)boxWidget.MarginY, boxWidget.VerticalAlignment, viewport.Height);
        backRect.Offset((float)offsetX, (float)offsetY);
        canvas.DrawRoundRect(backRect, (float)boxWidget.CornerRadius, (float)boxWidget.CornerRadius, backPaint);
        boxWidget.Envelope = backRect.ToMRect();
    }
}
