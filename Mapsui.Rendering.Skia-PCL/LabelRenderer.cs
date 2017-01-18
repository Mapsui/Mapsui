using System;
using System.Collections.Generic;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class LabelRenderer
    {
        private static readonly IDictionary<string, SKBitmapInfo> LabelBitmapCache =
            new Dictionary<string, SKBitmapInfo>();

        public static void DrawAsBitmap(SKCanvas canvas, LabelStyle style, IFeature feature, float x, float y)
        {
            var text = style.GetLabelText(feature);

            var key = text + "_" + style.Font.FontFamily + "_" + style.Font.Size + "_" + (float)style.Font.Size + "_" +
                      style.BackColor + "_" + style.ForeColor;

            if (!LabelBitmapCache.Keys.Contains(key))
                LabelBitmapCache[key] = new SKBitmapInfo { Bitmap = CreateLabelAsBitmap(style, text) };

            var info = LabelBitmapCache[key];

            BitmapHelper.RenderTexture(canvas, info.Bitmap, (int)Math.Round(x), (int)Math.Round(y),
                offsetX: (float)style.Offset.X, offsetY: (float)-style.Offset.Y,
                horizontalAlignment: style.HorizontalAlignment, verticalAlignment: style.VerticalAlignment);
        }

        public static void Draw(SKCanvas canvas, LabelStyle style, IFeature feature, float x, float y)
        {
            var text = style.GetLabelText(feature);
            DrawLabel(canvas, x, y, style, text);
        }

        private static SKBitmap CreateLabelAsBitmap(LabelStyle style, string text)
        {
            using (var paint = CreatePaint(style))
            {
                return CreateLabelAsBitmap(style, text, paint);
            }
        }

        private static SKBitmap CreateLabelAsBitmap(LabelStyle style, string text, SKPaint paint)
        {
            var rect = new SKRect();
            paint.MeasureText(text, ref rect);

            var backRect = new SKRect(0, 0, rect.Width + 6, rect.Height + 6);

            var bitmap = new SKBitmap((int)backRect.Width, (int)backRect.Height);

            using (var target = new SKCanvas(bitmap))
            {
                target.Clear();

                DrawBackground(style, backRect, target);
                target.DrawText(text, -rect.Left + 3, -rect.Top +3, paint);
                return bitmap;
            }
        }

        private static void DrawLabel(SKCanvas target, float x, float y, LabelStyle style, string text)
        {
            var paint = CreatePaint(style);

            var rect = new SKRect();

            paint.MeasureText(text, ref rect);

            var align = CalcAlignFactor(style.VerticalAlignment);

            rect.Offset(
                x - rect.Width*0.5f + (float) style.Offset.X,
                y + rect.Height*0.5f + (float) style.Offset.Y);

            var backRect = rect;
            backRect.Inflate(3, 3);
            DrawBackground(style, backRect, target);

            target.DrawText(text, rect.Left, rect.Bottom, paint);
        }

        private static object CalcAlignFactor(LabelStyle.VerticalAlignmentEnum verticalAlignment)
        {
            return (verticalAlignment == LabelStyle.VerticalAlignmentEnum.Center) ? 0.5f : 0f; 
        }

        private static void DrawBackground(LabelStyle style, SKRect rect, SKCanvas target)
        {
            if (style.BackColor != null)
            {
                var color = style.BackColor?.Color?.ToSkia();
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

        private static SKPaint CreatePaint(LabelStyle style)
        {
            return new SKPaint
            {
                TextSize = (float) style.Font.Size,
                IsAntialias = true,
                Color = style.ForeColor.ToSkia(),
                Typeface = SKTypeface.FromFamilyName(style.Font.FontFamily),
                IsStroke = false,
                FakeBoldText = false,
                IsEmbeddedBitmapText = true
            };
        }
    }
}