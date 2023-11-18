namespace Mapsui.UI.Maui.Extensions;

public static class ColorExtensions
{
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

    /// <summary>
    /// Convert Microsoft.Maui.Graphics.Color to Mapsui.Style.Color
    /// </summary>
    /// <param name="color">Color in Microsoft.Maui.Graphics.Color format </param>
    /// <returns>Color in Mapsui.Styles.Color format</returns>
    public static Styles.Color ToMapsui(this Microsoft.Maui.Graphics.Color color)
    {
        return new Styles.Color((int)(color.Red * 255), (int)(color.Green * 255), (int)(color.Blue * 255), (int)(color.Alpha * 255));
    }
}
