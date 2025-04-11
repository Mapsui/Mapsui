using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.InfoWidgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class PerformanceWidgetRenderer : ISkiaWidgetRenderer
{
    private readonly string[] _textHeader = { "Frames", "Last", "Mean", "Min", "Max", "Count" };
    private readonly string[] _text = new string[6];

    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService, float layerOpacity)
    {
        var performanceWidget = (PerformanceWidget)widget;
        if (!performanceWidget.Performance.GetIsActive())
            return;

        var textSize = performanceWidget.TextSize;

        using var font = new SKFont() { Size = (float)textSize };
        using var textPaint = new SKPaint { Color = performanceWidget.TextColor.ToSkia() };
        var opacity = (performanceWidget.BackColor?.A ?? 255f) * performanceWidget.Opacity;
        using var backgroundPaint = new SKPaint { Color = performanceWidget.BackColor.ToSkia().WithAlpha((byte)opacity), Style = SKPaintStyle.Fill, };

        var widthHeader = 0f;

        for (var i = 0; i < _textHeader.Length; i++)
            widthHeader = System.Math.Max(widthHeader, font.MeasureText(_textHeader[5], textPaint));

        var width = widthHeader + 20 + font.MeasureText("0000.000", textPaint) + 4;
        var height = _textHeader.Length * (performanceWidget.TextSize + 2) - 2 + 4;

        performanceWidget.UpdateEnvelope(width, height, viewport.Width, viewport.Height);

        _text[0] = performanceWidget.Performance.FPS.ToString("0 fps");
        _text[1] = performanceWidget.Performance.LastDrawingTime.ToString("0.000 ms");
        _text[2] = performanceWidget.Performance.Mean.ToString("0.000 ms");
        _text[3] = performanceWidget.Performance.Min.ToString("0.000 ms");
        _text[4] = performanceWidget.Performance.Max.ToString("0.000 ms");
        _text[5] = performanceWidget.Performance.Count.ToString("0");

        var rect = performanceWidget.Envelope?.ToSkia() ?? canvas.DeviceClipBounds;

        canvas.DrawRect(rect, backgroundPaint);

        for (var i = 0; i < _textHeader.Length; i++)
        {
            canvas.DrawText(_textHeader[i], (float)(rect.Left + 2), (float)(rect.Top + 2 * i + textSize * (i + 1)), SKTextAlign.Left, font, textPaint);
            canvas.DrawText(_text[i], (float)(rect.Right - 2 - font.MeasureText(_text[i], textPaint)), (float)(rect.Top + (2 + textSize) * (i + 1)), SKTextAlign.Left, font, textPaint);
        }
    }
}
