using System;
using System.Diagnostics.CodeAnalysis;

namespace Mapsui.Projections;

public enum CrsType
{
    Epgs,
    EsriString,
    Proj4String
}

public static class CrsHelper
{
    public const string EpsgPrefix = "EPSG:";
    public const string EsriStringPrefix = "ESRISTRING:";
    public const string Proj4StringPrefix = "PROJ4STRING:";

    public static string? ToStandardizedCRS(string? crs)
    {
        if (crs == null) return null;
        if (string.IsNullOrWhiteSpace(crs)) return crs.Trim();

        var crsType = GetCrsType(crs);

        if (crsType == CrsType.Epgs) return EpsgPrefix + crs[EpsgPrefix.Length..];
        if (crsType == CrsType.EsriString) return EsriStringPrefix + crs[EsriStringPrefix.Length..];
        if (crsType == CrsType.Proj4String) return Proj4StringPrefix + crs[Proj4StringPrefix.Length..];

        throw new Exception($"crs is not recognized as a projection string: '{crs}'");
    }

    public static int ToEpsgCode(string crs)
    {
        return int.Parse(crs[EpsgPrefix.Length..]);
    }

    public static CrsType GetCrsType(string crs)
    {
        ArgumentNullException.ThrowIfNull(crs);

        if (crs.StartsWith(EpsgPrefix)) return CrsType.Epgs;
        if (crs.StartsWith(EsriStringPrefix)) return CrsType.EsriString;
        if (crs.StartsWith(Proj4StringPrefix)) return CrsType.Proj4String;
        throw new Exception($"crs not recognized: '{crs}'");
    }

    public static bool IsCrsProvided([NotNullWhen(true)] string? fromCRS, [NotNullWhen(true)] string? toCRS)
    {
        return !string.IsNullOrEmpty(fromCRS) && !string.IsNullOrEmpty(toCRS);
    }

    public static bool IsProjectionNeeded(string? fromCRS, string? toCRS)
    {
        return !fromCRS?.Equals(toCRS) == true;
    }
}
