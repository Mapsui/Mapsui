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
        
        public static string ToStandardizedCRS(string crs)
        {
            if (crs == null) return null;
            if (string.IsNullOrWhiteSpace(crs)) return crs.Trim();

            var crsType = GetCrsType(crs);

            if (crsType == CrsType.Epgs) return EpsgPrefix + crs.Substring(EpsgPrefix.Length);
            if (crsType == CrsType.EsriString) return EsriStringPrefix + crs.Substring(EsriStringPrefix.Length);
            if (crsType == CrsType.Proj4String) return Proj4StringPrefix + crs.Substring(Proj4StringPrefix.Length);

            throw new Exception(string.Format("crs is not recognized as a projection string: '{0}'", crs));
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
            throw new Exception(string.Format("crs not recognized: '{0}'", crs));
        }

        public static bool NeedsTransform(ITransformation transformation, string fromCRS, string toCRS)
        {
            return (transformation != null && !string.IsNullOrWhiteSpace(fromCRS) && !string.IsNullOrWhiteSpace(toCRS) && fromCRS != toCRS);
        }
    }
}
