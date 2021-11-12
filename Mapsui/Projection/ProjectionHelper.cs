using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
using Mapsui.Projection;

namespace Mapsui.Utilities
{
    public static class ProjectionHelper
    {
        public const string EpsgPrefix = "EPSG:";
        public const string EsriStringPrefix = "ESRISTRING:";
        public const string Proj4StringPrefix = "PROJ4STRING:";


        private static bool IsTransformationSupported(IGeometryTransformation geometryTransformation, string fromCRS, string toCRS)
        {
            return geometryTransformation.IsProjectionSupported(fromCRS, toCRS) == true;
        }

        public static BoundingBox? Transform(BoundingBox? extent,
            IGeometryTransformation geometryTransformation, string fromCRS, string toCRS)
        {
            if (extent == null) return null;

            if (!CrsHelper.IsTransformationNeeded(fromCRS, toCRS)) return extent;

            if (!CrsHelper.IsCrsProvided(fromCRS, toCRS))
                throw new NotSupportedException($"CRS is not provided. From CRS: {fromCRS}. To CRS {toCRS}");

            if (!IsTransformationSupported(geometryTransformation, fromCRS, toCRS))
                throw new NotSupportedException($"Transformation is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

            var copiedExtent = extent.Copy();
            geometryTransformation.Transform(fromCRS, toCRS, copiedExtent);
            return copiedExtent;
        }

        public static IEnumerable<GeometryFeature>? Transform(IEnumerable<GeometryFeature>? features,
            IGeometryTransformation geometryTransformation, string fromCRS, string toCRS)
        {
            if (features == null) return null;

            if (!CrsHelper.IsTransformationNeeded(fromCRS, toCRS)) return features;

            if (!CrsHelper.IsCrsProvided(fromCRS, toCRS))
                throw new NotSupportedException($"CRS is not provided. From CRS: {fromCRS}. To CRS {toCRS}");

            if (!IsTransformationSupported(geometryTransformation, fromCRS, toCRS))
                throw new NotSupportedException($"Transformation is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

            var copiedFeatures = features.Copy().ToList();
            foreach (var copiedFeature in copiedFeatures)
            {
                geometryTransformation.Transform(fromCRS, toCRS, copiedFeature.Geometry);
            }
            return copiedFeatures;
        }
    }
}
