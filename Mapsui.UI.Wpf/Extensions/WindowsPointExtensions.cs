using Mapsui.Manipulations;
using System.Windows;

namespace Mapsui.UI.Wpf.Extensions;

public static class WindowsPointExtensions
{
    public static ScreenPosition ToScreenPosition(this Point point)
    {
        return new ScreenPosition(point.X, point.Y);
    }
}
