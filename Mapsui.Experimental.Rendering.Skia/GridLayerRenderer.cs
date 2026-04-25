using Mapsui.Extensions;
using Mapsui.Experimental.Rendering.Skia.Extensions;
using Mapsui.Layers;
using Mapsui.Rendering;
using SkiaSharp;
using System;
using System.Globalization;

namespace Mapsui.Experimental.Rendering.Skia;

internal static class GridLayerRenderer
{
    public static void Render(SKCanvas canvas, Viewport viewport, ILayer layer, RenderService _renderService)
    {
        var gridLayer = (GridLayer)layer;
        var opacity = (float)layer.Opacity;

        var worldSpanX = viewport.Width * viewport.Resolution;
        var worldSpanY = viewport.Height * viewport.Resolution;
        var step = CalcNiceStep(Math.Max(worldSpanX, worldSpanY), gridLayer.TargetLineCount);

        var extent = viewport.ToExtent();
        if (extent == null) return;

        using var linePaint = new SKPaint
        {
            Color = gridLayer.LineColor.ToSkia(opacity),
            StrokeWidth = gridLayer.LineWidth,
            Style = SKPaintStyle.Stroke,
            IsAntialias = false, // crisp axis-aligned lines
        };

        DrawVerticalLines(canvas, viewport, extent, step, linePaint);
        DrawHorizontalLines(canvas, viewport, extent, step, linePaint);

        if (gridLayer.ShowCoordinateLabels)
            DrawLabels(canvas, viewport, extent, step, gridLayer, opacity);
    }

    private static void DrawVerticalLines(SKCanvas canvas, Viewport viewport, MRect extent, double step, SKPaint paint)
    {
        var firstX = Math.Ceiling(extent.MinX / step) * step;
        for (var worldX = firstX; worldX <= extent.MaxX + step * 0.5; worldX += step)
        {
            var (x1, y1) = viewport.WorldToScreenXY(worldX, extent.MaxY);
            var (x2, y2) = viewport.WorldToScreenXY(worldX, extent.MinY);
            canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2, paint);
        }
    }

    private static void DrawHorizontalLines(SKCanvas canvas, Viewport viewport, MRect extent, double step, SKPaint paint)
    {
        var firstY = Math.Ceiling(extent.MinY / step) * step;
        for (var worldY = firstY; worldY <= extent.MaxY + step * 0.5; worldY += step)
        {
            var (x1, y1) = viewport.WorldToScreenXY(extent.MinX, worldY);
            var (x2, y2) = viewport.WorldToScreenXY(extent.MaxX, worldY);
            canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2, paint);
        }
    }

    private static void DrawLabels(SKCanvas canvas, Viewport viewport, MRect extent, double step, GridLayer gridLayer, float opacity)
    {
        var decimals = (int)Math.Max(0, Math.Ceiling(-Math.Log10(step)));
        var format = $"F{decimals}";
        var labelMargin = gridLayer.LabelSize * 0.5f;

        using var font = new SKFont { Size = gridLayer.LabelSize };
        using var textPaint = new SKPaint { Color = gridLayer.LabelColor.ToSkia(opacity), IsAntialias = true };

        // X labels near the bottom edge, at each vertical grid line
        var firstX = Math.Ceiling(extent.MinX / step) * step;
        for (var worldX = firstX; worldX <= extent.MaxX + step * 0.5; worldX += step)
        {
            var (screenX, _) = viewport.WorldToScreenXY(worldX, extent.MinY);
            if (screenX < 0 || screenX > viewport.Width) continue;
            var label = worldX.ToString(format, CultureInfo.InvariantCulture);
            canvas.DrawText(label, (float)screenX + labelMargin, (float)viewport.Height - labelMargin, SKTextAlign.Left, font, textPaint);
        }

        // Y labels near the left edge, at each horizontal grid line
        var firstY = Math.Ceiling(extent.MinY / step) * step;
        for (var worldY = firstY; worldY <= extent.MaxY + step * 0.5; worldY += step)
        {
            var (_, screenY) = viewport.WorldToScreenXY(extent.MinX, worldY);
            if (screenY < labelMargin || screenY > viewport.Height) continue;
            var label = worldY.ToString(format, CultureInfo.InvariantCulture);
            canvas.DrawText(label, labelMargin, (float)screenY - labelMargin, SKTextAlign.Left, font, textPaint);
        }
    }

    // Rounds worldSpan / targetLineCount to the nearest 1, 2 or 5 multiple of a power of ten,
    // producing "nice" grid intervals that stay fixed while panning and change in discrete steps while zooming.
    private static double CalcNiceStep(double worldSpan, int targetLineCount)
    {
        if (targetLineCount <= 0) return worldSpan > 0 ? worldSpan : 1;
        var rawStep = worldSpan / targetLineCount;
        if (rawStep <= 0) return 1;

        var magnitude = Math.Pow(10, Math.Floor(Math.Log10(rawStep)));
        var fraction = rawStep / magnitude;

        double niceFraction;
        if (fraction < 1.5) niceFraction = 1;
        else if (fraction < 3.5) niceFraction = 2;
        else if (fraction < 7.5) niceFraction = 5;
        else niceFraction = 10;

        return niceFraction * magnitude;
    }
}
