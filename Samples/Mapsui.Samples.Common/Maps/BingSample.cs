using BruTile.Predefined;
using Mapsui.Layers;

namespace Mapsui.Samples.Common.Maps
{
    public static class BingSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            var apiKey = "Enter your api key here"; // Contact Microsoft about how to use this
            map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.BingAerial, apiKey))
            {
                Name = "Bing Aerial"
            });
            map.NavigateTo(4.7773142678234581);
            map.NavigateTo(1059114.80157058, 5179580.75916194);
            return map;
        }
    }
}