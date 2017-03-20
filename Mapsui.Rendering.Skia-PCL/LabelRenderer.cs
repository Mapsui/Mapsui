using System;
using System.Collections.Generic;
using Mapsui.Providers;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class LabelRenderer
    {
        private static readonly IDictionary<string, BitmapInfo> LabelCache =
            new Dictionary<string, BitmapInfo>();

        public static void DrawAsBitmap(SKCanvas canvas, LabelStyle style, IFeature feature, float x, float y)
        {
            var text = style.GetLabelText(feature);

            var key = text + "_" + style.Font.FontFamily + "_" + style.Font.Size + "_" + (float)style.Font.Size + "_" +
                      style.BackColor + "_" + style.ForeColor;

            if (!LabelCache.Keys.Contains(key))
                LabelCache[key] = new BitmapInfo { Bitmap = CreateLabelAsBitmap(style, text) };

            var info = LabelCache[key];

            BitmapHelper.RenderBitmap(canvas, info.Bitmap, (int)Math.Round(x), (int)Math.Round(y),
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

            var horizontalAlign = CalcHorizontalAlignment(style.HorizontalAlignment);
            var verticalAlign = CalcVerticalAlignment(style.VerticalAlignment);
                        
            var backRectXOffset = -rect.Left;
            var backRectYOffset = rect.Bottom;

            rect.Offset(
                x - rect.Width * horizontalAlign + (float)style.Offset.X,
                y + rect.Height * verticalAlign + (float)style.Offset.Y);

            var backRect = rect; // copy
            rect.Offset(-backRectXOffset, -backRectYOffset); // correct for text specific offset returned paint.Measure

            backRect.Inflate(3, 3);
            DrawBackground(style, backRect, target);

            target.DrawText(text, rect.Left, rect.Bottom, paint);
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