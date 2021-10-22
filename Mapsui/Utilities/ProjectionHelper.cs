using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Extensions;
using Mapsui.Geometries;
using Mapsui.Projection;
using Mapsui.Providers;

namespace Mapsui.Utilities
{
    public enum CrsType
    {
        Epgs,
        EsriString,
        Proj4String
    }

    public static class ProjectionHelper
    {
        public const string EpsgPrefix = "EPSG:";
        public const string EsriStringPrefix = "ESRISTRING:";
        public const string Proj4StringPrefix = "PROJ4STRING:";
        
        public static string ToStandardizedCRS(string crs)
        {
            if (crs == null) return null;
            if (string.IsNullOrWhiteSpace(crs)) return crs.Trim();

            var crsType = GetCrsType(crs);

            if (crsType == CrsType.Epgs) return EpsgPrefix + crs.Substring(EpsgPrefix.Length);
            if (crsType == CrsType.EsriString) return EsriStringPrefix + crs.Substring(EsriStringPrefix.Length);
            if (crsType == CrsType.Proj4String) return Proj4StringPrefix + crs.Substring(Proj4StringPrefix.Length);

            throw new Exception($"crs is not recognized as a projection string: '{crs}'");
        }

        public static int ToEpsgCode(string crs)
        {
            return int.Parse(crs.Substring(EpsgPrefix.Length));
        }

        public static CrsType GetCrsType(string crs)
        {
            if (crs.StartsWith(EpsgPrefix)) return CrsType.Epgs;
            if (crs.StartsWith(EsriStringPrefix)) return CrsType.EsriString;
            if (crs.StartsWith(Proj4StringPrefix)) return CrsType.Proj4String;
            throw new Exception($"crs not recognized: '{crs}'");
        }

        private static bool IsCrsProvided(string fromCRS, string toCRS)
        {
            return !string.IsNullOrEmpty(fromCRS) && !string.IsNullOrEmpty(toCRS);
        }

        private static bool IsTransformationNeeded(string fromCRS, string toCRS)
        {
           return !fromCRS?.Equals(toCRS) == true;
        }

        private static bool IsTransformationSupported(IGeometryTransformation geometryTransformation, string fromCRS, string toCRS)
        {
            return geometryTransformation.IsProjectionSupported(fromCRS, toCRS) == true;
        }

        public static BoundingBox Transform(BoundingBox extent,
            IGeometryTransformation geometryTransformation, string fromCRS, string toCRS)
        {
            if (extent == null) return null;

            if (!IsTransformationNeeded(fromCRS, toCRS)) return extent;

            if (!IsCrsProvided(fromCRS, toCRS))
                throw new NotSupportedException($"CRS is not provided. From CRS: {fromCRS}. To CRS {toCRS}");

            if (!IsTransformationSupported(geometryTransformation, fromCRS, toCRS))
                throw new NotSupportedException($"Transformation is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

            var copiedExtent = extent.Copy();
            geometryTransformation.Transform(fromCRS, toCRS, copiedExtent);
            return copiedExtent;
        }

        public static IEnumerable<IGeometryFeature> Transform(IEnumerable<IGeometryFeature> features,
            IGeometryTransformation geometryTransformation, string fromCRS, string toCRS)
        {
            if (features == null) return null;

            if (!IsTransformationNeeded(fromCRS, toCRS)) return features;

            if (!IsCrsProvided(fromCRS, toCRS))
                throw new NotSupportedException($"CRS is not provided. From CRS: {fromCRS}. To CRS {toCRS}");

            if (!IsTransformationSupported(geometryTransformation, fromCRS, toCRS))
                throw new NotSupportedException($"Transformation is not supported. From CRS: {fromCRS}. To CRS {toCRS}");

            var copiedFeatures = features.Copy().ToList();
            foreach (var copiedFeature in copiedFeatures)
            {
                if (copiedFeature.Geometry is Raster) continue;
                var copiedGeometry = copiedFeature.Geometry.Copy();
                geometryTransformation.Transform(fromCRS, toCRS, copiedGeometry);
                copiedFeature.Geometry = copiedGeometry;
            }
            return copiedFeatures;
        }
    }
}
