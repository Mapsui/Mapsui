#if __MAUI__
namespace Mapsui.UI.Maui;
#elif __UWP__
namespace Mapsui.UI.Uwp;
#elif __ANDROID__ && !HAS_UNO_WINUI
namespace Mapsui.UI.Android;
#elif __IOS__ && !HAS_UNO_WINUI && !__FORMS__
namespace Mapsui.UI.iOS;
#elif __WINUI__
namespace Mapsui.UI.WinUI;
#elif __FORMS__
namespace Mapsui.UI.Forms;
#elif __AVALONIA__
namespace Mapsui.UI.Avalonia;
#elif __ETO_FORMS__
namespace Mapsui.UI.Eto;
#elif __BLAZOR__
namespace Mapsui.UI.Blazor;
#elif __WPF__
namespace Mapsui.UI.Wpf;
#else
namespace Mapsui.UI;
#endif

public static class ZoomLevelExtensions
{
    /// <summary>
    /// Convert zoom level (as described at https://wiki.openstreetmap.org/wiki/Zoom_levels) into a Mapsui resolution
    /// </summary>
    /// <param name="zoomLevel">Zoom level</param>
    /// <returns>Resolution in Mapsui format</returns>
    public static double ToMapsuiResolution(this double zoomLevel)
    {
        if (zoomLevel < 0 || zoomLevel > 30)
            return 0;

        return 156543.03392 / System.Math.Pow(2, zoomLevel);
    }
}
