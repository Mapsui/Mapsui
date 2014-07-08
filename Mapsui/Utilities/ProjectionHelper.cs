using System;
using Mapsui.Projection;

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
        
        public static string ToStandardizedCRS(string CRS)
        {
            if (CRS == null) return null;
            if (string.IsNullOrWhiteSpace(CRS)) return CRS.Trim();

            var crsType = GetCrsType(CRS);

            if (crsType == CrsType.Epgs) return EpsgPrefix + CRS.Substring(EpsgPrefix.Length);
            if (crsType == CrsType.EsriString) return EsriStringPrefix + CRS.Substring(EsriStringPrefix.Length);
            if (crsType == CrsType.Proj4String) return Proj4StringPrefix + CRS.Substring(Proj4StringPrefix.Length);

            throw new Exception(string.Format("CRS is not recognized as a projection string: '{0}'", CRS));
        }

        public static int ToEpsgCode(string CRS)
        {
            return int.Parse(CRS.Substring(EpsgPrefix.Length));
        }

        private static void ThrowIfNotInt(string CRS)
        {
            int result; // The result is not relevant here, just that it actually is an int.
            if (!int.TryParse(CRS, out result))
            {
                throw new Exception(string.Format("CRS is not recognized as a projection string: '{0}'", CRS));
            }
        }

        public static CrsType GetCrsType(string CRS)
        {
            if (CRS.StartsWith(EpsgPrefix)) return CrsType.Epgs;
            if (CRS.StartsWith(EsriStringPrefix)) return CrsType.EsriString;
            if (CRS.StartsWith(Proj4StringPrefix)) return CrsType.Proj4String;
            throw new Exception(string.Format("CRS not recognized: '{0}'", CRS));
        }

        public static bool NeedsTransform(ITransformation transformation, string layerCRS, string sourceCRS)
        {
            return (transformation != null && !string.IsNullOrWhiteSpace(layerCRS) && !string.IsNullOrWhiteSpace(sourceCRS) && layerCRS != sourceCRS);
        }
    }
}
