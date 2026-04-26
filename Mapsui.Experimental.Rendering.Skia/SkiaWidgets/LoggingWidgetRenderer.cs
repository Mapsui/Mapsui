using System;
using Mapsui.Logging;
using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;

namespace Mapsui.Experimental.Rendering.Skia.SkiaWidgets;

public class LoggingWidgetRenderer : ISkiaWidgetRenderer, IDisposable
{
    private readonly SKPaint _informationTextPaint;
    private readonly SKPaint _warningTextPaint;
    private readonly SKPaint _errorTextPaint;
    private readonly SKPaint _backgroundPaint;

    /// <summary>
    /// Renderer for LoggingWidget
    /// </summary>
    public LoggingWidgetRenderer()
    {
        _informationTextPaint = new SKPaint { Color = SKColors.Black };
        _warningTextPaint = new SKPaint { Color = SKColors.Orange, };
        _errorTextPaint = new SKPaint { Color = SKColors.Red, };
        _backgroundPaint = new SKPaint { Color = SKColors.White, Style = SKPaintStyle.Fill, };
    }

    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, Mapsui.Rendering.RenderService renderService, float layerOpacity, SKRect? dirtyScreenRect)
    {
        var loggingWidget = (LoggingWidget)widget;

        if (!loggingWidget.Enabled)
            return;

        UpdateSettings(loggingWidget);
        loggingWidget.UpdateEnvelope(loggingWidget.Width, loggingWidget.Height, viewport.Width, viewport.Height);

        if (loggingWidget.Envelope == null || loggingWidget.Envelope.Width == 0 || loggingWidget.Envelope.Height == 0)
            return;

        var marginX = loggingWidget.Envelope.Left;
        var marginY = loggingWidget.Envelope.Bottom;
        var width = loggingWidget.Envelope.Width;
        var height = loggingWidget.Envelope.Height;
        var paddingX = loggingWidget.Padding.Left;
        var paddingY = loggingWidget.Padding.Top;

        var rect = new SKRect((float)marginX, (float)marginY, (float)(marginX + width), (float)(marginY + height));

        canvas.DrawRect(rect, _backgroundPaint);

        rect = new SKRect((float)(rect.Left + loggingWidget.Padding.Left),
            (float)(rect.Top + loggingWidget.Padding.Top),
            (float)(rect.Right - loggingWidget.Padding.Right),
            (float)(rect.Bottom - loggingWidget.Padding.Bottom));

        using var font = SkiaTextLayoutHelper.CreateSkFont(loggingWidget.Font, (float)loggingWidget.TextSize, renderService);
        var line = 0;
        var effectiveTextSize = font.Size;

        canvas.Save();
        canvas.ClipRect(rect);

        foreach (var entry in loggingWidget.ListOfLogEntries)
        {
            if (entry.LogLevel > loggingWidget.LogLevelFilter)
                continue;

            var top = marginY + (paddingY * line) + effectiveTextSize * (line + 1);

            if (top >= loggingWidget.Envelope.Height)
                break;

            var paint = entry.LogLevel switch
            {
                LogLevel.Error => _errorTextPaint,
                LogLevel.Warning => _warningTextPaint,
                _ => _informationTextPaint,
            };

            canvas.DrawText(entry.FormattedLogLine, (float)(marginX + paddingX), (float)(marginY + (paddingX * line) + effectiveTextSize * (line + 1)), SKTextAlign.Left, font, paint);

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
        _backgroundPaint.Color = loggingWidget.BackColor.ToSkia().WithAlpha((byte)(255.0f * loggingWidget.Opacity));
        _errorTextPaint.Color = loggingWidget.ErrorTextColor.ToSkia();
        _warningTextPaint.Color = loggingWidget.WarningTextColor.ToSkia();
        _informationTextPaint.Color = loggingWidget.InformationTextColor.ToSkia();
    }
}
