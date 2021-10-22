using System;
using System.Collections.Generic;

namespace Mapsui.Projection
{
    /// <summary>
    /// A very minimal implementation of ITransformation. It is only capable of projecting between
    /// SphericalMercator and WGS84.
    /// </summary>
    public class MapsuiTransformation : MinimalTransformation, IMapsuiTransformation
    {
        private readonly IDictionary<string, Func<double, double, (double, double)>> _toLonLat = new Dictionary<string, Func<double, double, (double, double)>>();
        private readonly IDictionary<string, Func<double, double, (double, double)>> _fromLonLat = new Dictionary<string, Func<double, double, (double, double)>>();

        public MapsuiTransformation()
        {
            _toLonLat["EPSG:4326"] = (x, y) => new(x, y);
            _fromLonLat["EPSG:4326"] = (x, y) => new(x, y);
            _toLonLat["EPSG:3857"] = SphericalMercator.ToLonLat;
            _fromLonLat["EPSG:3857"] = SphericalMercator.FromLonLat;
        }

        public void Transform(string fromCRS, string toCRS, MPoint point)
        {
            Transform(point, _toLonLat[fromCRS]);
            Transform(point, _fromLonLat[toCRS]);
        }

        private static void Transform(MPoint point, Func<double, double, (double, double)> transformFunc)
        {
            (point.X, point.Y) = transformFunc(point.X, point.Y);
        }
        
        public void Transform(string fromCRS, string toCRS, MRect rect)
        {
            Transform(rect.Min, _toLonLat[fromCRS]);
            Transform(rect.Min, _fromLonLat[toCRS]);

            Transform(rect.Max, _toLonLat[fromCRS]);
            Transform(rect.Max, _fromLonLat[toCRS]);
        }
    }
}
