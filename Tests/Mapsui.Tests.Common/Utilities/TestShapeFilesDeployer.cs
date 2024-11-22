using System;
using System.IO;
using System.Reflection;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Tests.Common.Maps;

namespace Mapsui.Tests.Common.Utilities;

public static class TestShapeFilesDeployer
{
    public static string ShapeFilesLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

    public static void CopyEmbeddedResourceToFile(string shapefile)
    {
        shapefile = Path.GetFileNameWithoutExtension(shapefile);
        var assembly = typeof(ShapefileZoomSample).GetTypeInfo().Assembly;
        assembly.CopyEmbeddedResourceToFile("Mapsui.Tests.Common.Resources.Shapefiles.", ShapeFilesLocation, shapefile + ".dbf");
        assembly.CopyEmbeddedResourceToFile("Mapsui.Tests.Common.Resources.Shapefiles.", ShapeFilesLocation, shapefile + ".prj");
        assembly.CopyEmbeddedResourceToFile("Mapsui.Tests.Common.Resources.Shapefiles.", ShapeFilesLocation, shapefile + ".shp");
        assembly.CopyEmbeddedResourceToFile("Mapsui.Tests.Common.Resources.Shapefiles.", ShapeFilesLocation, shapefile + ".shx");
        assembly.CopyEmbeddedResourceToFile("Mapsui.Tests.Common.Resources.Shapefiles.", ShapeFilesLocation, shapefile + ".shp.sidx");
    }
}
