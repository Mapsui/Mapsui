using System;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class PlatformLabelBitmap
    {
        public static SKBitmap Create(LabelStyle style, string text)
        {
            //!!!var font = new Font(style.Font.FontFamily, (float)style.Font.Size, FontStyle.Bold);

            using (var paint = new SKPaint())
            {
                paint.TextSize = 64.0f;
                paint.IsAntialias = true;
                paint.Color = style.ForeColor.ToSkia();
                paint.IsStroke = false;
                
                var rect = new SKRect();

                var size = paint.MeasureText(text, ref rect);
                var targetBitmap = new SKBitmap((int) Math.Ceiling(rect.Width), (int) Math.Ceiling(rect.Height));
                var targetGraphics = new SKCanvas(targetBitmap);
                
                // Render a text label
                targetGraphics.Clear(style.BackColor.Color.ToSkia());
                targetGraphics.DrawText(text, 0, 0, paint);

                return targetBitmap;
            }
        }
    }
}
