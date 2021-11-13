using System;
using System.Collections.Generic;

namespace Mapsui.Projection
{
    /// <summary>
    /// A very minimal implementation that is only capable of projecting between
    /// SphericalMercator and WGS84.
    /// </summary>
    public class MapsuiProjection : MinimalProjection, IMapsuiProjection
    {
        private readonly IDictionary<string, Func<double, double, (double, double)>> _toLonLat = new Dictionary<string, Func<double, double, (double, double)>>();
        private readonly IDictionary<string, Func<double, double, (double, double)>> _fromLonLat = new Dictionary<string, Func<double, double, (double, double)>>();

        public MapsuiProjection()
        {
            _toLonLat["EPSG:4326"] = (x, y) => new(x, y);
            _fromLonLat["EPSG:4326"] = (x, y) => new(x, y);
            _toLonLat["EPSG:3857"] = SphericalMercator.ToLonLat;
            _fromLonLat["EPSG:3857"] = SphericalMercator.FromLonLat;
        }

        public void Project(string fromCRS, string toCRS, MPoint point)
        {
            Project(point, _toLonLat[fromCRS]);
            Project(point, _fromLonLat[toCRS]);
        }

        private static void Project(MPoint point, Func<double, double, (double, double)> projectFunc)
        {
            (point.X, point.Y) = projectFunc(point.X, point.Y);
        }

        public void Project(string fromCRS, string toCRS, MRect rect)
        {
            Project(rect.Min, _toLonLat[fromCRS]);
            Project(rect.Min, _fromLonLat[toCRS]);

            Project(rect.Max, _toLonLat[fromCRS]);
            Project(rect.Max, _fromLonLat[toCRS]);
        }
    }
}
