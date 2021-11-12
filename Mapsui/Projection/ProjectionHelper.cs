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

        public static BoundingBox? Project(BoundingBox? extent,
            IGeometryProjection geometryProjection, string fromCRS, string toCRS)
        {
            if (extent == null) return null;

            if (!CrsHelper.IsProjectionNeeded(fromCRS, toCRS)) return extent;

            if (!CrsHelper.IsCrsProvided(fromCRS, toCRS))
                throw new NotSupportedException($"CRS is not provided. From CRS: {fromCRS}. To CRS {toCRS}");

            if (!geometryProjection.IsProjectionSupported(fromCRS, toCRS))
                throw new NotSupportedException($"Projection is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

            var copiedExtent = extent.Copy();
            geometryProjection.Project(fromCRS, toCRS, copiedExtent);
            return copiedExtent;
        }

        public static IEnumerable<GeometryFeature>? Project(IEnumerable<GeometryFeature>? features,
            IGeometryProjection geometryProjection, string fromCRS, string toCRS)
        {
            if (features == null) return null;

            if (!CrsHelper.IsProjectionNeeded(fromCRS, toCRS)) return features;

            if (!CrsHelper.IsCrsProvided(fromCRS, toCRS))
                throw new NotSupportedException($"CRS is not provided. From CRS: {fromCRS}. To CRS {toCRS}");

            if (!geometryProjection.IsProjectionSupported(fromCRS, toCRS))
                throw new NotSupportedException($"Projection is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

            var copiedFeatures = features.Copy().ToList();
            foreach (var copiedFeature in copiedFeatures)
            {
                geometryProjection.Project(fromCRS, toCRS, copiedFeature.Geometry);
            }
            return copiedFeatures;
        }
    }
}
