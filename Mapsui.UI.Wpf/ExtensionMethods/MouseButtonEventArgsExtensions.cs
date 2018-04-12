using System.Windows.Input;

namespace Mapsui.UI.Wpf
{
    public static class MouseButtonEventArgsExtensions
    {
        public static MapInfo GetMapInfo(this MouseButtonEventArgs e, MapControl mapControl)
        {
            var screenPosition = e.GetPosition(mapControl).ToMapsui();
            return mapControl.GetMapInfo(screenPosition);
        }
    }
}