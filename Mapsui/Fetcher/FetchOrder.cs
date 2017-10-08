using BruTile;

namespace Mapsui.Fetcher
{
    class FetchOrder
    {
        public TileInfo TileInfo { get; set; }
        public ITileSource TileSource { get; set; }
    }
}
