using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps.DataFormats;
using System;
using System.IO;
using System.Reflection;

namespace Mapsui.Samples.Common.Utilities;

public static class GeoTiffDeployer
{
    public static string GeoTiffLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

    public static void CopyEmbeddedResourceToFile(string geoTif)
    {
        geoTif = Path.GetFileNameWithoutExtension(geoTif);
        var assembly = typeof(GeoTiffSample).GetTypeInfo().Assembly;
        assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.GeoTiff.", GeoTiffLocation, geoTif + ".tfw");
        assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.GeoTiff.", GeoTiffLocation, geoTif + ".tif");
    }
}
