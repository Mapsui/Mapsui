using Mapsui.Manipulations;
using Windows.Foundation;

namespace Mapsui.UI.WinUI.Extensions;

public static class PointExtensions
{
    public static ScreenPosition ToScreenPosition(this Point point)
    {
        return new ScreenPosition(point.X, point.Y);
    }
}
