using Mapsui.Widgets;
using Microsoft.Maui;

namespace Mapsui.UI.Maui.Extensions;

public static class TextAlignmentExtensions
{
    /// <summary>
    /// Convert Microsoft.Maui.TextAlignment to Mapsui.Widgets.Alignment
    /// </summary>
    /// <param name="textAlignment">TextAlignment in Microsoft.Maui format</param>
    /// <returns>Alignment value in Mapsui.Widgets format</returns>
    public static Alignment ToMapsui(this TextAlignment textAlignment)
    {
        return textAlignment switch
        {
            TextAlignment.Start => Alignment.Left,
            TextAlignment.Center => Alignment.Center,
            TextAlignment.End => Alignment.Right,
            _ => Alignment.Auto,
        };
    }
}
