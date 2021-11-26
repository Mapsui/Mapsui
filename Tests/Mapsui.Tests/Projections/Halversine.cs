// This code is copied from this location. Not sure what license it is.
// https://gist.github.com/jammin77/033a332542aa24889452
using System;

namespace Mapsui.Tests.Projections
{
    /// <summary>
    /// The distance type to return the results in.
    /// </summary>
    public enum DistanceType { Miles, Kilometers };

    /// <summary>
    /// Specifies a Latitude / Longitude point.
    /// </summary>
    public struct Position
    {
        public double Latitude;
        public double Longitude;
    }

    public static class Haversine
    {
        public static double Distance(double lon1, double lat1, double lon2, double lat2, DistanceType type = DistanceType.Kilometers)
        {
            return Distance(new Position { Longitude = lon1, Latitude = lat1 }, new Position { Longitude = lon2, Latitude = lat2 });
        }

        /// <summary>
        /// Returns the distance in miles or kilometers of any two
        /// latitude / longitude points.
        /// </summary>
        /// <param name=”pos1″></param>
        /// <param name=”pos2″></param>
        /// <param name=”type”></param>
        /// <returns></returns>
        public static double Distance(Position pos1, Position pos2, DistanceType type = DistanceType.Kilometers)
        {
            double R = type == DistanceType.Miles ? 3960 : 6371;

            var dLat = ToRadian(pos2.Latitude - pos1.Latitude);
            var dLon = ToRadian(pos2.Longitude - pos1.Longitude);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadian(pos1.Latitude)) * Math.Cos(ToRadian(pos2.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Asin(Math.Min(1, Math.Sqrt(a)));
            var d = R * c;

            return d;
        }

        /// <summary>
        /// Convert to Radians.
        /// </summary>
        /// <param name=”val”></param>
        /// <returns></returns>
        private static double ToRadian(double val)
        {
            return Math.PI / 180 * val;
        }
    }
}