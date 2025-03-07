using Mapsui.Extensions;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Styles;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.SkiaStyles;

public abstract class PointStyleRenderer
{
    public delegate void PointStyleDrawer(SKCanvas canvas, IPointStyle style, RenderService renderService, float opacity);

    public static void DrawPointStyle(SKCanvas canvas, Viewport viewport, double x, double y, IPointStyle imageStyle,
        RenderService renderService, float opacity, PointStyleDrawer drawPointStyle)
    {
        try
        {
            canvas.Save();

            // Translate to the position
            var (destinationX, destinationY) = viewport.WorldToScreenXY(x, y);
            canvas.Translate((float)destinationX, (float)destinationY);

            // Scale
            canvas.Scale((float)imageStyle.SymbolScale, (float)imageStyle.SymbolScale);

            // Rotate
            var rotation = imageStyle.SymbolRotation;
            if (imageStyle.RotateWithMap)
                rotation += viewport.Rotation;
            if (rotation != 0)
                canvas.RotateDegrees((float)rotation);

            // Translate to offset
            canvas.Translate((float)imageStyle.Offset.X, (float)-imageStyle.Offset.Y);

            drawPointStyle(canvas, imageStyle, renderService, opacity);
        }
        finally
        {
            canvas.Restore();
        }
    }
}
