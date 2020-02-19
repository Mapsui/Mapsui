namespace Mapsui.UI.Forms.Extensions
{
    public static class TextAlignmentExtensions
    {
        /// <summary>
        /// Convert Xamarin.Forms.TextAlignment to Mapsui/RichTextKit.Styles.Color
        /// </summary>
        /// <param name="textAlignment">TextAlignment in Xamarin.Forms format</param>
        /// <returns>TextAlignment in Mapsui/RichTextKit format</returns>
        public static Topten.RichTextKit.TextAlignment ToMapsui(this Xamarin.Forms.TextAlignment textAlignment)
        {
            Topten.RichTextKit.TextAlignment result;

            switch (textAlignment)
            {
                case Xamarin.Forms.TextAlignment.Start:
                    result = Topten.RichTextKit.TextAlignment.Left;
                    break;
                case Xamarin.Forms.TextAlignment.Center:
                    result = Topten.RichTextKit.TextAlignment.Center;
                    break;
                case Xamarin.Forms.TextAlignment.End:
                    result = Topten.RichTextKit.TextAlignment.Right;
                    break;
                default:
                    result = Topten.RichTextKit.TextAlignment.Auto;
                    break;
            }

            return result;
        }
    }
}