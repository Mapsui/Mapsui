using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class ZoomInOutWidgetRenderer : ISkiaWidgetRenderer
{
    private const float _stroke = 3;

    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService,
        float layerOpacity)
    {
        var zoomInOut = (ZoomInOutWidget)widget;

        using var paintStroke = CreatePaint(zoomInOut.StrokeColor.ToSkia(layerOpacity), _stroke, SKPaintStyle.Stroke);
        using var paintBackground = CreatePaint(zoomInOut.BackColor.ToSkia(layerOpacity), _stroke, SKPaintStyle.Fill);
        using var paintText = CreatePaint(zoomInOut.TextColor.ToSkia(layerOpacity), _stroke, SKPaintStyle.Fill);

        paintStroke.Color = zoomInOut.StrokeColor.ToSkia((float)zoomInOut.Opacity);
        paintBackground.Color = zoomInOut.BackColor.ToSkia((float)zoomInOut.Opacity);
        paintText.Color = zoomInOut.TextColor.ToSkia((float)zoomInOut.Opacity);

        zoomInOut.UpdateEnvelope(zoomInOut.Orientation == Orientation.Vertical ? zoomInOut.Size : zoomInOut.Size * 2 - _stroke, zoomInOut.Orientation == Orientation.Vertical ? zoomInOut.Size * 2 - _stroke : zoomInOut.Size, viewport.Width, viewport.Height);

        var posX = (float)(zoomInOut.Envelope?.MinX ?? 0.0);
        var posY = (float)(zoomInOut.Envelope?.MinY ?? 0.0);

        // Draw a rect for zoom in button
        SKRect rect;

        rect = new SKRect((float)posX, (float)posY, (float)(posX + zoomInOut.Size), (float)(posY + zoomInOut.Size));
        canvas.DrawRoundRect(rect, 2, 2, paintBackground);
        canvas.DrawRoundRect(rect, 2, 2, paintStroke);

        // Draw rect for zoom out button
        if (zoomInOut.Orientation == Orientation.Vertical)
            rect = new SKRect((float)posX, (float)(posY + zoomInOut.Size), (float)(posX + zoomInOut.Size), (float)(posY + zoomInOut.Size * 2 - _stroke));
        else
            rect = new SKRect((float)(posX + zoomInOut.Size), (float)posY, (float)(posX + zoomInOut.Size * 2 - _stroke), (float)(posY + zoomInOut.Size));
        canvas.DrawRoundRect(rect, 2, 2, paintBackground);
        canvas.DrawRoundRect(rect, 2, 2, paintStroke);

        // Draw +
        canvas.DrawLine((float)(posX + zoomInOut.Size * 0.3f), (float)(posY + zoomInOut.Size * 0.5f), (float)(posX + zoomInOut.Size * 0.7f), (float)(posY + zoomInOut.Size * 0.5f), paintText);
        canvas.DrawLine((float)(posX + zoomInOut.Size * 0.5f), (float)(posY + zoomInOut.Size * 0.3f), (float)(posX + zoomInOut.Size * 0.5f), (float)(posY + zoomInOut.Size * 0.7f), paintText);

        // Draw -
        if (zoomInOut.Orientation == Orientation.Vertical)
            canvas.DrawLine((float)(posX + zoomInOut.Size * 0.3f), (float)(posY - _stroke + zoomInOut.Size * 1.5f), (float)(posX + zoomInOut.Size * 0.7f), (float)(posY - _stroke + zoomInOut.Size * 1.5f), paintText);
        else
            canvas.DrawLine((float)(posX - _stroke + zoomInOut.Size * 1.3f), (float)(posY + zoomInOut.Size * 0.5f), (float)(posX - _stroke + zoomInOut.Size * 1.7f), (float)(posY + zoomInOut.Size * 0.5f), paintText);

        // Perhaps we should resize the Envelop about half of stroke, because of Skia rendering have of line outside
        if (zoomInOut.Orientation == Orientation.Vertical)
            zoomInOut.Envelope = new MRect(posX, posY, posX + rect.Width, posY + rect.Width * 2 - _stroke);
        else
            zoomInOut.Envelope = new MRect(posX, posY, posX + rect.Width * 2 - _stroke, posY + rect.Width);
    }

    private static SKPaint CreatePaint(SKColor color, float strokeWidth, SKPaintStyle style)
    {
        var paint = new SKPaint
        {
            Color = color,
            StrokeWidth = strokeWidth,
            Style = style,
            StrokeCap = SKStrokeCap.Square,
            IsAntialias = true
        };

        return paint;
    }
}
