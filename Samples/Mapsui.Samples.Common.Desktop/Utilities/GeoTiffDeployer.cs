using System;
using System.IO;
using System.Reflection;
using Mapsui.Samples.Common.Extensions;

namespace Mapsui.Samples.Common.Desktop.Utilities
{
    public static class GeoTiffDeployer
    {
        public static string GeoTiffLocation { get; set; } =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

        public static void CopyEmbeddedResourceToFile(string geoTif)
        {
            geoTif = Path.GetFileNameWithoutExtension(geoTif);
            var assembly = typeof(GeoTiffSample).GetTypeInfo().Assembly;
            assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.Desktop.Images.", GeoTiffLocation, geoTif + ".tfw");
            assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.Desktop.Images.", GeoTiffLocation, geoTif + ".tif");
        }
    }
}
