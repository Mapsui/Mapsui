using System;
using Mapsui.Logging;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Styles;
using Mapsui.Widgets;
using Mapsui.Widgets.PerformanceWidget;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class LoggingWidgetRenderer : ISkiaWidgetRenderer, IDisposable
{
    private readonly SKPaint _informationTextPaint;
    private readonly SKPaint _warningTextPaint;
    private readonly SKPaint _errorTextPaint;
    private readonly SKPaint _backgroundPaint;
    private readonly int _textSize;
    private readonly float _levelWidth;
    private readonly SKRect _rect;
    private readonly SKRect _clipRect;

    /// <summary>
    /// Renderer for LoggingWidget
    /// </summary>
    /// <param name="rect">Position of widget on screen</param>
    /// <param name="textSize">Size of text</param>
    /// <param name="informationTextColor">Color for information text</param>
    /// <param name="warningTextColor">Color for warning text</param>
    /// <param name="errorTextColor">Color for error text</param>
    /// <param name="backgroundColor">Color for background</param>
    public LoggingWidgetRenderer(MRect rect, int textSize, Color informationTextColor, Color warningTextColor, Color errorTextColor, Color backgroundColor)
    {
        _rect = rect.ToSkia();
        _clipRect = _rect;
        _clipRect.Inflate(-2, -2);
        _textSize = textSize;

        _informationTextPaint = new SKPaint { Color = informationTextColor.ToSkia(), TextSize = textSize, };
        _warningTextPaint = new SKPaint { Color = warningTextColor.ToSkia(), TextSize = textSize, };
        _errorTextPaint = new SKPaint { Color = errorTextColor.ToSkia(), TextSize = textSize, };
        _backgroundPaint = new SKPaint { Color = backgroundColor.ToSkia(), Style = SKPaintStyle.Fill, };

        _levelWidth = _informationTextPaint.MeasureText(LogLevel.Information.ToString());
    }

    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        var loggingWidget = (LoggingWidget)widget;

        var paint = _backgroundPaint;
        paint.Color = _backgroundPaint.Color.WithAlpha((byte)(255.0f * loggingWidget.Opacity));

        canvas.DrawRect(_rect, paint);

        var line = 0;

        canvas.Save();
        canvas.ClipRect(_clipRect);

        foreach (var entry in loggingWidget.ListOfLogEntries)
        {
            paint = entry.LogLevel switch
            {
                LogLevel.Error => _errorTextPaint,
                LogLevel.Warning => _warningTextPaint,
                _ => _informationTextPaint,
            };

            canvas.DrawText(entry.LogLevel.ToString(), _rect.Left + 2, _rect.Top + (2 * line) + _textSize * (line + 1), paint);
            canvas.DrawText(entry.Description, _rect.Left + 2 + _levelWidth + 4, _rect.Top + (2 * line) + _textSize * (line + 1), paint);

            line++;
        }

        canvas.Restore();
    }

    public virtual void Dispose()
    {
        _informationTextPaint.Dispose();
        _warningTextPaint.Dispose();
        _errorTextPaint.Dispose();
        _backgroundPaint.Dispose();
    }
}
