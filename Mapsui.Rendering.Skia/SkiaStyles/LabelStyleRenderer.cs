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

namespace Mapsui.Rendering.Skia
{
    public class LabelStyleRenderer : ISkiaStyleRenderer
    {
        private static readonly IDictionary<string, BitmapInfo> LabelCache =
            new Dictionary<string, BitmapInfo>();

        private static readonly SKPaint Paint = new()
        {
            IsAntialias = true,
            IsStroke = false,
            FakeBoldText = false,
            IsEmbeddedBitmapText = true
        };

        private static readonly Dictionary<string, SKTypeface> CacheTypeface = new();

        public static void DrawAsBitmap(SKCanvas canvas, LabelStyle style, IFeature feature, float x, float y, float layerOpacity)
        {
            var text = style.GetLabelText(feature);

            var key = text + "_" + style.Font.FontFamily + "_" + style.Font.Size + "_" + (float)style.Font.Size + "_" +
                      style.BackColor + "_" + style.ForeColor;

            if (!LabelCache.Keys.Contains(key))
                LabelCache[key] = new BitmapInfo { Bitmap = CreateLabelAsBitmap(style, text, layerOpacity) };

            var info = LabelCache[key];
            var offsetX = style.Offset.IsRelative ? info.Width * style.Offset.X : style.Offset.X;
            var offsetY = style.Offset.IsRelative ? info.Height * style.Offset.Y : style.Offset.Y;

            BitmapRenderer.Draw(canvas, info.Bitmap, (int)Math.Round(x), (int)Math.Round(y),
                offsetX: (float)offsetX, offsetY: (float)-offsetY,
                horizontalAlignment: style.HorizontalAlignment, verticalAlignment: style.VerticalAlignment);
        }


        public bool Draw(SKCanvas canvas, IReadOnlyViewport viewport, ILayer layer, IFeature feature, IStyle style, ISymbolCache symbolCache, long iteration)
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
                        DrawLabel(canvas, (float)pointX, (float)pointY, labelStyle, text, (float)layer.Opacity);
                        break;
                    case (LineString lineStringFeature):
                        if (feature.Extent == null)
                            return false;
                        var (lineStringCenterX, lineStringCenterY) = viewport.WorldToScreenXY(feature.Extent.Centroid.X, feature.Extent.Centroid.Y);
                        DrawLabel(canvas, (float)lineStringCenterX, (float)lineStringCenterY, labelStyle, text, (float)layer.Opacity);
                        break;
                    case (GeometryFeature polygonFeature):
                        if (polygonFeature.Extent is null)
                            return false;
                        var worldCenter = polygonFeature.Extent.Centroid;
                        var (polygonCenterX, polygonCenterY) = viewport.WorldToScreenXY(worldCenter.X, worldCenter.Y);
                        DrawLabel(canvas, (float)polygonCenterX, (float)polygonCenterY, labelStyle, text, (float)layer.Opacity);
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogLevel.Error, ex.Message, ex);
            }

            return true;
        }

        private static SKImage CreateLabelAsBitmap(LabelStyle style, string? text, float layerOpacity)
        {
            UpdatePaint(style, layerOpacity);

            return CreateLabelAsBitmap(style, text, Paint, layerOpacity);
        }

        private static SKImage CreateLabelAsBitmap(LabelStyle style, string? text, SKPaint paint, float layerOpacity)
        {
            var rect = new SKRect();
            paint.MeasureText(text, ref rect);

            var backRect = new SKRect(0, 0, rect.Width + 6, rect.Height + 6);

            var skImageInfo = new SKImageInfo((int)backRect.Width, (int)backRect.Height);

            var bitmap = SKImage.Create(skImageInfo);

            // todo: Construct SKCanvas with SKImage once this option becomes available
            using var target = new SKCanvas(SKBitmap.FromImage(bitmap));
            target.Clear();

            DrawBackground(style, backRect, target, layerOpacity);
            target.DrawText(text, -rect.Left + 3, -rect.Top + 3, paint);
            return bitmap;
        }

        private static void DrawLabel(SKCanvas target, float x, float y, LabelStyle style, string? text, float layerOpacity)
        {
            UpdatePaint(style, layerOpacity);

            var rect = new SKRect();

            Line[]? lines = null;

            float emHeight = 0;
            float maxWidth = 0;
            var hasNewline = text?.Contains("\n") ?? false; // There could be a multi line text by newline

            // Get default values for unit em
            if (style.MaxWidth > 0 || hasNewline)
            {
                Paint.MeasureText("M", ref rect);
                emHeight = Paint.FontSpacing;
                maxWidth = (float)style.MaxWidth * rect.Width;
            }

            Paint.MeasureText(text, ref rect);

            var baseline = -rect.Top;  // Distance from top to baseline of text

            var drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);

            if ((style.MaxWidth > 0 && drawRect.Width > maxWidth) || hasNewline)
            {
                // Text has a line feed or should be shorten by character wrap
                if (hasNewline || style.WordWrap == LabelStyle.LineBreakMode.CharacterWrap)
                {
                    lines = SplitLines(text, Paint, hasNewline ? drawRect.Width : maxWidth, string.Empty);
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
                    lines = SplitLines(text, Paint, maxWidth, " ");
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
                    var result = text?.Substring(text.Length - (int)style.MaxWidth - 2);
                    while (result?.Length > 1 && Paint.MeasureText("..." + result) > maxWidth)
                        result = result.Substring(1);
                    text = "..." + result;
                    Paint.MeasureText(text, ref rect);
                    drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
                }

                // Shorten it at end
                if (style.WordWrap == LabelStyle.LineBreakMode.TailTruncation)
                {
                    var result = text?.Substring(0, (int)style.MaxWidth + 2);
                    while (result?.Length > 1 && Paint.MeasureText(result + "...") > maxWidth)
                        result = result.Substring(0, result.Length - 1);
                    text = result + "...";
                    Paint.MeasureText(text, ref rect);
                    drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
                }

                // Shorten it in the middle
                if (style.WordWrap == LabelStyle.LineBreakMode.MiddleTruncation)
                {
                    var result1 = text?.Substring(0, (int)(style.MaxWidth / 2) + 1);
                    var result2 = text?.Substring(text.Length - (int)(style.MaxWidth / 2) - 1);
                    while (result1?.Length > 1 && result2?.Length > 1 &&
                           Paint.MeasureText(result1 + "..." + result2) > maxWidth)
                    {
                        result1 = result1.Substring(0, result1.Length - 1);
                        result2 = result2.Substring(1);
                    }

                    text = result1 + "..." + result2;
                    Paint.MeasureText(text, ref rect);
                    drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
                }
            }

            var horizontalAlign = CalcHorizontalAlignment(style.HorizontalAlignment);
            var verticalAlign = CalcVerticalAlignment(style.VerticalAlignment);

            var offsetX = style.Offset.IsRelative ? drawRect.Width * style.Offset.X : style.Offset.X;
            var offsetY = style.Offset.IsRelative ? drawRect.Height * style.Offset.Y : style.Offset.Y;

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
                UpdatePaint(style, layerOpacity);
                Paint.Style = SKPaintStyle.StrokeAndFill;
                Paint.Color = style.Halo.Color.ToSkia(layerOpacity);
                Paint.StrokeWidth = (float)style.Halo.Width * 2;

                if (lines != null)
                {
                    var left = drawRect.Left;
                    foreach (var line in lines)
                    {
                        if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Center)
                            target.DrawText(line.Value, (float)(left + (drawRect.Width - line.Width) * 0.5), drawRect.Top + line.Baseline, Paint);
                        else if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right)
                            target.DrawText(line.Value, left + drawRect.Width - line.Width, drawRect.Top + line.Baseline, Paint);
                        else
                            target.DrawText(line.Value, left, drawRect.Top + line.Baseline, Paint);
                    }
                }
                else
                    target.DrawText(text, drawRect.Left, drawRect.Top + baseline, Paint);
            }

            UpdatePaint(style, layerOpacity);

            if (lines != null)
            {
                var left = drawRect.Left;
                foreach (var line in lines)
                {
                    if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Center)
                        target.DrawText(line.Value, (float)(left + (drawRect.Width - line.Width) * 0.5), drawRect.Top + line.Baseline, Paint);
                    else if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right)
                        target.DrawText(line.Value, left + drawRect.Width - line.Width, drawRect.Top + line.Baseline, Paint);
                    else
                        target.DrawText(line.Value, left, drawRect.Top + line.Baseline, Paint);
                }
            }
            else
                target.DrawText(text, drawRect.Left, drawRect.Top + baseline, Paint);
        }

        private static float CalcHorizontalAlignment(LabelStyle.HorizontalAlignmentEnum horizontalAlignment)
        {
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Center) return 0.5f;
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Left) return 0f;
            if (horizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right) return 1f;
            throw new ArgumentException();
        }

        private static float CalcVerticalAlignment(LabelStyle.VerticalAlignmentEnum verticalAlignment)
        {
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Center) return 0.5f;
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Top) return 0f;
            if (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Bottom) return 1f;
            throw new ArgumentException();
        }

        private static void DrawBackground(LabelStyle style, SKRect rect, SKCanvas target, float layerOpacity)
        {
            var color = style.BackColor?.Color?.ToSkia(layerOpacity);
            if (color.HasValue)
            {
                var rounding = style.CornerRounding;
                using var backgroundPaint = new SKPaint { Color = color.Value };
                target.DrawRoundRect(rect, rounding, rounding, backgroundPaint);
                if (style.BorderThickness > 0 &&
                    style.BorderColor != Color.Transparent)
                {
                    using SKPaint borderPaint = new SKPaint
                    {
                        Color = style.BorderColor.ToSkia(),
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = (float)style.BorderThickness
                    };
                    target.DrawRoundRect(rect, rounding, rounding, borderPaint);
                }
            }
        }

        private static void UpdatePaint(LabelStyle style, float layerOpacity)
        {
            if (!CacheTypeface.TryGetValue(style.Font.ToString(), out var typeface))
            {
                typeface = SKTypeface.FromFamilyName(style.Font.FontFamily,
                    style.Font.Bold ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal,
                    SKFontStyleWidth.Normal,
                    style.Font.Italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright);
                CacheTypeface[style.Font.ToString()] = typeface;
            }

            Paint.Style = SKPaintStyle.Fill;
            Paint.TextSize = (float)style.Font.Size;
            Paint.Color = style.ForeColor.ToSkia(layerOpacity);
            Paint.Typeface = typeface;
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
                return Array.Empty<Line>();

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
    }
}