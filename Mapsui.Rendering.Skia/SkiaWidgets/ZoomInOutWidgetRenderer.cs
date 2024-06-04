using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class ZoomInOutWidgetRenderer : ISkiaWidgetRenderer
{
    private const float Stroke = 3;

    private static SKPaint? _paintStroke;
    private static SKPaint? _paintBackground;
    private static SKPaint? _paintText;

    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService,
        float layerOpacity)
    {
        var zoomInOut = (ZoomInOutWidget)widget;

        // If this is the first time, we call this renderer, ...
        if (_paintStroke == null || _paintBackground == null || _paintText == null)
        {
            // ... than create the paints
#pragma warning disable IDISP007 // Don't dispose injected
            _paintStroke?.Dispose();
            _paintStroke = CreatePaint(zoomInOut.StrokeColor.ToSkia(layerOpacity), Stroke, SKPaintStyle.Stroke);
            _paintBackground?.Dispose();
            _paintBackground = CreatePaint(zoomInOut.BackColor.ToSkia(layerOpacity), Stroke, SKPaintStyle.Fill);
            _paintText?.Dispose();
            _paintText = CreatePaint(zoomInOut.TextColor.ToSkia(layerOpacity), Stroke, SKPaintStyle.Fill);
#pragma warning restore IDISP007
        }
        else
        {
            // Update paints with new values
            _paintStroke.Color = zoomInOut.StrokeColor.ToSkia((float)zoomInOut.Opacity);
            _paintBackground.Color = zoomInOut.BackColor.ToSkia((float)zoomInOut.Opacity);
            _paintText.Color = zoomInOut.TextColor.ToSkia((float)zoomInOut.Opacity);
        }

        zoomInOut.UpdateEnvelope(zoomInOut.Orientation == Orientation.Vertical ? zoomInOut.Size : zoomInOut.Size * 2 - Stroke, zoomInOut.Orientation == Orientation.Vertical ? zoomInOut.Size * 2 - Stroke : zoomInOut.Size, viewport.Width, viewport.Height);

        var posX = (float)(zoomInOut.Envelope?.MinX ?? 0.0);
        var posY = (float)(zoomInOut.Envelope?.MinY ?? 0.0);

        // Draw a rect for zoom in button
        SKRect rect;

        rect = new SKRect((float)posX, (float)posY, (float)(posX + zoomInOut.Size), (float)(posY + zoomInOut.Size));
        canvas.DrawRoundRect(rect, 2, 2, _paintBackground);
        canvas.DrawRoundRect(rect, 2, 2, _paintStroke);

        // Draw rect for zoom out button
        if (zoomInOut.Orientation == Orientation.Vertical)
            rect = new SKRect((float)posX, (float)(posY + zoomInOut.Size), (float)(posX + zoomInOut.Size), (float)(posY + zoomInOut.Size * 2 - Stroke));
        else
            rect = new SKRect((float)(posX + zoomInOut.Size), (float)posY, (float)(posX + zoomInOut.Size * 2 - Stroke), (float)(posY + zoomInOut.Size));
        canvas.DrawRoundRect(rect, 2, 2, _paintBackground);
        canvas.DrawRoundRect(rect, 2, 2, _paintStroke);

        // Draw +
        canvas.DrawLine((float)(posX + zoomInOut.Size * 0.3f), (float)(posY + zoomInOut.Size * 0.5f), (float)(posX + zoomInOut.Size * 0.7f), (float)(posY + zoomInOut.Size * 0.5f), _paintText);
        canvas.DrawLine((float)(posX + zoomInOut.Size * 0.5f), (float)(posY + zoomInOut.Size * 0.3f), (float)(posX + zoomInOut.Size * 0.5f), (float)(posY + zoomInOut.Size * 0.7f), _paintText);

        // Draw -
        if (zoomInOut.Orientation == Orientation.Vertical)
            canvas.DrawLine((float)(posX + zoomInOut.Size * 0.3f), (float)(posY - Stroke + zoomInOut.Size * 1.5f), (float)(posX + zoomInOut.Size * 0.7f), (float)(posY - Stroke + zoomInOut.Size * 1.5f), _paintText);
        else
            canvas.DrawLine((float)(posX - Stroke + zoomInOut.Size * 1.3f), (float)(posY + zoomInOut.Size * 0.5f), (float)(posX - Stroke + zoomInOut.Size * 1.7f), (float)(posY + zoomInOut.Size * 0.5f), _paintText);

        // Perhaps we should resize the Envelop about half of stroke, because of Skia rendering have of line outside
        if (zoomInOut.Orientation == Orientation.Vertical)
            zoomInOut.Envelope = new MRect(posX, posY, posX + rect.Width, posY + rect.Width * 2 - Stroke);
        else
            zoomInOut.Envelope = new MRect(posX, posY, posX + rect.Width * 2 - Stroke, posY + rect.Width);
    }

    private static SKPaint CreatePaint(SKColor color, float strokeWidth, SKPaintStyle style)
    {
        var paint = new SKPaint
        {
            LcdRenderText = true,
            Color = color,
            StrokeWidth = strokeWidth,
            Style = style,
            StrokeCap = SKStrokeCap.Square,
            IsAntialias = true
        };

        return paint;
    }
}
