
namespace Mapsui.UI.Eto.Extensions;

using global::Eto.Drawing;
public static class Extensions
{
    public static MPoint ToMapsui(this PointF point)
    {
        return new MPoint(point.X, point.Y);
    }
}
