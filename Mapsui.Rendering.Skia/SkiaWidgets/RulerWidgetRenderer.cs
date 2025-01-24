using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class RulerWidgetRenderer : ISkiaWidgetRenderer
{
    private static readonly TextBoxWidgetRenderer _textBoxRenderer = new();

    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService, float layerOpacity)
    {
        if (widget is RulerWidget measureWidget)
        {
            // Use all of the canvas
            measureWidget.Envelope = new MRect(0, 0, viewport.Width, viewport.Height);

            if (!measureWidget.IsActive)
                return;

            DrawLine(canvas, viewport, measureWidget, renderService, layerOpacity);
        }
        else
            throw new Exception("Widget is not a MeasureWidget");
    }

    public static void DrawLine(SKCanvas canvas, Viewport viewport, RulerWidget measureWidget, RenderService renderService, float layerOpacity)
    {
        if (measureWidget.StartPosition is MPoint start && measureWidget.CurrentPosition is MPoint current && measureWidget.DistanceInKilometers is not null)
        {
            var distanceInMeters = measureWidget.DistanceInKilometers;
            var formattedDistance = $"{distanceInMeters} km.";

            var screenStart = viewport.WorldToScreen(start);
            var screenCurrent = viewport.WorldToScreen(current);

            measureWidget.TextBox.Text = $"Distance: {formattedDistance}";
            measureWidget.TextBox.Margin = new MRect((int)screenCurrent.X, (int)(viewport.Height - screenCurrent.Y));
            _textBoxRenderer.Draw(canvas, viewport, measureWidget.TextBox, renderService, layerOpacity);

            // Use the envelope to draw
            using var skPaint = new SKPaint { Color = measureWidget.Color.ToSkia(), StrokeWidth = 3, IsAntialias = true };
            using var skPaintDots = new SKPaint { Color = measureWidget.ColorOfBeginAndEndDots.ToSkia(), StrokeWidth = 3, IsAntialias = true };
            canvas.DrawCircle((float)screenStart.X, (float)screenStart.Y, 6, skPaintDots);
            canvas.DrawCircle((float)screenCurrent.X, (float)screenCurrent.Y, 6, skPaintDots);
            canvas.DrawLine((float)screenStart.X, (float)screenStart.Y, (float)screenCurrent.X, (float)screenCurrent.Y, skPaint);
        }
    }
}
