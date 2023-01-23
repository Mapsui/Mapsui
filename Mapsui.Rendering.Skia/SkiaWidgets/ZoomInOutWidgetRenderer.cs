using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.Zoom;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class ZoomInOutWidgetRenderer : ISkiaWidgetRenderer
{
    private const float Stroke = 3;

    private static SKPaint? _paintStroke;
    private static SKPaint? _paintBackground;
    private static SKPaint? _paintText;

    public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget,
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
            _paintStroke.Color = zoomInOut.StrokeColor.ToSkia(zoomInOut.Opacity);
            _paintBackground.Color = zoomInOut.BackColor.ToSkia(zoomInOut.Opacity);
            _paintText.Color = zoomInOut.TextColor.ToSkia(zoomInOut.Opacity);
        }

        var posX = zoomInOut.CalculatePositionX(0, (float)viewport.Width, zoomInOut.Orientation == Orientation.Vertical ? zoomInOut.Size : zoomInOut.Size * 2 - Stroke);
        var posY = zoomInOut.CalculatePositionY(0, (float)viewport.Height, zoomInOut.Orientation == Orientation.Vertical ? zoomInOut.Size * 2 - Stroke : zoomInOut.Size);

        // Draw a rect for zoom in button
        SKRect rect;

        rect = new SKRect(posX, posY, posX + zoomInOut.Size, posY + zoomInOut.Size);
        canvas.DrawRoundRect(rect, 2, 2, _paintBackground);
        canvas.DrawRoundRect(rect, 2, 2, _paintStroke);

        // Draw rect for zoom out button
        if (zoomInOut.Orientation == Orientation.Vertical)
            rect = new SKRect(posX, posY + zoomInOut.Size, posX + zoomInOut.Size, posY + zoomInOut.Size * 2 - Stroke);
        else
            rect = new SKRect(posX + zoomInOut.Size, posY, posX + zoomInOut.Size * 2 - Stroke, posY + zoomInOut.Size);
        canvas.DrawRoundRect(rect, 2, 2, _paintBackground);
        canvas.DrawRoundRect(rect, 2, 2, _paintStroke);

        // Draw +
        canvas.DrawLine(posX + zoomInOut.Size * 0.3f, posY + zoomInOut.Size * 0.5f, posX + zoomInOut.Size * 0.7f, posY + zoomInOut.Size * 0.5f, _paintText);
        canvas.DrawLine(posX + zoomInOut.Size * 0.5f, posY + zoomInOut.Size * 0.3f, posX + zoomInOut.Size * 0.5f, posY + zoomInOut.Size * 0.7f, _paintText);

        // Draw -
        if (zoomInOut.Orientation == Orientation.Vertical)
            canvas.DrawLine(posX + zoomInOut.Size * 0.3f, posY - Stroke + zoomInOut.Size * 1.5f, posX + zoomInOut.Size * 0.7f, posY - Stroke + zoomInOut.Size * 1.5f, _paintText);
        else
            canvas.DrawLine(posX - Stroke + zoomInOut.Size * 1.3f, posY + zoomInOut.Size * 0.5f, posX - Stroke + zoomInOut.Size * 1.7f, posY + zoomInOut.Size * 0.5f, _paintText);

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
