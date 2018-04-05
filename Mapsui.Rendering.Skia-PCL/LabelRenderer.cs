using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class LabelRenderer
    {
        private static readonly IDictionary<string, BitmapInfo> LabelCache =
            new Dictionary<string, BitmapInfo>();

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

            BitmapHelper.RenderBitmap(canvas, info.Bitmap, (int)Math.Round(x), (int)Math.Round(y),
                offsetX: (float)offsetX, offsetY: (float)-offsetY,
                horizontalAlignment: style.HorizontalAlignment, verticalAlignment: style.VerticalAlignment);
        }

        public static void Draw(SKCanvas canvas, LabelStyle style, IFeature feature, float x, float y,
            float layerOpacity)
        {
            var text = style.GetLabelText(feature);
            if (string.IsNullOrEmpty(text)) return;
            DrawLabel(canvas, x, y, style, text, layerOpacity);
        }

        private static SKImage CreateLabelAsBitmap(LabelStyle style, string text, float layerOpacity)
        {
            using (var paint = CreatePaint(style, layerOpacity))
            {
                return CreateLabelAsBitmap(style, text, paint, layerOpacity);
            }
        }

        private static SKImage CreateLabelAsBitmap(LabelStyle style, string text, SKPaint paint, float  layerOpacity)
        {
            var rect = new SKRect();
            paint.MeasureText(text, ref rect);

            var backRect = new SKRect(0, 0, rect.Width + 6, rect.Height + 6);

            var skImageInfo = new SKImageInfo((int)backRect.Width, (int)backRect.Height);

            var bitmap = SKImage.Create(skImageInfo);
            
            // todo: Construct SKCanvas with SKImage once this option becomes available
            using (var target = new SKCanvas(SKBitmap.FromImage(bitmap)))
            {
                target.Clear();

                DrawBackground(style, backRect, target, layerOpacity);
                target.DrawText(text, -rect.Left + 3, -rect.Top +3, paint);
                return bitmap;
            }
        }

        private static void DrawLabel(SKCanvas target, float x, float y, LabelStyle style, string text, float layerOpacity)
        {
            var paint = CreatePaint(style, layerOpacity);

            var rect = new SKRect();

            Line[] lines = null;

            float emWidth = 0;
            float emHeight = 0;

            if (style.MaxWidth > 0)
            {
                paint.MeasureText("M", ref rect);
                emWidth = rect.Width;
                emHeight = paint.FontSpacing;
            }

            paint.MeasureText(text, ref rect);

            var baseline = -rect.Top;

            var drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);

            if (style.MaxWidth > 0 && style.WordWrap != LabelStyle.LineBreakMode.NoWrap && drawRect.Width > style.MaxWidth * emWidth)
            {
                float maxWidth = (float)style.MaxWidth * emWidth;

                // Text is to long, so shorten or wrap it
                if (style.WordWrap == LabelStyle.LineBreakMode.WordWrap)
                {
                    lines = SplitLines(text, paint, maxWidth);
                    var width = 0f;
                    for (var i = 0; i < lines.Length; i++)
                    {
                        lines[i].Baseline = baseline + (float)(style.LineHeight * emHeight * i);
                        width = Math.Max(lines[i].Width, width);
                    }

                    drawRect = new SKRect(0, 0, width, (float)(drawRect.Height + style.LineHeight * emHeight * (lines.Length - 1)));
                }

                if (style.WordWrap == LabelStyle.LineBreakMode.HeadTruncation)
                {
                    var result = text.Substring(text.Length - (int) style.MaxWidth - 2);
                    while (result.Length > 1 && paint.MeasureText("..." + result) > maxWidth)
                        result = result.Substring(1);
                    text = "..." + result;
                    paint.MeasureText(text, ref rect);
                    drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
                }

                if (style.WordWrap == LabelStyle.LineBreakMode.TailTruncation)
                {
                    var result = text.Substring(0, (int)style.MaxWidth + 2);
                    while (result.Length > 1 && paint.MeasureText(result + "...") > maxWidth)
                        result = result.Substring(0, result.Length - 1);
                    text = result + "...";
                    paint.MeasureText(text, ref rect);
                    drawRect = new SKRect(0, 0, rect.Right - rect.Left, rect.Bottom - rect.Top);
                }

                if (style.WordWrap == LabelStyle.LineBreakMode.MiddleTruncation)
                {
                    var result1 = text.Substring(0, (int)(style.MaxWidth/2) + 1);
                    var result2 = text.Substring(text.Length - (int)(style.MaxWidth/2) - 1);
                    while (result1.Length > 1 && result2.Length > 1 &&
                           paint.MeasureText(result1 + "..." + result2) > maxWidth)
                    {
                        result1 = result1.Substring(0, result1.Length - 1);
                        result2 = result2.Substring(1);
                    }

                    text = result1 + "..." + result2;
                    paint.MeasureText(text, ref rect);
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

            var backRect = drawRect; // copy

            backRect.Inflate(3, 3);

            DrawBackground(style, backRect, target, layerOpacity);

            if (style.Halo != null)
            {
                var haloPaint = CreatePaint(style, layerOpacity);
                haloPaint.Style = SKPaintStyle.StrokeAndFill;
                haloPaint.Color = style.Halo.Color.ToSkia(layerOpacity);

                // TODO: PenStyle
                /*
                float[] intervals = { 10.0f, 5.0f, 2.0f, 5.0f };
                haloPaint.SetPathEffect(SkDashPathEffect::Make(intervals, count, 0.0f));
                */

                haloPaint.StrokeWidth = (float)style.Halo.Width * 2;

                if (lines != null)
                {
                    var left = drawRect.Left;
                    foreach (var line in lines)
                    {
                        if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Center)
                            target.DrawText(line.Value, (float)(left + (drawRect.Width - line.Width) * 0.5), drawRect.Top + line.Baseline, haloPaint);
                        else if (style.HorizontalAlignment == LabelStyle.HorizontalAlignmentEnum.Right)
                            target.DrawText(line.Value, left + drawRect.Width - line.Width, drawRect.Top + line.Baseline, haloPaint);
                        else
                            target.DrawText(line.Value, left, drawRect.Top + line.Baseline, haloPaint);
                    }
                }
                else
                    target.DrawText(text, drawRect.Left, drawRect.Top + baseline, haloPaint);
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

        private static float CalcHorizontalAlignment(LabelStyle.HorizontalAlignmentEnum horizontalAligment)
        {
            if (horizontalAligment == LabelStyle.HorizontalAlignmentEnum.Center) return 0.5f;
            if (horizontalAligment == LabelStyle.HorizontalAlignmentEnum.Left) return 0f;
            if (horizontalAligment == LabelStyle.HorizontalAlignmentEnum.Right) return 1f;
            throw new ArgumentException(); 
        }

        private static float CalcVerticalAlignment(LabelStyle.VerticalAlignmentEnum verticalAligment)
        {
            if (verticalAligment == LabelStyle.VerticalAlignmentEnum.Center) return 0.5f;
            if (verticalAligment == LabelStyle.VerticalAlignmentEnum.Top) return 0f;
            if (verticalAligment == LabelStyle.VerticalAlignmentEnum.Bottom) return 1f;
            throw new ArgumentException();
        }

        private static void DrawBackground(LabelStyle style, SKRect rect, SKCanvas target, float layerOpacity)
        {
            if (style.BackColor != null)
            {
                var color = style.BackColor?.Color?.ToSkia(layerOpacity);
                if (color.HasValue)
                {
                    var rounding = 6;
                    using (var backgroundPaint = new SKPaint {Color = color.Value})
                    {
                        target.DrawRoundRect(rect, rounding, rounding, backgroundPaint);
                    }
                }
            }
        }

        private static readonly Dictionary<string, SKTypeface> CacheTypeface = new Dictionary<string, SKTypeface>();

        private static SKPaint CreatePaint(LabelStyle style, float layerOpacity)
        {
            if (!CacheTypeface.TryGetValue(style.Font.FontFamily, out SKTypeface typeface))
            {
                typeface = SKTypeface.FromFamilyName(style.Font.FontFamily);
                CacheTypeface[style.Font.FontFamily] = typeface;
            }

            return new SKPaint
            {
                TextSize = (float) style.Font.Size,
                IsAntialias = true,
                Color = style.ForeColor.ToSkia(layerOpacity),
                Typeface = typeface,
                IsStroke = false,
                FakeBoldText = false,
                IsEmbeddedBitmapText = true
            };
        }

        private class Line
        {
            public string Value { get; set; }
            public float Width { get; set; }
            public float Baseline { get; set; }
        }

        private static Line[] SplitLines(string text, SKPaint paint, float maxWidth)
        {
            var spaceWidth = paint.MeasureText(" ");
            var lines = text.Split('\n');

            return lines.SelectMany((line) =>
            {
                var result = new List<Line>();

                var words = line.Split(new[] { " " }, StringSplitOptions.None);

                var lineResult = new StringBuilder();
                float width = 0;
                foreach (var word in words)
                {
                    var wordWidth = paint.MeasureText(word);
                    var wordWithSpaceWidth = wordWidth + spaceWidth;
                    var wordWithSpace = word + " ";

                    if (width + wordWidth > maxWidth)
                    {
                        result.Add(new Line() { Value = lineResult.ToString(), Width = width });
                        lineResult = new StringBuilder(wordWithSpace);
                        width = wordWithSpaceWidth;
                    }
                    else
                    {
                        lineResult.Append(wordWithSpace);
                        width += wordWithSpaceWidth;
                    }
                }

                result.Add(new Line() { Value = lineResult.ToString(), Width = width });

                return result.ToArray();
            }).ToArray();
        }
    }
}