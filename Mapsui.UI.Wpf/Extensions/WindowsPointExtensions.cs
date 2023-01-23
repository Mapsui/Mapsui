using System.Windows;

namespace Mapsui.UI.Wpf.Extensions;

public static class WindowsPointExtensions
{
    public static MPoint ToMapsui(this Point point)
    {
        return new MPoint(point.X, point.Y);
    }
}
