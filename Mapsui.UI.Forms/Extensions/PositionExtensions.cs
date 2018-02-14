namespace Mapsui.UI.Forms.Extensions
{
    public static class PositionExtensions
    {
        /// <summary>
        /// Convert Mapsui.Geometries.Point to Xamarin.Forms.Maps.Position
        /// </summary>
        /// <param name="point">Point in Mapsui format</param>
        /// <returns>Position in Xamarin.Forms.Maps format</returns>
        public static Position ToForms(this Mapsui.Geometries.Point point)
        {
            var latLon = Mapsui.Projection.SphericalMercator.ToLonLat(point.X, point.Y);

            return new Position(latLon.Y, latLon.X);
        }

        /// <summary>
        /// Convert Xamarin.Forms.Maps.Position to Mapsui.Geometries.Point
        /// </summary>
        /// <param name="position">Point in Xamarin.Forms.Maps.Position format</param>
        /// <returns>Position in Mapsui format</returns>
        public static Mapsui.Geometries.Point ToMapsui(this Position position)
        {
            return Mapsui.Projection.SphericalMercator.FromLonLat(position.Longitude, position.Latitude);
        }
    }
}