using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Projection;

namespace Mapsui.Utilities
{
    public static class ProjectionHelper
    {
        public const string EpsgPrefix = "EPSG:";

        public static MRect? Project(MRect? extent, IProjection projection, string fromCRS, string toCRS)
        {
            if (extent == null) return null;

            if (!CrsHelper.IsProjectionNeeded(fromCRS, toCRS)) return extent;

            if (!CrsHelper.IsCrsProvided(fromCRS, toCRS))
                throw new NotSupportedException($"CRS is not provided. From CRS: {fromCRS}. To CRS {toCRS}");

            if (!projection.IsProjectionSupported(fromCRS, toCRS))
                throw new NotSupportedException($"Projection is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

            var copiedExtent = new MRect(extent);
            projection.Project(fromCRS, toCRS, copiedExtent);
            return copiedExtent;
        }

        public static IEnumerable<IFeature>? Project(
            IEnumerable<IFeature>? features, IProjection projection, string fromCRS, string toCRS)
        {
            if (features == null) return null;

            if (!CrsHelper.IsProjectionNeeded(fromCRS, toCRS)) return features;

            if (!CrsHelper.IsCrsProvided(fromCRS, toCRS))
                throw new NotSupportedException($"CRS is not provided. From CRS: {fromCRS}. To CRS {toCRS}");

            if (!projection.IsProjectionSupported(fromCRS, toCRS))
                throw new NotSupportedException($"Projection is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

            var copiedFeatures = features.Copy().ToList();
            projection.Project(fromCRS, toCRS, copiedFeatures);
            return copiedFeatures;
        }
    }
}
