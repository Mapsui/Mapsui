using Mapsui.Widgets;
using Mapsui.Widgets.DrawingTime;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets
{
    public class DrawingTimeWidgetRenderer : ISkiaWidgetRenderer
    {
        SKPoint point;
        SKPaint textPaint;
        SKPaint backgroundPaint;

        public DrawingTimeWidgetRenderer(float x, float y, int textSize, SKColor textColor, SKColor backgroundColor)
        {
            point = new SKPoint(x, y + textSize);
            textPaint = new SKPaint { Color = textColor, TextSize = textSize, };
            backgroundPaint = new SKPaint { Color = backgroundColor, Style = SKPaintStyle.Fill, };
        }

        public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget, float layerOpacity)
        {
            var drawingTimewidget = (DrawingTimeWidget)widget;
            var text = drawingTimewidget.LastDrawingTime.ToString("0.000") + " ms";
            var width = textPaint.MeasureText(text) + 4;
            var rect = new SKRect(point.X - 2, point.Y - 12 , point.X + width - 2, point.Y + 2);

            canvas.DrawRect(rect, backgroundPaint);
            canvas.DrawText(text, point, textPaint);
        }
    }
}