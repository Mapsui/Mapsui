using Mapsui.Styles;
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using Bitmap = System.Drawing.Bitmap;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;

namespace Mapsui.Rendering.OpenTK
{
    public class PlatformLabelBitmap
    {
        private static Bitmap _bitmap;
        private static Graphics _graphics;

        static PlatformLabelBitmap()
        {
            InitializeBitmapAndGraphics(16, 16);
            _graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
        }

        private static void InitializeBitmapAndGraphics(int width, int height)
        {
            if (_graphics != null) _graphics.Dispose();
            if (_bitmap != null) _bitmap.Dispose();
            _bitmap = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            _graphics = Graphics.FromImage(_bitmap);
        }

        public static MemoryStream Create(LabelStyle style, string text)
        {
            var font = new Font(style.Font.FontFamily, (float)style.Font.Size, FontStyle.Bold);

            var size = _graphics.MeasureString(text, font);
            var targetBitmap = new Bitmap((int)Math.Ceiling(size.Width), (int)Math.Ceiling(size.Height),
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            var targetGraphics = Graphics.FromImage(targetBitmap);
            targetGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;

            // Render a text label
            targetGraphics.Clear((style.BackColor == null) ? Color.Transparent : ToGdi(style.BackColor.Color));
            targetGraphics.DrawString(text, font, new SolidBrush(ToGdi(style.ForeColor)), PointF.Empty);

            var memoryStream = new MemoryStream();
            targetBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            memoryStream.Position = 0;
            return memoryStream;
        }

        private static Color ToGdi(Styles.Color color)
        {
            return Color.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
