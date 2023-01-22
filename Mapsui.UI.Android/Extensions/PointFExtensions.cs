using Android.Graphics;

namespace Mapsui.UI.Android.Extensions;

internal static class PointFExtensions
{
    public static MPoint ToMapsui(this PointF point)
    {
        return new MPoint(point.X, point.Y);
    }
}
