using System;
using System.Collections.Generic;

namespace Mapsui.UI.Forms
{
    /// <summary>
    /// MapSpan represents an area of the earth
    /// </summary>
    public class MapSpan
    {
        /// <summary>
        /// Radius of earth in EPSG:4327 in kilometers
        /// </summary>
        const double EarthRadiusKm = 6378.137;

        /// <summary>
        /// Circumference of earth in km.
        /// </summary>
        const double EarthCircumferenceKm = EarthRadiusKm * 2 * Math.PI;

        /// <summary>
        /// Minimum range degrees, here 1 m in degrees
        /// </summary>
        const double MinimumRangeDegrees = 0.001 / EarthCircumferenceKm * 360; // 1 meter

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Mapsui.UI.Forms.MapSpan"/> class.
        /// </summary>
        /// <param name="center">Center of the MapSpan</param>
        /// <param name="latitudeDegrees">Extend of MapSpand in latitude direction in degrees</param>
        /// <param name="longitudeDegrees">Extend of MapSpand in longitude direction in degrees</param>
        public MapSpan(Position center, double latitudeDegrees, double longitudeDegrees)
        {
            Center = center;
            LatitudeDegrees = Math.Min(Math.Max(latitudeDegrees, MinimumRangeDegrees), 90.0);
            LongitudeDegrees = Math.Min(Math.Max(longitudeDegrees, MinimumRangeDegrees), 180.0);
        }

        /// <summary>
        /// Center of MapSpan
        /// </summary>
        /// <value>Center of MapSpan</value>
        public Position Center { get; }

        /// <summary>
        /// Extend of MapSpan in latitude degrees
        /// </summary>
        /// <value>Extend in latitude degrees</value>
        public double LatitudeDegrees { get; }

        /// <summary>
        /// Extend of MapSpan in longitude degrees
        /// </summary>
        /// <value>Extend in longitude degrees</value>
        public double LongitudeDegrees { get; }

        /// <summary>
        /// Radius of smallest circle thats fit into MapSpan
        /// </summary>
        /// <value>The radius.</value>
        public Distance Radius
        {
            get
            {
                double latKm = LatitudeDegreesToKm(LatitudeDegrees);
                double longKm = LongitudeDegreesToKm(Center, LongitudeDegrees);
                return new Distance(1000 * Math.Min(latKm, longKm) / 2);
            }
        }

        /// <summary>
        /// Clamp latitude between north and south
        /// </summary>
        /// <returns>MapSpan with center clamped between north and south</returns>
        /// <param name="north">Maximal latitude degrees for north</param>
        /// <param name="south">Maximal latitude degrees for south</param>
        public MapSpan ClampLatitude(double north, double south)
        {
            north = Math.Min(Math.Max(north, 0), 90);
            south = Math.Max(Math.Min(south, 0), -90);
            double lat = Math.Max(Math.Min(Center.Latitude, north), south);
            double maxDLat = Math.Min(north - lat, -south + lat) * 2;
            return new MapSpan(new Position(lat, Center.Longitude), Math.Min(LatitudeDegrees, maxDLat), LongitudeDegrees);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/> is equal to the current <see cref="T:Mapsui.UI.Forms.MapSpan"/>
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with current <see cref="T:Mapsui.UI.Forms.MapSpan"/></param>
        /// <returns><c>true</c> if the specified <see cref="object"/> is equal to the current
        /// <see cref="T:Mapsui.UI.Forms.MapSpan"/>; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            return obj is MapSpan && Equals((MapSpan)obj);
        }

        /// <summary>
        /// Create a new MapSpan from center and radius
        /// </summary>
        /// <returns>New MapSpan</returns>
        /// <param name="center">Center for new MapSpan</param>
        /// <param name="radius">Radius for new MapSpan</param>
        public static MapSpan FromCenterAndRadius(Position center, Distance radius)
        {
            return new MapSpan(center, 2 * DistanceToLatitudeDegrees(radius), 2 * DistanceToLongitudeDegrees(center, radius));
        }

        /// <summary>
        /// Create a new MapSpan from multiple positions
        /// </summary>
        /// <returns>New MapSpan</returns>
        /// <param name="positions">List of positions to get the new MapSpan</param>
        public static MapSpan FromPositions(IEnumerable<Position> positions)
        {
            double minLat = double.MaxValue;
            double minLon = double.MaxValue;
            double maxLat = double.MinValue;
            double maxLon = double.MinValue;

            foreach (var p in positions)
            {
                minLat = Math.Min(minLat, p.Latitude);
                minLon = Math.Min(minLon, p.Longitude);
                maxLat = Math.Max(maxLat, p.Latitude);
                maxLon = Math.Max(maxLon, p.Longitude);
            }
            return new MapSpan(new Position((minLat + maxLat) / 2d, (minLon + maxLon) / 2d), maxLat - minLat, maxLon - minLon);
        }
        
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Center.GetHashCode();
                hashCode = (hashCode * 397) ^ LongitudeDegrees.GetHashCode();
                hashCode = (hashCode * 397) ^ LatitudeDegrees.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(MapSpan left, MapSpan right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MapSpan left, MapSpan right)
        {
            return !Equals(left, right);
        }

        /// <summary>
        /// Create a new MapSPan with given zoom factor
        /// </summary>
        /// <returns>New MapSpan</returns>
        /// <param name="zoomFactor">Zoom factor</param>
        public MapSpan WithZoom(double zoomFactor)
        {
            double maxDLat = Math.Min(90 - Center.Latitude, 90 + Center.Latitude) * 2;
            return new MapSpan(Center, Math.Min(LatitudeDegrees / zoomFactor, maxDLat), LongitudeDegrees / zoomFactor);
        }

        /// <summary>
        /// Convert a distance into latitude degrees.
        /// </summary>
        /// <returns>Latitude degrees</returns>
        /// <param name="distance">Distance</param>
        static double DistanceToLatitudeDegrees(Distance distance)
        {
            return distance.Kilometers / EarthCircumferenceKm * 360;
        }

        /// <summary>
        /// Convert a distances into longitude degrees
        /// </summary>
        /// <returns>Longitude degrees</returns>
        /// <param name="position">Position for latitude to use for circumference</param>
        /// <param name="distance">Distance</param>
        static double DistanceToLongitudeDegrees(Position position, Distance distance)
        {
            double latCircumference = LatitudeCircumferenceKm(position);
            return distance.Kilometers / latCircumference * 360;
        }

        bool Equals(MapSpan other)
        {
            return Center.Equals(other.Center) && LongitudeDegrees.Equals(other.LongitudeDegrees) && LatitudeDegrees.Equals(other.LatitudeDegrees);
        }

        /// <summary>
        /// Calculate circumference in km for given latitude
        /// </summary>
        /// <returns>Circumference in km</returns>
        /// <param name="position">Position to calculate circumference for</param>
        static double LatitudeCircumferenceKm(Position position)
        {
            return EarthCircumferenceKm * Math.Cos(position.Latitude * Math.PI / 180.0);
        }

        /// <summary>
        /// Calculate distance for latitudes degrees in km
        /// </summary>
        /// <returns>Distance in km</returns>
        /// <param name="latitudeDegrees">Latitude degrees</param>
        static double LatitudeDegreesToKm(double latitudeDegrees)
        {
            return EarthCircumferenceKm * latitudeDegrees / 360;
        }

        /// <summary>
        /// Calculate distance for longitude degrees in km
        /// </summary>
        /// <returns>Distance in km</returns>
        /// <param name="position">Position for latitude to use for calculation</param>
        /// <param name="longitudeDegrees">Longitude degrees</param>
        static double LongitudeDegreesToKm(Position position, double longitudeDegrees)
        {
            double latCircumference = LatitudeCircumferenceKm(position);
            return latCircumference * longitudeDegrees / 360;
        }
    }
}
