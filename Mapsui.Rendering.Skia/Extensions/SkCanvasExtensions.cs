using Mapsui.Rendering.Skia.Cache;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class SkCanvasExtensions
{
    public static void DrawPath(this SKCanvas canvas, CacheTracker<SKPath> path, CacheTracker<SKPaint> paint)
    {
        canvas.DrawPath(path.Instance, paint.Instance);
    }

    public static void DrawRect(this SKCanvas canvas, SKRect rect, CacheTracker<SKPaint> paint)
    {
        canvas.DrawRect(rect, paint.Instance);
    }
}
