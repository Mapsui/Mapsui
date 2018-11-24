namespace Mapsui.UI.Forms.Extensions
{
    public static class ColorExtensions
    {
        /// <summary>
        /// Convert Mapsui.Styles.Color to Xamarin.Forms.Color
        /// </summary>
        /// <param name="color">Color in Mapsui format</param>
        /// <returns>Color in Xamarin.Forms.Maps format</returns>
        public static Xamarin.Forms.Color ToForms(this Styles.Color color)
        {
            return new Xamarin.Forms.Color(color.R / 255.0, color.G / 255.0, color.B / 255.0, color.A / 255.0);
        }

        /// <summary>
        /// Convert Xamarin.Forms.Color to Mapsui.Style.Color
        /// </summary>
        /// <param name="color">Color in Xamarin.Forms.Color format </param>
        /// <returns>Color in Mapsui.Styles.Color format</returns>
        public static Styles.Color ToMapsui(this Xamarin.Forms.Color color)
        {
            return new Styles.Color((int)(color.R * 255), (int)(color.G * 255), (int)(color.B * 255), (int)(color.A * 255));
        }
    }
}