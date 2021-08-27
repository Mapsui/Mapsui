using System.Linq;
using System.Net.Http;
using BruTile.Wmts;
using Mapsui.Layers;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps
{
    public class WmtsSample : ISample
    {
        public string Name => "3 WMTS";
        public string Category => "Data";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            map.Layers.Add(GeodanOfficesSample.CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            var url = "http://geodata.nationaalgeoregister.nl/wmts/top10nl?VERSION=1.0.0&request=GetCapabilities";

            using (var httpClient = new HttpClient())
            using (var response = httpClient.GetStreamAsync(url).Result)
            {
                var tileSources = WmtsParser.Parse(response);
                var nature2000TileSource = tileSources.First(t => t.Name == "natura2000");
                return new TileLayer(nature2000TileSource) { Name = nature2000TileSource.Name };
            }
        }
    }
}