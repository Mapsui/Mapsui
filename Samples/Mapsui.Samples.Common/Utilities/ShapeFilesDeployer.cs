using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps.DataFormats;
using System;
using System.IO;
using System.Reflection;

namespace Mapsui.Samples.Common.Utilities;

public static class ShapeFilesDeployer
{
    private static object _lock = new();
    public static string ShapeFilesLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

    public static void CopyEmbeddedResourceToFile(string shapefile)
    {
        lock (_lock)
        {
            shapefile = Path.GetFileNameWithoutExtension(shapefile);
            var assembly = typeof(ShapefileSample).GetTypeInfo().Assembly;
            assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.World.", ShapeFilesLocation, shapefile + ".dbf");
            assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.World.", ShapeFilesLocation, shapefile + ".prj");
            assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.World.", ShapeFilesLocation, shapefile + ".shp");
            assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.World.", ShapeFilesLocation, shapefile + ".shx");
            assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.World.", ShapeFilesLocation, shapefile + ".shp.sidx");
        }
    }
}
