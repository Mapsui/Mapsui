using Mapsui.Projections;

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

public static class PositionExtensions
{
#if __MAUI__
    /// <summary>
    /// Convert Mapsui.Geometries.Point to Mapsui.UI.Maui.Position
    /// </summary>
    /// <param name="point">Point in Mapsui format</param>
    /// <returns>Position in Xamarin.Forms.Maps format</returns>
    public static Position ToMaui(this MPoint point)
#else
    /// <summary>
    /// Convert Mapsui.Geometries.Point to Xamarin.Forms.Maps.Position
    /// </summary>
    /// <param name="point">Point in Mapsui format</param>
    /// <returns>Position in Xamarin.Forms.Maps format</returns>
    public static Position ToForms(this MPoint point)
#endif
    {
        return point.ToNative();
    }

    public static Position ToNative(this MPoint point)
    {
        var result = SphericalMercator.ToLonLat(point.X, point.Y);
        return new Position(result.lat, result.lon);
    }
}
