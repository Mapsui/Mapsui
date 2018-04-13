using BruTile.Predefined;
using Mapsui.Fetcher;
using Mapsui.Layers;

namespace Mapsui.Samples.Common.Maps
{
    public static class BingSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            var apiKey = "Enter your api key here"; // Contact Microsoft about how to use this
            map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.BingAerial, apiKey), 
                fetchStrategy: new FetchStrategy()) // FetchStrategy get tiles from higher levels in advance
            {
                Name = "Bing Aerial",
                
            });
            map.NavigateTo(map.Resolutions[14]);
            map.NavigateTo(1059114.80157058, 5179580.75916194);
            return map;
        }
    }
}