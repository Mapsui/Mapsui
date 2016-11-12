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

                var padding = 4;
                rect = SKRect.Inflate(rect, padding, padding);

                var targetBitmap = new SKBitmap((int) Math.Ceiling(rect.Width), (int) Math.Ceiling(rect.Height));
                


                using (var target = new SKCanvas(targetBitmap))
                {
                    target.Clear((style.BackColor == null) ? new SKColor() : style.BackColor.Color.ToSkia());
                    target.DrawText(text, -rect.Left, -rect.Top, paint);
                    return targetBitmap;
                }
            }
        }
    }
}