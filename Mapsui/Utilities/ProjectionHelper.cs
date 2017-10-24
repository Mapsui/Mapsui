using System;
using System.Collections.Generic;
using System.Linq;
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

        public static bool NeedsTransform(ITransformation transformation, string fromCRS, string toCRS)
        {
            return transformation != null && !string.IsNullOrWhiteSpace(fromCRS) && !string.IsNullOrWhiteSpace(toCRS) && fromCRS != toCRS;
        }

        public static BoundingBox GetTransformedBoundingBox(ITransformation transformatiom, BoundingBox extent, string fromCRS, string toCRS)
        {
            if (!IsTransformationNeeded(fromCRS, toCRS))
                return extent;

            if (!IsProjectionInfoAvailable(transformatiom, fromCRS, toCRS))
                return null;

            if (IsTransformationSupported(transformatiom, fromCRS, toCRS))
                return transformatiom.Transform(fromCRS, toCRS, extent);

            return null;

        }

        private static bool IsProjectionInfoAvailable(ITransformation transformation, string fromCRS, string toCRS)
        {
            return !string.IsNullOrEmpty(fromCRS) && !string.IsNullOrEmpty(toCRS) && transformation != null;
        }

        private static bool IsTransformationNeeded(string fromCRS, string toCRS)
        {
           return !fromCRS.Equals(toCRS);
        }

        private static bool IsTransformationSupported(ITransformation transformation, string fromCRS, string toCRS)
        {
            return transformation.IsProjectionSupported(fromCRS, toCRS) == true;
        }

        public static BoundingBox Transform(BoundingBox extent,
            ITransformation transformation, string fromCRS, string toCRS)
        {
            if (extent == null) return null;
            if (NeedsTransform(transformation, fromCRS, toCRS))
                return transformation.Transform(fromCRS, toCRS, extent.Copy());
            return extent;
        }

        public static IEnumerable<IFeature> Transform(IEnumerable<IFeature> features,
            ITransformation transformation, string fromCRS, string toCRS)
        {
            if (!NeedsTransform(transformation, fromCRS, toCRS)) return features;

            var copiedFeatures = features.Copy().ToList();
            foreach (var feature in copiedFeatures)
            {
                if (feature.Geometry is Raster) continue;
                feature.Geometry = transformation.Transform(fromCRS, toCRS, feature.Geometry.Copy());
            }
            return copiedFeatures;
        }
    }
}
