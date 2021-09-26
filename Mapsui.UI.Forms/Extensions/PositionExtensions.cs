#if __MAUI__
namespace Mapsui.UI.Maui.Extensions
#else
namespace Mapsui.UI.Forms.Extensions
#endif
{
    public static class PositionExtensions
    {
#if __MAUI__
        /// <summary>
        /// Convert Mapsui.Geometries.Point to Mapsui.UI.Maui.Position
        /// </summary>
        /// <param name="point">Point in Mapsui format</param>
        /// <returns>Position in Xamarin.Forms.Maps format</returns>
        public static Position ToMaui(this Geometries.Point point)
#else
        /// <summary>
        /// Convert Mapsui.Geometries.Point to Xamarin.Forms.Maps.Position
        /// </summary>
        /// <param name="point">Point in Mapsui format</param>
        /// <returns>Position in Xamarin.Forms.Maps format</returns>
        public static Position ToForms(this Geometries.Point point)
#endif
        {
            return point.ToNative();
        }

        public static Position ToNative(this Geometries.Point point)
        {
            var latLon = Projection.SphericalMercator.ToLonLat(point.X, point.Y);
            return new Position(latLon.Y, latLon.X);
        }
    }
}