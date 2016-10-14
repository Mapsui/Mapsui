using System;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class PlatformLabelBitmap
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

                using (var targetBitmap = new SKBitmap((int) Math.Ceiling(rect.Width), (int) Math.Ceiling(rect.Height)))
                using (var targetGraphics = new SKCanvas(targetBitmap))
                {
                    targetGraphics.Clear(style.BackColor.Color.ToSkia());
                    targetGraphics.DrawText(text, -rect.Left, -rect.Top, paint);
                    return targetBitmap;
                }
            }
        }
    }
}