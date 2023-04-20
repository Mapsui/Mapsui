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

public static class ColorExtensions
{
#if __MAUI__
    /// <summary>
    /// Convert Mapsui.Styles.Color to Microsoft.Maui.Graphics.Color
    /// </summary>
    /// <param name="color">Color in Mapsui format</param>
    /// <returns>Color in Microsoft.Maui.Graphics format</returns>
    public static Microsoft.Maui.Graphics.Color ToMaui(this Styles.Color color)
    {
        return color.ToNative();
    }

    public static Microsoft.Maui.Graphics.Color ToNative(this Styles.Color color)
    {
        return new Microsoft.Maui.Graphics.Color(color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f);
    }
#else
    /// <summary>
    /// Convert Mapsui.Styles.Color to Xamarin.Forms.Color
    /// </summary>
    /// <param name="color">Color in Mapsui format</param>
    /// <returns>Color in Xamarin.Forms.Maps format</returns>
    public static Xamarin.Forms.Color ToForms(this Styles.Color color)
    {
        return color.ToNative();
    }

    public static Xamarin.Forms.Color ToNative(this Styles.Color color)
    {
        return new Xamarin.Forms.Color(color.R / 255.0, color.G / 255.0, color.B / 255.0, color.A / 255.0);
    }
#endif

#if __MAUI__
    /// <summary>
    /// Convert Microsoft.Maui.Graphics.Color to Mapsui.Style.Color
    /// </summary>
    /// <param name="color">Color in Microsoft.Maui.Graphics.Color format </param>
    /// <returns>Color in Mapsui.Styles.Color format</returns>
    public static Styles.Color ToMapsui(this Microsoft.Maui.Graphics.Color color)
    {
        return new Styles.Color((int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255), (int)(color.Alpha * 255));
    }
#else
    /// <summary>
    /// Convert Xamarin.Forms.Color to Mapsui.Style.Color
    /// </summary>
    /// <param name="color">Color in Xamarin.Forms.Color format </param>
    /// <returns>Color in Mapsui.Styles.Color format</returns>
    public static Styles.Color ToMapsui(this Xamarin.Forms.Color color)
    {
        return new Styles.Color((int)(color.R * 255), (int)(color.G * 255), (int)(color.B * 255), (int)(color.A * 255));
    }
#endif
}
