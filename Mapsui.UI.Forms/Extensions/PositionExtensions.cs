namespace Mapsui.UI.Forms.Extensions
{
    public static class PositionExtensions
    {
        /// <summary>
        /// Convert Mapsui.Geometries.Point to Xamarin.Forms.Maps.Position
        /// </summary>
        /// <param name="point">Point in Mapsui format</param>
        /// <returns>Position in Xamarin.Forms.Maps format</returns>
        public static Position ToForms(this Geometries.Point point)
        {
            var latLon = Projection.SphericalMercator.ToLonLat(point.X, point.Y);

            return new Position(latLon.Y, latLon.X);
        }
    }
}