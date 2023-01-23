using System;
using System.Linq;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaWidgets;

public class ScaleBarWidgetRenderer : ISkiaWidgetRenderer, IDisposable
{
    private SKPaint? _paintScaleBar;
    private SKPaint? _paintScaleBarStroke;
    private SKPaint? _paintScaleText;
    private SKPaint? _paintScaleTextStroke;

    public void Draw(SKCanvas canvas, IReadOnlyViewport viewport, IWidget widget,
        float layerOpacity)
    {
        var scaleBar = (ScaleBarWidget)widget;
        if (!scaleBar.CanProject()) return;

        // If this is the first time, we call this renderer, ...
        if (_paintScaleBar == null)
        {
            // ... than create the paints
            _paintScaleBar = CreateScaleBarPaint(SKPaintStyle.Fill);
            _paintScaleBarStroke = CreateScaleBarPaint(SKPaintStyle.Stroke);
            _paintScaleText = CreateTextPaint(SKPaintStyle.Fill);
            _paintScaleTextStroke = CreateTextPaint(SKPaintStyle.Stroke);
        }

        // Update paints with new values
        _paintScaleBar.Color = scaleBar.TextColor.ToSkia(layerOpacity);
        _paintScaleBar.StrokeWidth = scaleBar.StrokeWidth * scaleBar.Scale;
        _paintScaleBarStroke!.Color = scaleBar.Halo.ToSkia(layerOpacity);
        _paintScaleBarStroke.StrokeWidth = scaleBar.StrokeWidthHalo * scaleBar.Scale;
        _paintScaleText!.Color = scaleBar.TextColor.ToSkia(layerOpacity);
        _paintScaleText.StrokeWidth = scaleBar.StrokeWidth * scaleBar.Scale;
        _paintScaleText.Typeface = SKTypeface.FromFamilyName(scaleBar.Font?.FontFamily,
            SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        _paintScaleText.TextSize = (float)(scaleBar.Font?.Size ?? 10) * scaleBar.Scale;
        _paintScaleTextStroke!.Color = scaleBar.Halo.ToSkia(layerOpacity);
        _paintScaleTextStroke.StrokeWidth = scaleBar.StrokeWidthHalo / 2 * scaleBar.Scale;
        _paintScaleTextStroke.Typeface = SKTypeface.FromFamilyName(scaleBar.Font?.FontFamily,
            SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
        _paintScaleTextStroke.TextSize = (float)(scaleBar.Font?.Size ?? 10) * scaleBar.Scale;

        float scaleBarLength1;
        string? scaleBarText1;
        float scaleBarLength2;
        string? scaleBarText2;

        (scaleBarLength1, scaleBarText1, scaleBarLength2, scaleBarText2) = scaleBar.GetScaleBarLengthAndText(viewport);

        // Calc height of scale bar
        var textSize = SKRect.Empty;

        // Do this, because height of text changes sometimes (e.g. from 2 m to 1 m)
        _paintScaleTextStroke.MeasureText("9999 m", ref textSize);

        var scaleBarHeight = textSize.Height + (scaleBar.TickLength + scaleBar.StrokeWidthHalo * 0.5f + scaleBar.TextMargin) * scaleBar.Scale;

        if (scaleBar.ScaleBarMode == ScaleBarMode.Both && scaleBar.SecondaryUnitConverter != null)
        {
            scaleBarHeight *= 2;
        }
        else
        {
            scaleBarHeight += scaleBar.StrokeWidthHalo * 0.5f * scaleBar.Scale;
        }

        scaleBar.Height = scaleBarHeight;

        // Draw lines

        // Get lines for scale bar
        var points = scaleBar.GetScaleBarLinePositions(viewport, scaleBarLength1, scaleBarLength2, scaleBar.StrokeWidthHalo);

        // Draw outline of scale bar
        for (var i = 0; i < points.Count; i += 2)
        {
            canvas.DrawLine((float)points[i].X, (float)points[i].Y, (float)points[i + 1].X, (float)points[i + 1].Y, _paintScaleBarStroke);
        }

        // Draw scale bar
        for (var i = 0; i < points.Count; i += 2)
        {
            canvas.DrawLine((float)points[i].X, (float)points[i].Y, (float)points[i + 1].X, (float)points[i + 1].Y, _paintScaleBar);
        }

        if (!points.Any()) throw new NotImplementedException($"A {nameof(ScaleBarWidget)} can not be drawn without line positions");

        var envelop = new MRect(points.Select(p => p.MRect));
        envelop = envelop.Grow(scaleBar.StrokeWidthHalo * 0.5f * scaleBar.Scale);

        // Draw text

        // Calc text height
        var textSize1 = SKRect.Empty;
        var textSize2 = SKRect.Empty;

        scaleBarText1 ??= string.Empty;
        _paintScaleTextStroke.MeasureText(scaleBarText1, ref textSize1);

        if (scaleBar.ScaleBarMode == ScaleBarMode.Both && scaleBar.SecondaryUnitConverter != null)
        {
            // If there is SecondaryUnitConverter we need to calculate the size before passing it into GetScaleBarTextPositions
            scaleBarText2 ??= string.Empty;
            _paintScaleTextStroke.MeasureText(scaleBarText2, ref textSize2);
        }

        var (posX1, posY1, posX2, posY2) = scaleBar.GetScaleBarTextPositions(viewport, textSize.ToMRect(), textSize1.ToMRect(), textSize2.ToMRect(), scaleBar.StrokeWidthHalo);

        // Now draw text
        canvas.DrawText(scaleBarText1, posX1, posY1 - textSize1.Top, _paintScaleTextStroke);
        canvas.DrawText(scaleBarText1, posX1, posY1 - textSize1.Top, _paintScaleText);

        envelop = envelop?.Join(new MRect(posX1, posY1, posX1 + textSize1.Width, posY1 + textSize1.Height));

        if (scaleBar.ScaleBarMode == ScaleBarMode.Both && scaleBar.SecondaryUnitConverter != null)
        {
            // Now draw second text
            canvas.DrawText(scaleBarText2, posX2, posY2 - textSize2.Top, _paintScaleTextStroke);
            canvas.DrawText(scaleBarText2, posX2, posY2 - textSize2.Top, _paintScaleText);

            envelop = envelop?.Join(new MRect(posX2, posY2, posX2 + textSize2.Width, posY2 + textSize2.Height));
        }

        scaleBar.Envelope = envelop;

        if (scaleBar.ShowEnvelop && envelop != null)
        {
            // Draw a rect around the scale bar for testing
            var tempPaint = _paintScaleTextStroke;
            canvas.DrawRect(new SKRect((float)envelop.MinX, (float)envelop.MinY, (float)envelop.MaxX, (float)envelop.MaxY), tempPaint);
        }
    }

    private static SKPaint CreateScaleBarPaint(SKPaintStyle style)
    {
        return new SKPaint
        {
            LcdRenderText = true,
            Style = style,
            StrokeCap = SKStrokeCap.Square
        };
    }

    private static SKPaint CreateTextPaint(SKPaintStyle style)
    {
        return new SKPaint
        {
            LcdRenderText = true,
            Style = style,
            IsAntialias = true
        };
    }

    public virtual void Dispose()
    {
        _paintScaleBar?.Dispose();
        _paintScaleBarStroke?.Dispose();
        _paintScaleText?.Dispose();
        _paintScaleTextStroke?.Dispose();
    }
}
