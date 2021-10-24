using System;
using System.Collections.Generic;

namespace Mapsui.Projection
{
    /// <summary>
    /// A very minimal implementation of ITransformation. It is only capable of projecting between
    /// SphericalMercator and WGS84.
    /// </summary>
    public class MinimalTransformation : ITransformation
    {
        private readonly IDictionary<string, Func<double, double, (double, double)>> _toLonLat =
            new Dictionary<string, Func<double, double, (double, double)>>();

        private readonly IDictionary<string, Func<double, double, (double, double)>> _fromLonLat =
            new Dictionary<string, Func<double, double, (double, double)>>();

        public MinimalTransformation()
        {
            _toLonLat["EPSG:4326"] = (x, y) => (x, y);
            _fromLonLat["EPSG:4326"] = (x, y) => (x, y);
            _toLonLat["EPSG:3857"] = SphericalMercator.ToLonLat;
            _fromLonLat["EPSG:3857"] = SphericalMercator.FromLonLat;
        }

        public (double X, double Y) Transform(string fromCRS, string toCRS, double x, double y)
        {
            var (lon, lat) = Transform(x, y, _toLonLat[fromCRS]);
            return Transform(lon, lat, _fromLonLat[toCRS]);
        }

        private static (double X, double Y) Transform(double x, double y, Func<double, double, (double, double)> transformFunc)
        {
            return transformFunc(x, y);
        }

        public bool? IsProjectionSupported(string fromCRS, string toCRS)
        {
            if (!_toLonLat.ContainsKey(fromCRS)) return false;
            if (!_fromLonLat.ContainsKey(toCRS)) return false;

            return true;
        }
    }
}
