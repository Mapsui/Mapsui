using Mapsui.UI.Maui.Extensions;
using Color = Microsoft.Maui.Graphics.Color;

namespace Mapsui.UI.Maui;

/// <summary> Known Color Helper for Maui </summary>
public static class KnownColor
{
    public static Color White => Styles.Color.White.ToMaui();
    public static Color Red => Styles.Color.Red.ToMaui();
    public static Color Black => Styles.Color.Black.ToMaui();
    public static Color DarkGray => Styles.Color.DarkGray.ToMaui();
}
