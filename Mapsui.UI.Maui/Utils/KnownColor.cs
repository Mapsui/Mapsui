using Mapsui.UI.Maui.Extensions;
using Color = Microsoft.Maui.Graphics.Color;

namespace Mapsui.UI.Maui;

/// <summary> Known Color Helper for Maui </summary>
public static class KnownColor
{
    public static Color White => System.Drawing.Color.White.ToNative();
    public static Color Red => System.Drawing.Color.Red.ToNative();
    public static Color Black => System.Drawing.Color.Black.ToNative();
    public static Color DarkGray => System.Drawing.Color.DarkGray.ToNative();
}
