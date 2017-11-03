using System.Linq;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using Mapsui.Geometries;
using LineString = GeoJSON.Net.Geometry.LineString;
using MultiPolygon = GeoJSON.Net.Geometry.MultiPolygon;
using Polygon = GeoJSON.Net.Geometry.Polygon;

namespace Mapsui.VectorTiles.Extensions
{
    static class GeoJSONObjectExtensions
    {
        public static Geometry ToMapsui(this IGeometryObject geoJsonObject)
        {
            if (geoJsonObject.Type == GeoJSONObjectType.Polygon)
            {
                return ToMapsuiPolygon((Polygon) geoJsonObject);
            }
            if (geoJsonObject.Type == GeoJSONObjectType.MultiPolygon)
            {
                return ToMapsuiMultiPolygon((MultiPolygon)geoJsonObject);
            }
            return null;
        }
        
        private static Geometry ToMapsuiMultiPolygon(MultiPolygon multiPolygon)
        {
            var mapsuiPolygon = new Geometries.MultiPolygon();
            foreach (var polygon in multiPolygon.Coordinates)
            {
                mapsuiPolygon.Polygons.Add((Geometries.Polygon)ToMapsuiPolygon(polygon));
            }
            return mapsuiPolygon;
        }

        private static Geometry ToMapsuiPolygon(Polygon polygon)
        {
            var mapsuiPolygon = new Geometries.Polygon();

            foreach (var lineString in polygon.Coordinates)
            {
                if (lineString.Coordinates.Count == 0) continue;

                if (mapsuiPolygon.IsEmpty())
                {
                    mapsuiPolygon.ExteriorRing = ToLinearRing(lineString);
                }
                else
                {
                    mapsuiPolygon.InteriorRings.Add(ToLinearRing(lineString));
                }
            }
            return mapsuiPolygon;
        }

        private static LinearRing ToLinearRing(LineString lineString)
        {
            return new LinearRing(lineString.Coordinates.Select(position =>
            {
                var sphericalPosition = Projection.SphericalMercator.FromLonLat(position.Longitude, position.Latitude);
                return new[] { sphericalPosition.X, sphericalPosition.Y };
            }));
        }
    }
}
