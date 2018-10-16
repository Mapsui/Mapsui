using BruTile.Predefined;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps
{
    public class BingSample : IDemoSample
    {
        public string Name => "1.2 Virtual Earth";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            var apiKey = "Enter your api key here"; // Contact Microsoft about how to use this
            map.Layers.Add(new TileLayer(KnownTileSources.Create(KnownTileSource.BingAerial, apiKey), 
                fetchStrategy: new FetchStrategy()) // FetchStrategy get tiles from higher levels in advance
            {
                Name = "Bing Aerial",
                
            });
            map.Home = n => n.NavigateTo(new Point(1059114.80157058, 5179580.75916194), map.Resolutions[14]);
            return map;
        }
    }
}