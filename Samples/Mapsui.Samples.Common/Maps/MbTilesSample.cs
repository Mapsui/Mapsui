using System.IO;
using BruTile.MbTiles;
using Mapsui.Layers;
using SQLite;

namespace Mapsui.Samples.Common.Maps
{
    public static class MbTilesSample
    {
        // This is a hack used for iOS/Android deployment
        // For Mac it should be @"." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "MbTiles";
        //public static string MbTilesLocation { get; set; } = @"." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + ".." + Path.DirectorySeparatorChar + "MbTiles";
        public static string MbTilesLocation { get; set; } = @"." + Path.DirectorySeparatorChar + "MbTiles";

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateMbTilesLayer(Path.Combine(MbTilesLocation, "world.mbtiles")));
            return map;
        }

        public static TileLayer CreateMbTilesLayer(string path)
        {
            var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
            var mbTilesLayer = new TileLayer(mbTilesTileSource);
            return mbTilesLayer;
        }
    }
}