using Avalonia;

namespace Mapsui.UI.Avalonia.V0.Extensions;

public static class PointExtensions
{
    public static MPoint ToMapsui(this Point point)
    {
        return new MPoint(point.X, point.Y);
    }
}
