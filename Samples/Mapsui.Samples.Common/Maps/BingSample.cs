using BruTile.Predefined;
using Mapsui.Fetcher;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps
{
    public class BingSample : ISample
    {
        public string Name => "3 Virtual Earth";
        public string Category => "Demo";
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap(KnownTileSource source = KnownTileSource.BingAerial)
        {
            var map = new Map();
            var apiKey = "Enter your api key here"; // Contact Microsoft about how to use this
            map.Layers.Add(new TileLayer(KnownTileSources.Create(source, apiKey), 
                fetchStrategy: new FetchStrategy()) // FetchStrategy get tiles from higher levels in advance
            {
                Name = "Bing Aerial",
                
            });
            map.Home = n => n.NavigateTo(new Point(1059114.80157058, 5179580.75916194), map.Resolutions[14]);
            return map;
        }
    }
}