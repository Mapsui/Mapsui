using System;
using SharpMap.Geometries;

namespace Projection
{
    public static class SphericalMercator
    {
        private readonly static double radius = 6378137;
        private static double D2R = Math.PI / 180;
        private static double HALF_PI = Math.PI / 2;

        public static Point FromLonLat(double lon, double lat)
        {
            double lonRadians = (D2R * lon);
            double latRadians = (D2R * lat);

            double x = radius * lonRadians;
            double y = radius * Math.Log(Math.Tan(Math.PI * 0.25 + latRadians * 0.5));

            return new Point((float)x, (float)y);
        }

        public static Point ToLonLat(double x, double y)
        {
            double ts;
            ts = Math.Exp(-y / (radius));
            double latRadians = HALF_PI - 2 * Math.Atan(ts);

            double lonRadians = x / (radius);

            double lon = (lonRadians / D2R);
            double lat = (latRadians / D2R);

            return new Point((float)lon, (float)lat);
        }
    }
}