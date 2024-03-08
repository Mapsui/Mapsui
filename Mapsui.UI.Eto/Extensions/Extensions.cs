namespace Mapsui.UI.Eto.Extensions;

using global::Eto.Drawing;
using Mapsui.Manipulations;

public static class Extensions
{
    public static ScreenPosition ToScreenPosition(this PointF point)
    {
        return new ScreenPosition(point.X, point.Y);
    }
}
