using BruTile;
using BruTile.Predefined;
using Mapsui.Layers;
using SQLite;

namespace Mapsui.Samples.Common.Maps
{
    public static class MbTilesSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            const string path = @".\MbTiles\test.mbtiles";
            var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
            map.Layers.Add(new TileLayer(mbTilesTileSource));
            return map;
        }
    }
}