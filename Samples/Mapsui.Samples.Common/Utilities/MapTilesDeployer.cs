using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps.DataFormats;
using System;
using System.IO;
using System.Reflection;

namespace Mapsui.Samples.Common.Utilities;

public static class MapTilesDeployer
{
    public static string MapTileLocation { get; set; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

    public static void CopyEmbeddedResourceToFile(string mapTile)
    {
        var assembly = typeof(MapTilerSample).GetTypeInfo().Assembly;
        CopyTile(assembly, mapTile, @"_0._0", @$"0{Path.DirectorySeparatorChar}0", "0");
        CopyTile(assembly, mapTile, @"_1._0", @$"1{Path.DirectorySeparatorChar}0", "0");
        CopyTile(assembly, mapTile, @"_1._0", @$"1{Path.DirectorySeparatorChar}0", "1");
        CopyTile(assembly, mapTile, @"_1._1", @$"1{Path.DirectorySeparatorChar}1", "0");
        CopyTile(assembly, mapTile, @"_1._1", @$"1{Path.DirectorySeparatorChar}1", "1");
    }

    private static void CopyTile(Assembly assembly, string mapTile, string resourceFolder, string folder, string tile)
    {
        assembly.CopyEmbeddedResourceToFile(@$"Mapsui.Samples.Common.GeoData.{mapTile}.{resourceFolder}.", MapTileLocation + @$"{Path.DirectorySeparatorChar}{mapTile}{Path.DirectorySeparatorChar}{folder}{Path.DirectorySeparatorChar}", $"{tile}.png");
    }
}
