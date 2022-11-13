using System;
using System.IO;
using System.Reflection;
using Mapsui.Samples.Common.Extensions;
using Mapsui.Samples.Common.Maps;

namespace Mapsui.Samples.Common.Desktop.Utilities
{
    public static class MapTilesDeployer
    {
        public static string MapTileLocation { get; set; } =  Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Mapsui.Samples");

        public static void CopyEmbeddedResourceToFile(string mapTile)
        {
            var assembly = typeof(ShapefileSample).GetTypeInfo().Assembly;
            CopyTile(assembly, mapTile, @"0\0", "0");
            CopyTile(assembly, mapTile, @"1\0", "0");
            CopyTile(assembly, mapTile, @"1\0", "1");
            CopyTile(assembly, mapTile, @"1\1", "0");
            CopyTile(assembly, mapTile, @"1\1", "0");
        }

        private static void CopyTile(Assembly assembly, string mapTile, string folder, string tile)
        {
            assembly.CopyEmbeddedResourceToFile(@$"Mapsui.Samples.Common.Desktop.GeoData.World.{mapTile}.{folder.Replace('\\', '.')}.", MapTileLocation + @$"{mapTile}\{folder}\", "{tile}.png");
        }
    }
}
