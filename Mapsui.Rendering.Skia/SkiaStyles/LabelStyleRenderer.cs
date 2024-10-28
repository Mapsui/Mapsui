using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts;
using Mapsui.Rendering.Skia.Extensions;
using Mapsui.Rendering.Skia.SkiaStyles;
using Mapsui.Styles;
using NetTopologySuite.Geometries;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Rendering.Skia.Images;

namespace Mapsui.Rendering.Skia;

public class LabelStyleRenderer : ISkiaStyleRenderer, IFeatureSize
{
    public void DrawAsBitmap(SKCanvas canvas, LabelStyle style, IFeature feature, float x, float y, float layerOpacity, VectorCache vectorCache)
    {
        var text = style.GetLabelText(feature);

        using var holder = vectorCache.GetOrCreate((style, text, layerOpacity), CreateLabelAsBitmap);
        var image = holder.Instance;
        var offsetX = style.Offset is RelativeOffset ? image.Width * style.Offset.X : style.Offset.X;
        var offsetY = style.Offset is RelativeOffset ? image.Height * style.Offset.Y : style.Offset.Y;

        if (image is BitmapImage bitmapImage)
            BitmapRenderer.Draw(canvas, bitmapImage.Image, (int)Math.Round(x), (int)Math.Round(y),
                offsetX: (float)offsetX, offsetY: (float)-offsetY,
                horizontalAlignment: style.HorizontalAlignment, verticalAlignment: style.VerticalAlignment);
        else
            throw new InvalidOperationException("Unexpected drawable image type");
    }

    public static SKTypeface CreateTypeFace(Font font)
    {
        return SKTypeface.FromFamilyName(font.FontFamily,
            font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
            SKFontStyleWidth.Normal,
            font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
    }


    public bool Draw(SKCanvas canvas, Viewport viewport, ILayer layer, IFeature feature, IStyle style, RenderService renderService, long iteration)
    {
        try
        {
            var labelStyle = (LabelStyle)style;
            var text = labelStyle.GetLabelText(feature);

            if (string.IsNullOrEmpty(text))
                return false;

            switch (feature)
            {
                case (PointFeature pointFeature):
                    var (pointX, pointY) = viewport.WorldToScreenXY(pointFeature.Point.X, pointFeature.Point.Y);
                    DrawLabel(canvas, (float)pointX, (float)pointY, labelStyle, text, (float)layer.Opacity, renderService);
                    break;
                case (LineString lineStringFeature):
                    if (feature.Extent == null)
                        return false;
                    var (lineStringCenterX, lineStringCenterY) = viewport.WorldToScreenXY(feature.Extent.Centroid.X, feature.Extent.Centroid.Y);
                    DrawLabel(canvas, (float)lineStringCenterX, (float)lineStringCenterY, labelStyle, text, (float)layer.Opacity, renderService);
                    break;
                case (GeometryFeature polygonFeature):
                    if (polygonFeature.Extent is null)
                        return false;
                    var worldCenter = polygonFeature.Extent.Centroid;
                    var (polygonCenterX, polygonCenterY) = viewport.WorldToScreenXY(worldCenter.X, worldCenter.Y);
                    DrawLabel(canvas, (float)polygonCenterX, (float)polygonCenterY, labelStyle, text, (float)layer.Opacity, renderService);
                    break;
            }
        }
        catch (Exception ex)
        {
            Logger.Log(LogLevel.Error, ex.Message, ex);
        }

        return true;
    }

    private static IDrawableImage CreateLabelAsBitmap((LabelStyle Style, string? Text, float LayerOpacity) valueTuple, IRenderService renderService)
    {
        var style = valueTuple.Style;
        var layerOpacity = valueTuple.LayerOpacity;
        var vectorCache = ((RenderService)renderService).VectorCache;
        using var paintHolder = vectorCache.GetOrCreate((style.Font, style.ForeColor, layerOpacity), CreatePaint);
        var paint = paintHolder.Instance;
        return new BitmapImage(CreateLabelAsImage(style, valueTuple.Text, paint, layerOpacity));
    }

    private static SKImage CreateLabelAsImage(LabelStyle style, string? text, SKPaint paint, float layerOpacity)
    {
        var rect = new SKRect();
        paint.MeasureText(text, ref rect);

        var backRect = new SKRect(0, 0, rect.Width + 6, rect.Height + 6);

        var skImageInfo = new SKImageInfo((int)backRect.Width, (int)backRect.Height);

        var image = SKImage.Create(skImageInfo);
        using var bitmap = SKBitmap.FromImage(image);
        // Todo: Construct the SKCanvas from SKImage instead of SKBitmap once this option becomes available.
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear();

        DrawBackground(style, backRect, canvas, layerOpacity);
        canvas.DrawText(text, -rect.Left + 3, -rect.Top + 3, paint);
        return image;
    }

    private void DrawLabel(SKCanvas target, float x, float y, LabelStyle style, string? text, float layerOpacity, IRenderService renderService)
    {
        var vectorCache = ((RenderService)renderService).VectorCache;
        using var paintHolder = vectorCache.GetOrCreate((style.Font, style.ForeColor, layerOpacity), CreatePaint);
        var paint = paintHolder.Instance;

        var rect = new SKRect();

        Line[]? lines = null;

        float emHeight = 0;
        float maxWidth = 0;
        var hasNewline = text?.Contains('\n') ?? false; // There could be a multi line text by newline

        // Get default values for unit em
        if (style.MaxWidth > 0 || hasNewline)
        {
            paint.MeasureText("M", ref rect);
            emHeight = paint.FontSpacing;
            maxWidth = (float)style.MaxWidth * rect.Width;
        }

        paint.MeasureText(text, ref rect);

        var baseline = -rect.Top;  // Distance from top to baseline of text

        var drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);

        if ((style.MaxWidth > 0 && drawRect.Width > maxWidth) || hasNewline)
        {
            // Text has a line feed or should be shorten by character wrap
            if (hasNewline || style.WordWrap == LabelStyle.LineBreakMode.CharacterWrap)
            {
                lines = SplitLines(text, paint, hasNewline ? drawRect.Width : maxWidth, string.Empty);
                var width = 0f;
                for (var i = 0; i < lines.Length; i++)
                {
                    lines[i].Baseline = baseline + (float)(style.LineHeight * emHeight * i);
                    width = Math.Max(lines[i].Width, width);
                }

                drawRect = new SKRect(0, 0, width, (float)(drawRect.Height + style.LineHeight * emHeight * (lines.Length - 1)));
            }

            // Text is to long, so wrap it by words
            if (style.WordWrap == LabelStyle.LineBreakMode.WordWrap)
            {
                lines = SplitLines(text, paint, maxWidth, " ");
                var width = 0f;
                for (var i = 0; i < lines.Length; i++)
                {
                    lines[i].Baseline = baseline + (float)(style.LineHeight * emHeight * i);
                    width = Math.Max(lines[i].Width, width);
                }

                drawRect = new SKRect(0, 0, width, (float)(drawRect.Height + style.LineHeight * emHeight * (lines.Length - 1)));
            }

            // Shorten it at beginning
            if (style.WordWrap == LabelStyle.LineBreakMode.HeadTruncation)
            {
                var result = text?[(text.Length - (int)style.MaxWidth - 2)..];
                while (result?.Length > 1 && paint.MeasureText("..." + result) > maxWidth)
                    result = result[1..];
                text = "..." + result;
                paint.MeasureText(text, ref rect);
                drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }

            // Shorten it at end
            if (style.WordWrap == LabelStyle.LineBreakMode.TailTruncation)
            {
                var result = text?[..((int)style.MaxWidth + 2)];
                while (result?.Length > 1 && paint.MeasureText(result + "...") > maxWidth)
                    result = result[..^1];
                text = result + "...";
                paint.MeasureText(text, ref rect);
                drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }

            // Shorten it in the middle
            if (style.WordWrap == LabelStyle.LineBreakMode.MiddleTruncation)
            {
                var result1 = text?[..((int)(style.MaxWidth / 2) + 1)];
                var result2 = text?[(text.Length - (int)(style.MaxWidth / 2) - 1)..];
                while (result1?.Length > 1 && result2?.Length > 1 &&
                       paint.MeasureText(result1 + "..." + result2) > maxWidth)
                {
                    result1 = result1[..^1];
                    result2 = result2[1..];
                }

                text = result1 + "..." + result2;
                paint.MeasureText(text, ref rect);
                drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
        }

        var horizontalAlign = CalcHorizontalAlignment(style.HorizontalAlignment);
        var verticalAlign = CalcVerticalAlignment(style.VerticalAlignment);

        var offsetX = style.Offset is RelativeOffset ? drawRect.Width * style.Offset.X : style.Offset.X;
        var offsetY = style.Offset is RelativeOffset ? drawRect.Height * style.Offset.Y : style.Offset.Y;

        drawRect.Offset(
            x - drawRect.Width * horizontalAlign + (float)offsetX,
            y - drawRect.Height * verticalAlign + (float)offsetY);

        // If style has a background color, than draw background rectangle
        if (style.BackColor != null)
        {
            var backRect = drawRect;
            backRect.Inflate(3, 3);
            DrawBackground(style, backRect, target, layerOpacity);
        }

        // If style has a halo value, than draw halo text
        if (style.Halo != null)
        {
            using var paintHaloHolder = vectorCache.GetOrCreate((style, style.Halo), CreateHaloPaintHolder);
            using var paintHalo = paintHaloHolder.Instance;
            if (lines != null)
            {
                var left = drawRect.Left;
                foreach (var line in lines)
                {
                    if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Center)
                        target.DrawText(line.Value, (float)(left + (drawRect.Width - line.Width) * 0.5), drawRect.Top + line.Baseline, paintHalo);
                    else if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right)
                        target.DrawText(line.Value, left + drawRect.Width - line.Width, drawRect.Top + line.Baseline, paintHalo);
                    else
                        target.DrawText(line.Value, left, drawRect.Top + line.Baseline, paintHalo);
                }
            }
            else
                target.DrawText(text, drawRect.Left, drawRect.Top + baseline, paintHalo);
        }

        if (lines != null)
        {
            var left = drawRect.Left;
            foreach (var line in lines)
            {
                if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Center)
                    target.DrawText(line.Value, (float)(left + (drawRect.Width - line.Width) * 0.5), drawRect.Top + line.Baseline, paint);
                else if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right)
                    target.DrawText(line.Value, left + drawRect.Width - line.Width, drawRect.Top + line.Baseline, paint);
                else
                    target.DrawText(line.Value, left, drawRect.Top + line.Baseline, paint);
            }
        }
        else
            target.DrawText(text, drawRect.Left, drawRect.Top + baseline, paint);
    }

    private static float CalcHorizontalAlignment(LabelStyle.HorizontalAlignmentEnum horizontalAlignment)
    {
        if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Center) return 0.5f;
        if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Left) return 0f;
        if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right) return 1f;
        throw new ArgumentException($"Unknown {nameof(LabelStyle.HorizontalAlignmentEnum)} type '{nameof(horizontalAlignment)}");
    }

    private static float CalcVerticalAlignment(LabelStyle.VerticalAlignmentEnum verticalAlignment)
    {
        if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Center) return 0.5f;
        if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Top) return 0f;
        if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Bottom) return 1f;
        throw new ArgumentException($"Unknown {nameof(LabelStyle.VerticalAlignmentEnum)} type '{nameof(verticalAlignment)}");
    }

    private static void DrawBackground(LabelStyle style, SKRect rect, SKCanvas target, float layerOpacity)
    {
        var color = style.BackColor?.Color?.ToSkia(layerOpacity);
        if (color.HasValue)
        {
            var rounding = style.CornerRounding;
            using var backgroundPaint = new SKPaint { Color = color.Value, IsAntialias = true };
            target.DrawRoundRect(rect, rounding, rounding, backgroundPaint);
            if (style.BorderThickness > 0 &&
                style.BorderColor != Color.Transparent)
            {
                using SKPaint borderPaint = new SKPaint
                {
                    Color = style.BorderColor.ToSkia(),
                    Style = SKPaintStyle.Stroke,
                    StrokeWidth = (float)style.BorderThickness,
                    IsAntialias = true
                };
                target.DrawRoundRect(rect, rounding, rounding, borderPaint);
            }
        }
    }

    private static SKPaint CreatePaint((Font Font, Color ForeColor, float LayerOpacity) style, IRenderService renderService)
    {
        var vectorCache = ((RenderService)renderService).VectorCache;
        using var typeface = vectorCache.GetOrCreate(style.Font, CreateTypeFace);

        SKPaint paint = new()
        {
            IsAntialias = true,
            IsStroke = false,
            FakeBoldText = false,
            IsEmbeddedBitmapText = true,
            Style = SKPaintStyle.Fill,
            TextSize = (float)style.Font.Size,
            Color = style.ForeColor.ToSkia(style.LayerOpacity),
            Typeface = typeface.Instance
        };

        return paint;
    }

    private static SKPaint CreatePaint((Font Font, Color ForeColor, float LayerOpacity, SKPaintStyle PaintStyle, float StrokeWidth) style, IRenderService renderService)
    {
        var paint = CreatePaint((style.Font, style.ForeColor, style.LayerOpacity), renderService);
        paint.Style = style.PaintStyle;
        paint.StrokeWidth = style.StrokeWidth;
        return paint;
    }

    private class Line
    {
        public string? Value { get; set; }
        public float Width { get; set; }
        public float Baseline { get; set; }
    }

    private static Line[] SplitLines(string? text, SKPaint paint, float maxWidth, string splitCharacter)
    {
        if (text == null)
            return [];

        var spaceWidth = paint.MeasureText(" ");
        var lines = text.Split('\n');

        return lines.SelectMany(line =>
        {
            var result = new List<Line>();
            string[] words;

            if (splitCharacter == string.Empty)
            {
                words = line.ToCharArray().Select(x => x.ToString()).ToArray();
                spaceWidth = 0;
            }
            else
            {
                words = line.Split(new[] { splitCharacter }, StringSplitOptions.None);
            }

            var lineResult = new StringBuilder();
            float width = 0;
            foreach (var word in words)
            {
                var wordWidth = paint.MeasureText(word);
                var wordWithSpaceWidth = wordWidth + spaceWidth;
                var wordWithSpace = word + splitCharacter;

                if (width + wordWidth > maxWidth)
                {
                    result.Add(new Line { Value = lineResult.ToString(), Width = width });
                    lineResult = new StringBuilder(wordWithSpace);
                    width = wordWithSpaceWidth;
                }
                else
                {
                    lineResult.Append(wordWithSpace);
                    width += wordWithSpaceWidth;
                }
            }

            result.Add(new Line { Value = lineResult.ToString(), Width = width });

            return result.ToArray();
        }).ToArray();
    }

    bool IFeatureSize.NeedsFeature => true;

    double IFeatureSize.FeatureSize(IStyle style, IRenderService renderingService, IFeature? feature)
    {
        var skiaRenderService = (RenderService)renderingService;
        if (feature == null) throw new ArgumentNullException(nameof(feature));

        if (style is LabelStyle labelStyle)
        {
            return FeatureSize(feature, labelStyle, skiaRenderService);
        }

        return 0;
    }

    public static double FeatureSize(IFeature feature, LabelStyle labelStyle, IRenderService renderService)
    {
        VectorCache vectorCache = ((RenderService)renderService).VectorCache;
        var text = labelStyle.GetLabelText(feature);

        if (string.IsNullOrEmpty(text))
            return 0;

        // for measuring the text size the opacity can be set to 1try
        using var paintHolder = labelStyle.Halo != null ? vectorCache.GetOrCreate((labelStyle, labelStyle.Halo), CreateHaloPaintHolder) : vectorCache.GetOrCreate((labelStyle.Font, labelStyle.ForeColor, 1f), CreatePaint);
        var paint = paintHolder.Instance;
        var rect = new SKRect();
        paint.MeasureText(text, ref rect);

        double size = Math.Max(rect.Width, rect.Height);

        var drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);

        var offset = labelStyle.Offset.CalcOffset(drawRect.Width, drawRect.Height);

        // Pythagoras for maximal distance
        var length = Math.Sqrt(offset.X * offset.X + offset.Y * offset.Y);

        // add offset to size multiplied by two because the total size increased by the offset
        size += (length * 2);

        return size;
    }

    private static SKPaint CreateHaloPaintHolder((LabelStyle labelStyle, Pen halo) style, IRenderService renderService)
    {
        LabelStyle labelStyle = style.labelStyle;
        Pen halo = style.halo;
        var strokeWidth = (float)halo!.Width * 2;
        var paintStyle = SKPaintStyle.StrokeAndFill;
        return CreatePaint((labelStyle.Font, halo.Color, 1, paintStyle, strokeWidth), renderService);
    }
}
