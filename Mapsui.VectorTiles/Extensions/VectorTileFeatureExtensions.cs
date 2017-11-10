using Mapbox.Vector.Tile;
using Mapsui.Geometries;
using Mapsui.Logging;
using Mapsui.Providers;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.VectorTiles.Extensions
{
    public static class VectorTileFeatureExtensions
    {
        private static List<Point> Project(List<Coordinate> coords, int x, int y, int z, uint extent)
        {
            return coords.Select(coord => coord.ToPosition(x, y, z, extent)).ToList();
        }

        private static LineString CreateLineString(List<Point> pos)
        {
            return new LineString(pos);
        }

        private static IGeometry GetPointGeometry(List<Point> pointList)
        {
            IGeometry geom;
            if (pointList.Count == 1)
            {
                geom = new Point(pointList[0][0], pointList[0][1]);
            }
            else
            {
                var points = pointList.Select(p => new Point(p[0], p[1])).ToList();
                geom = new MultiPoint(points);
            }
            return geom;
        }

        private static List<LineString> GetLineStringList(List<List<Point>> pointList)
        {
            return pointList.Select(CreateLineString).ToList();
        }

        private static IGeometry GetLineGeometry(List<List<Point>> pointList)
        {
            IGeometry geom;

            if (pointList.Count == 1)
            {
                geom = new LineString(pointList[0]);
            }
            else
            {
                geom = new MultiLineString { LineStrings = GetLineStringList(pointList) };
            }
            return geom;
        }

        private static Polygon ToMapsui(List<List<Point>> lines)
        {
            var polygon = new Polygon();

            foreach (var innerring in lines)
            {
                var linearRing = new LinearRing(innerring);
                if (polygon.ExteriorRing.IsEmpty())
                    polygon.ExteriorRing = linearRing;
                else
                    polygon.InteriorRings.Add(linearRing);
            }

            return polygon;
        }

        private static IGeometry GetPolygonGeometry(List<List<List<Point>>> polygons)
        {
            IGeometry geom = null;

            if (polygons.Count == 1)
            {
                geom = ToMapsui(polygons[0]);
            }
            else if (polygons.Count > 1)
            {
                var mapsuiPolygons = new List<Polygon>();
                foreach (var mapsuiPolygon in polygons)
                {
                    mapsuiPolygons.Add(ToMapsui(mapsuiPolygon));
                }

                geom = new MultiPolygon(mapsuiPolygons);
            }
            return geom;
        }

        public static List<Point> ProjectPoints(List<List<Coordinate>> geometry, int x, int y, int z, uint extent)
        {
            var projectedCoords = new List<Point>();
            var coords = new List<Coordinate>();

            foreach (var g in geometry)
            {
                coords.Add(g[0]);
                projectedCoords = Project(coords, x, y, z, extent);
            }
            return projectedCoords;
        }

        public static List<List<Point>> ProjectLines(List<List<Coordinate>> geometry, int x, int y, int z, uint extent)
        {
            var pointList = new List<List<Point>>();
            foreach (var g in geometry)
            {
                var projectedCoords = Project(g, x, y, z, extent);
                pointList.Add(projectedCoords);
            }
            return pointList;
        }

        public static List<List<List<Point>>> ProjectPolygons(List<List<List<Coordinate>>> geometry, int x, int y, int z, uint extent)
        {
            var result = new List<List<List<Point>>>();
            foreach (var g in geometry)
            {
                var projectedCoords = ProjectLines(g, x, y, z, extent);
                result.Add(projectedCoords);
            }
            return result;
        }

        public static Feature ToMapsui(this VectorTileFeature vectortileFeature, int x, int y, int z)
        {
            IGeometry geom = null;

            switch (vectortileFeature.GeometryType)
            {
                case Tile.GeomType.Point:
                    var projectedPoints = ProjectPoints(vectortileFeature.Geometry, x, y, z, vectortileFeature.Extent);
                    geom = GetPointGeometry(projectedPoints);
                    break;
                case Tile.GeomType.LineString:
                    var projectedLines = ProjectLines(vectortileFeature.Geometry, x, y, z, vectortileFeature.Extent);
                    geom = GetLineGeometry(projectedLines);
                    break;
                case Tile.GeomType.Polygon:
                    var rings = ClassifyRings.Classify(vectortileFeature.Geometry);
                    var projectedPolygons = ProjectPolygons(rings, x, y, z, vectortileFeature.Extent);
                    geom = GetPolygonGeometry(projectedPolygons);
                    break;
                default:
                    Logger.Log(LogLevel.Warning, "Unkown vector tile type");
                    break;
            }

            var result = new Feature
            {
                Geometry = geom,
                ["id"] = vectortileFeature.Id,
            };

            foreach (var item in vectortileFeature.Attributes)
            {
                result[item.Key] = item.Value;
            }

            return result;
        }
    }
}