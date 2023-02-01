using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps.DataFormats;
using System;
using System.IO;
using System.Reflection;

namespace Mapsui.Samples.Common.Utilities;

public static class GeoJsonDeployer
{
    public static string GeoJsonLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

    public static void CopyEmbeddedResourceToFile(string geoJson)
    {
        geoJson = Path.GetFileNameWithoutExtension(geoJson);
        var assembly = typeof(GeoJsonSample).GetTypeInfo().Assembly;
        assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.GeoJson.", GeoJsonLocation, geoJson + ".geojson");
    }
}
