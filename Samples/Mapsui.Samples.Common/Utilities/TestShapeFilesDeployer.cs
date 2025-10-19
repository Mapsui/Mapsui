using System;
using System.IO;
using System.Reflection;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps.Tests;

namespace Mapsui.Samples.Common.Utilities;

public static class TestShapeFilesDeployer
{
    public static string ShapeFilesLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

    public static void CopyEmbeddedResourceToFile(string shapefile)
    {
        shapefile = Path.GetFileNameWithoutExtension(shapefile);
        var assembly = typeof(ShapefileZoomSample).GetTypeInfo().Assembly;
        assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.Shapefiles.", ShapeFilesLocation, shapefile + ".dbf");
        assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.Shapefiles.", ShapeFilesLocation, shapefile + ".prj");
        assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.Shapefiles.", ShapeFilesLocation, shapefile + ".shp");
        assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.Shapefiles.", ShapeFilesLocation, shapefile + ".shx");
        assembly.CopyEmbeddedResourceToFile("Mapsui.Samples.Common.GeoData.Shapefiles.", ShapeFilesLocation, shapefile + ".shp.sidx");
    }
}
