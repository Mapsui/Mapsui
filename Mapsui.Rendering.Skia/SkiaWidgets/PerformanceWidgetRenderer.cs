using Mapsui.Widgets;
using Mapsui.Widgets.Performance;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets
{
    public class PerformanceWidgetRenderer : ISkiaWidgetRenderer
    {
        SKPoint point1, point2;
        SKPaint textPaint;
        SKPaint backgroundPaint;

        public PerformanceWidgetRenderer(float x, float y, int textSize, SKColor textColor, SKColor backgroundColor)
        {
            point1 = new SKPoint(x, y + textSize);
            point2 = new SKPoint(x, y + 2 * textSize + 2);
            textPaint = new SKPaint { Color = textColor, TextSize = textSize, };
            backgroundPaint = new SKPaint { Color = backgroundColor, Style = SKPaintStyle.Fill, };
        }

        public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget, float layerOpacity)
        {
            var performanceWidget = (PerformanceWidget)widget;
            var text1 = "Last: " + performanceWidget.Performance.Mean.ToString("0.000") + " ms";
            var text2 = "Count: " + performanceWidget.Performance.Count.ToString("0000");
            var width1 = textPaint.MeasureText(text1) + 4;
            var width2 = textPaint.MeasureText(text2) + 4;
            var width = System.Math.Max(width1, width2);
            var rect = new SKRect(point1.X - 2, point1.Y - 12 , point1.X + width - 2, point2.Y + 2);

            canvas.DrawRect(rect, backgroundPaint);
            canvas.DrawText(text1, point1, textPaint);
            canvas.DrawText(text2, point2, textPaint);
        }
    }
}