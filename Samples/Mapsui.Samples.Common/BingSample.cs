using BruTile.Predefined;
using Mapsui.Layers;

namespace Mapsui.Samples.Common
{
    public static class BingSample
    {
        public static ILayer CreateLayer()
        {
            return new TileLayer(KnownTileSources.Create(KnownTileSource.BingAerial))
            {
                Name = "Bing Aerial"
            };
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            return map;
        }
    }
}
