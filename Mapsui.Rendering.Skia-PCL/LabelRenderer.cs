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

            paint.MeasureText(text, ref rect);

            var horizontalAlign = CalcHorizontalAlignment(style.HorizontalAlignment);
            var verticalAlign = CalcVerticalAlignment(style.VerticalAlignment);
                        
            var backRectXOffset = rect.Left;
            var backRectYOffset = rect.Bottom;
            var offsetX = style.Offset.IsRelative ? rect.Width * style.Offset.X : style.Offset.X;
            var offsetY = style.Offset.IsRelative ? rect.Height * style.Offset.Y : style.Offset.Y;

            rect.Offset(
                x - rect.Width * horizontalAlign + (float)offsetX,
                y + rect.Height * verticalAlign + (float)offsetY);

            var backRect = rect; // copy
            rect.Offset(-backRectXOffset, -backRectYOffset); // correct for text specific offset returned paint.Measure

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

                target.DrawText(text, rect.Left, rect.Bottom, haloPaint);
            }

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

        private static SKPaint CreatePaint(LabelStyle style, float layerOpacity)
        {
            return new SKPaint
            {
                TextSize = (float) style.Font.Size,
                IsAntialias = true,
                Color = style.ForeColor.ToSkia(layerOpacity),
                Typeface = SKTypeface.FromFamilyName(style.Font.FontFamily),
                IsStroke = false,
                FakeBoldText = false,
                IsEmbeddedBitmapText = true
            };
        }
    }
}