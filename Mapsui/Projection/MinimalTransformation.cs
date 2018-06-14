using Mapsui.Geometries;
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
        private readonly IDictionary<string, Func<double, double, Point>> _toLonLat = new Dictionary<string, Func<double, double, Point>>();
        private readonly IDictionary<string, Func<double, double, Point>> _fromLonLat = new Dictionary<string, Func<double, double, Point>>();

        public MinimalTransformation()
        {
            _toLonLat["EPSG:4326"] = (x, y) => new Point(x, y);
            _fromLonLat["EPSG:4326"] = (x, y) => new Point(x, y);
            _toLonLat["EPSG:3857"] = SphericalMercator.ToLonLat;
            _fromLonLat["EPSG:3857"] = SphericalMercator.FromLonLat;
        }

        public IGeometry Transform(string fromCRS, string toCRS, IGeometry geometry)
        {
            Transform(geometry.AllVertices(), _toLonLat[fromCRS]);
            Transform(geometry.AllVertices(), _fromLonLat[toCRS]);
            return geometry; // this method should not have a return value
        }

        private static void Transform(IEnumerable<Point> points, Func<double, double, Point> transformFunc)
        {
            foreach (var point in points)
            {
                var transformed = transformFunc(point.X, point.Y);
                point.X = transformed.X;
                point.Y = transformed.Y;
            }
        }

        public BoundingBox Transform(string fromCRS, string toCRS, BoundingBox boundingBox)
        {
            Transform(boundingBox.AllVertices(), _toLonLat[fromCRS]);
            Transform(boundingBox.AllVertices(), _fromLonLat[toCRS]);
            return boundingBox; // this method not have a return value
        }

        public bool? IsProjectionSupported(string fromCRS, string toCRS)
        {
            if (!_toLonLat.ContainsKey(fromCRS)) return false;
            if (!_fromLonLat.ContainsKey(toCRS)) return false;
            return true;
        }
    }
}
