using Mapsui.Widgets;
using Mapsui.Widgets.Performance;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets
{
    public class PerformanceWidgetRenderer : ISkiaWidgetRenderer
    {
        SKPoint point1, point2, point3, point4, point5;
        SKPaint textPaint;
        SKPaint backgroundPaint;
        float widthHeader;
        string textHeader1, textHeader2, textHeader3, textHeader4, textHeader5;

        public PerformanceWidgetRenderer(float x, float y, int textSize, SKColor textColor, SKColor backgroundColor)
        {
            textHeader1 = "Last ";
            textHeader2 = "Mean ";
            textHeader3 = "Min ";
            textHeader4 = "Max ";
            textHeader5 = "Count ";

            textPaint = new SKPaint { Color = textColor, TextSize = textSize, };
            backgroundPaint = new SKPaint { Color = backgroundColor, Style = SKPaintStyle.Fill, };

            widthHeader = textPaint.MeasureText(textHeader5);

            point1 = new SKPoint(x, y + textSize);
            point2 = new SKPoint(x, y + 2 * textSize + 2);
            point3 = new SKPoint(x, y + 3 * textSize + 4);
            point4 = new SKPoint(x, y + 4 * textSize + 6);
            point5 = new SKPoint(x, y + 5 * textSize + 8);
        }

        public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget, float layerOpacity)
        {
            var performanceWidget = (PerformanceWidget)widget;
            var text1 = performanceWidget.Performance.LastDrawingTime.ToString("0.000") + " ms";
            var text2 = performanceWidget.Performance.Mean.ToString("0.000") + " ms";
            var text3 = performanceWidget.Performance.Min.ToString("0.000") + " ms";
            var text4 = performanceWidget.Performance.Max.ToString("0.000") + " ms";
            var text5 = performanceWidget.Performance.Count.ToString("0");
            var width1 = textPaint.MeasureText(text1);
            var width2 = textPaint.MeasureText(text2);
            var width3 = textPaint.MeasureText(text3);
            var width4 = textPaint.MeasureText(text4);
            var width5 = textPaint.MeasureText(text5);
            var width = System.Math.Max(width1, System.Math.Max(width2, System.Math.Max(width3, System.Math.Max(width4, width5))));
            var rect = new SKRect(point1.X - 2, point1.Y - 12 , point1.X + widthHeader + width + 2, point5.Y + 2);

            var paint = backgroundPaint;
            paint.Color = backgroundPaint.Color.WithAlpha((byte)(255.0f * performanceWidget.Opacity));
            canvas.DrawRect(rect, paint);
            canvas.DrawText(textHeader1, point1, textPaint);
            canvas.DrawText(text1, new SKPoint(point1.X + widthHeader + width - width1, point1.Y), textPaint);
            canvas.DrawText(textHeader2, point2, textPaint);
            canvas.DrawText(text2, new SKPoint(point2.X + widthHeader + width - width2, point2.Y), textPaint);
            canvas.DrawText(textHeader3, point3, textPaint);
            canvas.DrawText(text3, new SKPoint(point3.X + widthHeader + width - width3, point3.Y), textPaint);
            canvas.DrawText(textHeader4, point4, textPaint);
            canvas.DrawText(text4, new SKPoint(point4.X + widthHeader + width - width4, point4.Y), textPaint);
            canvas.DrawText(textHeader5, point5, textPaint);
            canvas.DrawText(text5, new SKPoint(point5.X + widthHeader + width - width5, point5.Y), textPaint);

            widget.Envelope = new Geometries.BoundingBox(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }
    }
}