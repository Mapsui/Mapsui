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
    }
}
