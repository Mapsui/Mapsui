using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia
{
    public class SvgRenderer
    {
        public static void Draw(SKCanvas canvas, SkiaSharp.Extended.Svg.SKSvg svg, float x, float y, float orientation = 0,
            float offsetX = 0, float offsetY = 0,
            LabelStyle.HorizontalAlignmentEnum horizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Left,
            LabelStyle.VerticalAlignmentEnum verticalAlignment = LabelStyle.VerticalAlignmentEnum.Top,
            float opacity = 1f,
            float scale = 1f)
        {
            // todo: I assume we also need to apply opacity.
            // todo: It seems horizontalAlignment and verticalAlignment would make sense too. Is this similar to Anchor?

            canvas.Save();

            canvas.Translate(x, y);
            canvas.RotateDegrees(orientation, 0, 0); // todo: degrees or radians?
            canvas.Scale(scale, scale);

            var halfWidth = svg.CanvasSize.Width / 2;
            var halfHeight = svg.CanvasSize.Height / 2;

            // 0/0 are assumed at center of image, but Svg has 0/0 at left top position
            canvas.Translate(-halfWidth + offsetX, -halfHeight - offsetY);

            canvas.DrawPicture(svg.Picture, new SKPaint() { IsAntialias = true });

            canvas.Restore();
        }
    }
}
