using SkiaSharp;
using Svg.Skia;
using System;
using Mapsui.Rendering.Skia.Extensions;
using Color = Mapsui.Styles.Color;

namespace Mapsui.Rendering.Skia;

public class SvgRenderer
{
    public static void Draw(SKCanvas canvas, SKSvg svg, float x, float y, float orientation = 0,
        float offsetX = 0, float offsetY = 0, float opacity = 1f, float scale = 1f,
        Color? blendModeColor = null)
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

        var transparencyColor = SKColors.White;

        SKPaint? paint = null;
        SKColorFilter? colorFilter = null;

        if (blendModeColor != null)
        {
            var color = blendModeColor.Value.ToSkia().WithAlpha(alpha);
            colorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.SrcIn);
        }
        else
        {
            var color = SKColors.White.WithAlpha(alpha);
            colorFilter = SKColorFilter.CreateBlendMode(color, SKBlendMode.DstIn);
        }

        if (colorFilter != null)
        {
            paint = new SKPaint()
            {
                IsAntialias = true,
                ColorFilter = colorFilter,
            };
        }
        
        canvas.DrawPicture(svg.Picture, paint);
        paint?.Dispose();
        colorFilter?.Dispose();

        canvas.Restore();
    }
}
