using Mapsui.Geometries;
using System;
using System.Collections.Generic;

namespace Mapsui.Projection
{
    /// <summary>
    /// A very minimal implementation of ITransformation. It is only capable of projecting between
    /// SphericalMercator and WGS84.
    /// </summary>
    public class GeometryTransformation : MinimalTransformation, IGeometryTransformation
    {
        private readonly IDictionary<string, Func<double, double, (double, double)>> _toLonLat =
            new Dictionary<string, Func<double, double, (double, double)>>();

        private readonly IDictionary<string, Func<double, double, (double, double)>> _fromLonLat =
            new Dictionary<string, Func<double, double, (double, double)>>();

        public GeometryTransformation()
        {
            _toLonLat["EPSG:4326"] = (x, y) => (x, y);
            _fromLonLat["EPSG:4326"] = (x, y) => (x, y);
            _toLonLat["EPSG:3857"] = SphericalMercator.ToLonLat;
            _fromLonLat["EPSG:3857"] = SphericalMercator.FromLonLat;
        }

        public void Transform(string fromCRS, string toCRS, IGeometry geometry)
        {
            Transform(geometry.AllVertices(), _toLonLat[fromCRS]);
            Transform(geometry.AllVertices(), _fromLonLat[toCRS]);
        }

        private static void Transform(IEnumerable<Point> points, Func<double, double, (double, double)> transformFunc)
        {
            foreach (var point in points)
            {
                (point.X, point.Y) = transformFunc(point.X, point.Y);
            }
        }

        public void Transform(string fromCRS, string toCRS, BoundingBox boundingBox)
        {
            Transform(boundingBox.AllVertices(), _toLonLat[fromCRS]);
            Transform(boundingBox.AllVertices(), _fromLonLat[toCRS]);
        }
    }
}
