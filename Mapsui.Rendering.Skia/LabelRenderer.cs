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

        public static void Draw(SKCanvas canvas, LabelStyle style, IFeature feature, float x, float y)
        {
            var text = style.GetLabelText(feature);

            var key = text + "_" + style.Font.FontFamily + "_" + style.Font.Size + "_" + (float) style.Font.Size + "_" +
                      style.BackColor + "_" + style.ForeColor;

            if (!LabelBitmapCache.Keys.Contains(key))
                LabelBitmapCache[key] = new SKBitmapInfo {Bitmap = CreateLabelAsBitmap(style, text)};

            var info = LabelBitmapCache[key];

            BitmapHelper.RenderTexture(canvas, info.Bitmap, x, y,
                offsetX: (float) style.Offset.X, offsetY: (float) -style.Offset.Y,
                horizontalAlignment: style.HorizontalAlignment, verticalAlignment: style.VerticalAlignment);
        }

        static SKBitmap CreateLabelAsBitmap(LabelStyle style, string text)
        {
            using (var paint = new SKPaint())
            {
                paint.TextSize = (float)style.Font.Size;
                paint.IsAntialias = true;
                paint.Color = style.ForeColor.ToSkia();
                paint.Typeface = SKTypeface.FromFamilyName(style.Font.FontFamily);
                paint.IsStroke = false;
                paint.FakeBoldText = false;
                paint.IsEmbeddedBitmapText = true;

                var rect = new SKRect();
                paint.MeasureText(text, ref rect);

                var padding = 3; // todo get this from LabelStyle
                rect = SKRect.Inflate(rect, padding, padding);

                var bitmap = new SKBitmap((int)Math.Ceiling(rect.Width), (int)Math.Ceiling(rect.Height));

                using (var target = new SKCanvas(bitmap))
                {
                    target.Clear();
                    if (style.BackColor != null)
                    {
                        var color = style.BackColor?.Color?.ToSkia();
                        if (color.HasValue)
                        {
                            var rounding = 6;
                            using (var backgroundPaint = new SKPaint { Color = color.Value })
                            {
                                target.DrawRoundRect(new SKRect(0, 0, rect.Width, rect.Height), rounding, rounding, backgroundPaint);
                            }
                        }
                    }
                    target.DrawText(text, -rect.Left, -rect.Top, paint);
                    return bitmap;
                }
            }
        }
    }
}