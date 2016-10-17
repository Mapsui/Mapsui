using BruTile.Predefined;
using Mapsui.Layers;

namespace Mapsui.Samples.Common.Maps
{
    public static class BingSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.BingAerial)) {Name = "Bing Aerial"});
            return map;
        }
    }
}