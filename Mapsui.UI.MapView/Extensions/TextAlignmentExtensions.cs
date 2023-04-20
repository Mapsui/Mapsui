using Mapsui.Widgets;

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

public static class TextAlignmentExtensions
{
    /// <summary>
    /// Convert Xamarin.Forms.TextAlignment to Mapsui/RichTextKit.Styles.Color
    /// </summary>
    /// <param name="textAlignment">TextAlignment in Xamarin.Forms format</param>
    /// <returns>TextAlignment in Mapsui/RichTextKit format</returns>
    public static Alignment ToMapsui(this TextAlignment textAlignment)
    {
        Alignment result;

        switch (textAlignment)
        {
            case TextAlignment.Start:
                result = Alignment.Left;
                break;
            case TextAlignment.Center:
                result = Alignment.Center;
                break;
            case TextAlignment.End:
                result = Alignment.Right;
                break;
            default:
                result = Alignment.Auto;
                break;
        }

        return result;
    }
}
