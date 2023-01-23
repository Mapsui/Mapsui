using System;
using Mapsui.Widgets;
using Mapsui.Widgets.PerformanceWidget;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class PerformanceWidgetRenderer : ISkiaWidgetRenderer, IDisposable
{
    private readonly SKPaint _textPaint;
    private readonly SKPaint _backgroundPaint;
    private readonly int _textSize;
    private readonly float _widthHeader;
    private readonly string[] _textHeader = { "Last", "Mean", "Frames", "Min", "Max", "Count", "Dropped" };
    private readonly string[] _text = new string[7];
    private readonly SKRect _rect;
    private readonly MRect _envelope;

    /// <summary>
    /// Renderer for PerformanceWidget
    /// </summary>
    /// <param name="x">X position of widget on screen</param>
    /// <param name="y">Y position of widget on screen</param>
    /// <param name="textSize">Size of text</param>
    /// <param name="textColor">Color for text</param>
    /// <param name="backgroundColor">Color for background</param>
    public PerformanceWidgetRenderer(float x, float y, int textSize, SKColor textColor, SKColor backgroundColor)
    {
        _textSize = textSize;

        _textPaint = new SKPaint { Color = textColor, TextSize = textSize, };
        _backgroundPaint = new SKPaint { Color = backgroundColor, Style = SKPaintStyle.Fill, };

        for (var i = 0; i < _textHeader.Length; i++)
            _widthHeader = System.Math.Max(_widthHeader, _textPaint.MeasureText(_textHeader[5]));

        var width = _widthHeader + 20 + _textPaint.MeasureText("0000.000");

        _rect = new SKRect(x, y, x + width + 4, y + _textHeader.Length * (textSize + 2) - 2 + 4);

        _envelope = new MRect(_rect.Left, _rect.Top, _rect.Right, _rect.Bottom);
    }

    public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget, float layerOpacity)
    {
        var performanceWidget = (PerformanceWidget)widget;

        _text[0] = performanceWidget.Performance.LastDrawingTime.ToString("0.000 ms");
        _text[1] = performanceWidget.Performance.Mean.ToString("0.000 ms");
        _text[2] = performanceWidget.Performance.FPS.ToString("0 fps");
        _text[3] = performanceWidget.Performance.Min.ToString("0.000 ms");
        _text[4] = performanceWidget.Performance.Max.ToString("0.000 ms");
        _text[5] = performanceWidget.Performance.Count.ToString("0");
        _text[6] = performanceWidget.Performance.Dropped.ToString("0");

        var paint = _backgroundPaint;
        paint.Color = _backgroundPaint.Color.WithAlpha((byte)(255.0f * performanceWidget.Opacity));

        canvas.DrawRect(_rect, paint);

        for (var i = 0; i < _textHeader.Length; i++)
        {
            canvas.DrawText(_textHeader[i], _rect.Left + 2, _rect.Top + 2 * i + _textSize * (i + 1), _textPaint);
            canvas.DrawText(_text[i], _rect.Right - 2 - _textPaint.MeasureText(_text[i]), _rect.Top + (2 + _textSize) * (i + 1), _textPaint);
        }

        widget.Envelope = _envelope;
    }

    public virtual void Dispose()
    {
        _textPaint.Dispose();
        _backgroundPaint.Dispose();
    }
}
