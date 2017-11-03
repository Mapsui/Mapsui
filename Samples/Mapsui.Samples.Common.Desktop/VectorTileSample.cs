using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Layers;
using Mapsui.VectorTiles;

namespace Mapsui.Samples.Common.Desktop
{
    public static class VectorTileSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            // A regular tile layer with a tile source that rasterizes vector tiles
            map.Layers.Add(new TileLayer(CreateVectorTileTileSource()) { Name = "Rasterized Vector Tiles"});
            // A vector tile layer that parses the binary tiles returned by the regular tile layer
            map.Layers.Add(new VectorTileLayer(CreateRegularTileSource()) { Name = " Pure Vector Tiles", Enabled = false});
            return map;
        }

        private static HttpVectorTileSource CreateVectorTileTileSource()
        {
            return new HttpVectorTileSource(
                new GlobalSphericalMercator(),
                "https://free.tilehosting.com/data/v3/{z}/{x}/{y}.pbf.pict?key=RiS4gsgZPZqe",
                name: "vector tile");
        }

        private static HttpTileSource CreateRegularTileSource()
        {
            return new HttpTileSource(new GlobalSphericalMercator(),
                "https://free.tilehosting.com/data/v3/{z}/{x}/{y}.pbf.pict?key=RiS4gsgZPZqe",
                tileFetcher: VectorTileLayer.FetchTile);
        }
    }
}
