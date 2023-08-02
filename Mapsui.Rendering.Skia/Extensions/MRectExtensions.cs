using System;
using SkiaSharp;

namespace Mapsui.Rendering.Skia.Extensions;

public static class MRectExtensions
{
    private const float _minFloatValue = float.MinValue / 2 - 1;
    private const float _maxFloatValue = float.MaxValue / 2 - 1;

    private static readonly SKRect _maxSkRect = new SKRect(_minFloatValue, _minFloatValue, _maxFloatValue, _maxFloatValue);

    private static readonly Viewport _maxViewPort = new Viewport(0, 0, 1, 0, 0, 0);

    public static SKRect ToSkia(this MRect? rect)
    {
        if (rect == null)
        {
            return _maxSkRect;
        }

        return new SKRect((float)rect.MinX, (float)rect.MinY, (float)rect.MaxX, (float)rect.MaxY);
    }

    public static Viewport ToViewPort(this MRect? rect)
    {
        if (rect == null)
        {
            return _maxViewPort;
        }

        var centroid = rect.Centroid;
        return new Viewport(centroid.X, centroid.Y, 1, 0, rect.Width, rect.Height);
    }

}
