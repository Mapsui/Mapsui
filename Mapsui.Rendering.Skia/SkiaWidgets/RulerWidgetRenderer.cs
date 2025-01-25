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
        if (widget is RulerWidget rulerWidget)
        {
            // Use all of the canvas
            rulerWidget.Envelope = new MRect(0, 0, viewport.Width, viewport.Height);

            if (!rulerWidget.IsActive)
                return;

            DrawLine(canvas, viewport, rulerWidget, renderService, layerOpacity);
        }
        else
            throw new Exception($"Widget is not a {nameof(RulerWidget)}");
    }

    public static void DrawLine(SKCanvas canvas, Viewport viewport, RulerWidget rulerWidget, RenderService renderService, float layerOpacity)
    {
        if (rulerWidget.StartPosition is MPoint start && rulerWidget.CurrentPosition is MPoint current && rulerWidget.DistanceInKilometers is not null)
        {
            var distanceInMeters = rulerWidget.DistanceInKilometers;
            var formattedDistance = $"{distanceInMeters:F2} km";

            var screenStart = viewport.WorldToScreen(start);
            var screenCurrent = viewport.WorldToScreen(current);

            rulerWidget.InfoBox.Text = $"Distance: {formattedDistance}";

            if (rulerWidget.ShowInfoNextToRuler)
            {
                var offSet = 4;
                rulerWidget.InfoBox.Margin = new MRect((int)screenCurrent.X + offSet, (int)(viewport.Height - screenCurrent.Y + offSet));
            }
            _textBoxRenderer.Draw(canvas, viewport, rulerWidget.InfoBox, renderService, layerOpacity);

            // Use the envelope to draw
            using var skPaint = new SKPaint { Color = rulerWidget.Color.ToSkia(), StrokeWidth = 3, IsAntialias = true };
            using var skPaintDots = new SKPaint { Color = rulerWidget.ColorOfBeginAndEndDots.ToSkia(), StrokeWidth = 3, IsAntialias = true };
            canvas.DrawCircle((float)screenStart.X, (float)screenStart.Y, 6, skPaintDots);
            canvas.DrawCircle((float)screenCurrent.X, (float)screenCurrent.Y, 6, skPaintDots);
            canvas.DrawLine((float)screenStart.X, (float)screenStart.Y, (float)screenCurrent.X, (float)screenCurrent.Y, skPaint);
        }
    }
}
