using Mapsui.Logging;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.LoggingWidget;
using SkiaSharp;
using System;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class LoggingWidgetRenderer : ISkiaWidgetRenderer, IDisposable
{
    private readonly SKPaint _informationTextPaint;
    private readonly SKPaint _warningTextPaint;
    private readonly SKPaint _errorTextPaint;
    private readonly SKPaint _backgroundPaint;

    private float _levelWidth;

    /// <summary>
    /// Renderer for LoggingWidget
    /// </summary>
    public LoggingWidgetRenderer()
    {
        _informationTextPaint = new SKPaint { Color = SKColors.Black, TextSize = 12, };
        _warningTextPaint = new SKPaint { Color = SKColors.Orange, TextSize = 12, };
        _errorTextPaint = new SKPaint { Color = SKColors.Red, TextSize = 12, };
        _backgroundPaint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill, };

        _levelWidth = _informationTextPaint.MeasureText(LogLevel.Information.ToString());
    }

    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, float layerOpacity)
    {
        var loggingWidget = (LoggingWidget)widget;

        if (!loggingWidget.Enabled)
            return;

        UpdateSettings(loggingWidget);

        var marginX = loggingWidget.MarginX;
        var marginY = loggingWidget.MarginY;
        var width = loggingWidget.Width;
        var height = loggingWidget.Height;
        var paddingX = loggingWidget.PaddingX;
        var paddingY = loggingWidget.PaddingY;

        var rect = new SKRect(marginX, marginY, marginX + width, marginY + height);

        canvas.DrawRect(rect, _backgroundPaint);

        rect.Inflate(-paddingX, -paddingY);

        var line = 0;

        canvas.Save();
        canvas.ClipRect(rect);

        foreach (var entry in loggingWidget.ListOfLogEntries)
        {
            var paint = entry.LogLevel switch
            {
                LogLevel.Error => _errorTextPaint,
                LogLevel.Warning => _warningTextPaint,
                _ => _informationTextPaint,
            };

            canvas.DrawText(entry.LogLevel.ToString(), marginX + paddingX, marginY + (paddingX * line) + loggingWidget.TextSize * (line + 1), paint);
            canvas.DrawText(entry.Description, marginX + paddingX + _levelWidth + 2 * paddingX, marginY + (paddingY * line) + loggingWidget.TextSize * (line + 1), paint);

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

    private void UpdateSettings(LoggingWidget loggingWidget)
    {
        _backgroundPaint.Color = loggingWidget.BackgroundColor.ToSkia().WithAlpha((byte)(255.0f * loggingWidget.Opacity));
        _errorTextPaint.Color = loggingWidget.ErrorTextColor.ToSkia();
        _errorTextPaint.TextSize = loggingWidget.TextSize;
        _warningTextPaint.Color = loggingWidget.WarningTextColor.ToSkia();
        _warningTextPaint.TextSize = loggingWidget.TextSize;
        _informationTextPaint.Color = loggingWidget.InformationTextColor.ToSkia();
        _informationTextPaint.TextSize = loggingWidget.TextSize;

        _levelWidth = _informationTextPaint.MeasureText(LogLevel.Information.ToString());
    }
}
