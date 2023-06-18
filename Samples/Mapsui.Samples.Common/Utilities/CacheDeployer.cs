using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps.DataFormats;
using System;
using System.IO;
using System.Reflection;

namespace Mapsui.Samples.Common.Utilities;

public static class CacheDeployer
{
    public static string CacheLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

    public static void CopyEmbeddedResourceToFile(string cache)
    {
        cache = Path.GetFileNameWithoutExtension(cache);
        var assembly = typeof(WmsProjectionTilingSample).GetTypeInfo().Assembly;
        assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.Cache.", CacheLocation, cache + ".sqlite");
    }
}
