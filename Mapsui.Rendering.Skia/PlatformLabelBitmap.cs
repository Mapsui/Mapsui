using System;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class PlatformLabelBitmap
    {
        public static SKBitmap Create(LabelStyle style, string text)
        {
            using (var paint = new SKPaint())
            {
                paint.TextSize = (float) style.Font.Size;
                paint.IsAntialias = true;
                paint.Color = style.ForeColor.ToSkia();
                paint.Typeface = SKTypeface.FromFamilyName(style.Font.FontFamily);
                paint.IsStroke = false;
                paint.FakeBoldText = true;
                paint.IsEmbeddedBitmapText = true;
                
                var rect = new SKRect();
                paint.MeasureText(text, ref rect);

                var padding = 4; // todo get this from LabelStyle
                rect = SKRect.Inflate(rect, padding, padding);

                var bitmap = new SKBitmap((int) Math.Ceiling(rect.Width), (int) Math.Ceiling(rect.Height));

                using (var target = new SKCanvas(bitmap))
                {
                    target.Clear();
                    if (style.BackColor != null)
                    {
                        var color = style.BackColor?.Background?.ToSkia();
                        if (color.HasValue)
                        {
                            var rounding = 5;
                            using (var backgroundPaint = new SKPaint {Color = color.Value})
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