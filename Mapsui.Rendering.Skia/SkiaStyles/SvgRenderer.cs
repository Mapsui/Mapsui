using SkiaSharp;
using Svg.Skia;
using System;

namespace Mapsui.Rendering.Skia;

public class SvgRenderer
{
    public static void Draw(SKCanvas canvas, SKSvg svg, float x, float y, float orientation = 0,
        float offsetX = 0, float offsetY = 0, float opacity = 1f, float scale = 1f)
    {
        if (svg.Picture == null)
            return;

        canvas.Save();

        canvas.Translate(x, y);
        canvas.RotateDegrees(orientation, 0, 0); // todo: degrees or radians?
        canvas.Scale(scale, scale);

        var halfWidth = svg.Picture.CullRect.Width / 2;
        var halfHeight = svg.Picture.CullRect.Height / 2;

        // 0/0 are assumed at center of image, but Svg has 0/0 at left top position
        canvas.Translate(-halfWidth + offsetX, -halfHeight - offsetY);

        var alpha = Convert.ToByte(255 * opacity);
        var transparency = SKColors.White.WithAlpha(alpha);
        using (var cf = SKColorFilter.CreateBlendMode(transparency, SKBlendMode.DstIn))
        {
            using var skPaint = new SKPaint
            {
                IsAntialias = true,
                ColorFilter = cf,
            };
            canvas.DrawPicture(svg.Picture, skPaint);
        }

        canvas.Restore();
    }
}
