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
            map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.BingAerial)) { Name = "Bing Aerial" });
            const string path = @".\MbTiles\torrejon-de-ardoz.mbtiles";
            var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
            map.Layers.Add(new TileLayer(mbTilesTileSource));
            return map;
        }
    }
}