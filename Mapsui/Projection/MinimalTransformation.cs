using Mapsui.Geometries;
using System;
using System.Collections.Generic;

namespace Mapsui.Projection
{
    public class MinimalTransformation : ITransformation
    {
        public int MapSRID { get; set; }

        private readonly IDictionary<int, Func<double, double, Point>> _toLatLon = new Dictionary<int, Func<double, double, Point>>();
        private readonly IDictionary<int, Func<double, double, Point>> _fromLatLon = new Dictionary<int, Func<double, double, Point>>();

        public MinimalTransformation()
        {
            _toLatLon[4326] = (x, y) => new Point(x, y);
            _fromLatLon[4326] = (x, y) => new Point(x, y);
            _toLatLon[3857] = SphericalMercator.ToLonLat;
            _fromLatLon[3857] = SphericalMercator.FromLonLat;
        }

        public IGeometry Transform(int fromSRID, int toSRID, IGeometry geometry)
        {
            Transform(geometry.AllVertices(), _toLatLon[fromSRID]);
            Transform(geometry.AllVertices(), _fromLatLon[toSRID]);
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

        public BoundingBox Transform(int fromSRID, int toSRID, BoundingBox boundingBox)
        {
            Transform(boundingBox.AllVertices(), _toLatLon[fromSRID]);
            Transform(boundingBox.AllVertices(), _fromLatLon[toSRID]);
            return boundingBox; // this method not have a return value
        }
    }
}
