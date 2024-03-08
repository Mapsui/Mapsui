using Avalonia;
using Mapsui.Manipulations;

namespace Mapsui.UI.Avalonia.Extensions;

public static class PointExtensions
{
    public static ScreenPosition ToScreenPosition(this Point point)
    {
        return new ScreenPosition(point.X, point.Y);
    }
}
