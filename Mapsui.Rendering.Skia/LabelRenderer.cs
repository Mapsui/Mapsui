using System.Collections.Generic;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public static class LabelRenderer
    {
        private static readonly IDictionary<string, SKBitmapInfo> LabelBitmapCache = new Dictionary<string, SKBitmapInfo>();

        public static void Draw(SKCanvas canvas, LabelStyle style, string text, float x, float y)
        {
            var key = text + "_" + style.Font.FontFamily + "_" + style.Font.Size + "_" + (float)style.Font.Size + "_" + style.BackColor + "_" + style.ForeColor;

            if (!LabelBitmapCache.Keys.Contains(key))
            {
                LabelBitmapCache[key] = new SKBitmapInfo { Bitmap = PlatformLabelBitmap.Create(style, text) };
            }

            var info = LabelBitmapCache[key];

            TextureHelper.RenderTexture(canvas, info.Bitmap, x, y, 
                offsetX:(float)style.Offset.X, offsetY:(float)style.Offset.Y,
                horizontalAlignment: style.HorizontalAlignment, verticalAlignment: style.VerticalAlignment);
        }
    }
}
