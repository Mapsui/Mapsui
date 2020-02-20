using Mapsui.Widgets;

namespace Mapsui.UI.Forms.Extensions
{
    public static class TextAlignmentExtensions
    {
        /// <summary>
        /// Convert Xamarin.Forms.TextAlignment to Mapsui/RichTextKit.Styles.Color
        /// </summary>
        /// <param name="textAlignment">TextAlignment in Xamarin.Forms format</param>
        /// <returns>TextAlignment in Mapsui/RichTextKit format</returns>
        public static Alignment ToMapsui(this Xamarin.Forms.TextAlignment textAlignment)
        {
            Alignment result;

            switch (textAlignment)
            {
                case Xamarin.Forms.TextAlignment.Start:
                    result = Alignment.Left;
                    break;
                case Xamarin.Forms.TextAlignment.Center:
                    result = Alignment.Center;
                    break;
                case Xamarin.Forms.TextAlignment.End:
                    result = Alignment.Right;
                    break;
                default:
                    result = Alignment.Auto;
                    break;
            }

            return result;
        }
    }
}