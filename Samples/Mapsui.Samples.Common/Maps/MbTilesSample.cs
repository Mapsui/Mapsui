using System.IO;
using BruTile.MbTiles;
using Mapsui.Layers;
using Mapsui.UI;
using SQLite;

namespace Mapsui.Samples.Common.Maps
{
    public class MbTilesSample : ISample
    {
        // This is a hack used for iOS/Android deployment
        public static string MbTilesLocation { get; set; } = @"." + Path.DirectorySeparatorChar + "MbTiles";

        public string Name => "1 MbTiles";
        public string Category => "Data";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateMbTilesLayer(Path.Combine(MbTilesLocation, "world.mbtiles"), "regular"));
            return map;
        }

        public static TileLayer CreateMbTilesLayer(string path, string name)
        {
            var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
            var mbTilesLayer = new TileLayer(mbTilesTileSource) { Name = name};
            return mbTilesLayer;
        }
    }
}