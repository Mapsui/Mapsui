using CoreGraphics;

namespace Mapsui.UI.iOS.Extensions;

internal static class CGPointExtensions
{
    public static MPoint ToMapsui(this CGPoint point)
    {
        return new MPoint(point.X, point.Y);
    }
}
