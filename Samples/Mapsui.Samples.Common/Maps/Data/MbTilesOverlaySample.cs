using System.IO;
using System.Threading.Tasks;
using BruTile.MbTiles;
using BruTile.Predefined;
using Mapsui.Tiling.Layers;
using Mapsui.UI;
using SQLite;

namespace Mapsui.Samples.Common.Maps
{
    public class MbTilesOverlaySample : ISample
    {
        public string Name => "2 MbTiles Overlay";
        public string Category => "Data";

        public Task<Map> CreateMapAsync()
        {
            var map = new Map();
            map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.BingAerial, persistentCache: BingArial.DefaultCache)) { Name = "Bing Aerial" });
            map.Layers.Add(CreateMbTilesLayer(Path.Combine(MbTilesSample.MbTilesLocation, "torrejon-de-ardoz.mbtiles")));
            return Task.FromResult(map);
        }
        public static TileLayer CreateMbTilesLayer(string path)
        {
            var mbTilesTileSource = new MbTilesTileSource(new SQLiteConnectionString(path, true));
            var mbTilesLayer = new TileLayer(mbTilesTileSource);
            return mbTilesLayer;
        }
    }
}