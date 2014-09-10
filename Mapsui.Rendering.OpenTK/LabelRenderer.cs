using Mapsui.Styles;
using System.Collections.Generic;

namespace Mapsui.Rendering.OpenTK
{
    public static class LabelRenderer
    {
        private static readonly IDictionary<string, TextureInfo> LabelBitmapCache = new Dictionary<string, TextureInfo>();

        public static void Draw(LabelStyle style, string text, float x, float y)
        {
            var key = text + "_" + style.Font.FontFamily + "_" + style.Font.Size + "_" + (float)style.Font.Size + "_" + style.BackColor + "_" + style.ForeColor;

            if (!LabelBitmapCache.Keys.Contains(key))
            {
                var memoryStream = PlatformLabelBitmap.Create(style, text);
                LabelBitmapCache[key] = TextureHelper.LoadTexture(memoryStream);
            }
            var info = LabelBitmapCache[key];
            TextureHelper.RenderTexture(info, x, y, offsetX:(float)style.Offset.X, offsetY:(float)style.Offset.Y,
                horizontalAlignment: style.HorizontalAlignment, verticalAlignment: style.VerticalAlignment);
        }
    }
}
