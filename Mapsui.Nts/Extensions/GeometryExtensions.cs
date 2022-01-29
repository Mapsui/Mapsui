using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace Mapsui.Nts.Extensions
{
    public static class GeometryExtensions
    {
        //!!! todo: Remove method and replace with direct call to Geometry.Coordinates
        public static IEnumerable<Coordinate> AllVertices(this Geometry? geometry)
        {
            if (geometry == null) return Array.Empty<Coordinate>();

            return geometry.Coordinates;
        }

        //!!! todo: Remove method and replace with direct call to Geometry.Coordinates
        private static IEnumerable<Coordinate> AllVertices(IEnumerable<Geometry> collection)
        {
            if (collection == null) throw new ArgumentNullException(nameof(collection));

            foreach (var geometry in collection)
                foreach (var point in geometry.AllVertices())
                    yield return point;
        }

        //!!! todo: Rename to ToEnvelope after everything compiles
        public static Envelope BoundingBox(this Geometry geometry)
        {
            return geometry.EnvelopeInternal;
        }

        public static GeometryFeature ToFeature(this Geometry geometry)
        {
            return new GeometryFeature(geometry);
        }

        public static IEnumerable<GeometryFeature> ToFeatures(this IEnumerable<Geometry> geometries)
        {
            return geometries.Select(g => new GeometryFeature(g)).ToList();
        }
    }
}