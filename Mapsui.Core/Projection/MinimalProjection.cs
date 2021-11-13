using System;
using System.Collections.Generic;
using Mapsui.Layers;

namespace Mapsui.Projection
{
    /// <summary>
    /// A very minimal implementation that is only capable of projecting between
    /// SphericalMercator and WGS84.
    /// </summary>
    public class MinimalProjection : IProjection
    {
        private readonly IDictionary<string, Func<double, double, (double, double)>> _toLonLat =
            new Dictionary<string, Func<double, double, (double, double)>>();

        private readonly IDictionary<string, Func<double, double, (double, double)>> _fromLonLat =
            new Dictionary<string, Func<double, double, (double, double)>>();

        public MinimalProjection()
        {
            _toLonLat["EPSG:4326"] = (x, y) => (x, y);
            _fromLonLat["EPSG:4326"] = (x, y) => (x, y);
            _toLonLat["EPSG:3857"] = SphericalMercator.ToLonLat;
            _fromLonLat["EPSG:3857"] = SphericalMercator.FromLonLat;
        }

        public (double X, double Y) Project(string fromCRS, string toCRS, double x, double y)
        {
            var (lon, lat) = Project(x, y, _toLonLat[fromCRS]);
            return Project(lon, lat, _fromLonLat[toCRS]);
        }

        private static (double X, double Y) Project(double x, double y, Func<double, double, (double, double)> projectFunc)
        {
            return projectFunc(x, y);
        }

        public bool IsProjectionSupported(string fromCRS, string toCRS)
        {
            return _toLonLat.ContainsKey(fromCRS) && _fromLonLat.ContainsKey(toCRS);
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

        public void Project(string fromCRS, string toCRS, IFeature feature)
        {
            Project(feature, _toLonLat[fromCRS]);
            Project(feature, _fromLonLat[toCRS]);
        }

        private static void Project(IFeature feature, Func<double, double, (double, double)> transformFunc)
        {
            feature.CoordinateVisitor((x, y, setter) => {
                var (xOut, yOut) = transformFunc(x, y);
                setter(xOut, yOut);
            });
        }
    }
}
