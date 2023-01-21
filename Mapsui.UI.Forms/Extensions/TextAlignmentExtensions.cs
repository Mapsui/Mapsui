using Mapsui.Widgets;

#if __MAUI__
using Microsoft.Maui;
#else
using Xamarin.Forms;
#endif

#if __MAUI__
namespace Mapsui.UI.Maui.Extensions;
#else
namespace Mapsui.UI.Forms.Extensions;
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
