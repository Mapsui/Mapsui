using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BruTile.Wmts;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Data
{
    public class WmtsSample : ISample
    {
        public string Name => "3 WMTS";
        public string Category => "Data";

        public async void Setup(IMapControl mapControl)
        {
            try
            {
                mapControl.Map = await CreateMap();
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, e.Message, e);
            }
        }

        public static async Task<Map> CreateMap()
        {
            var map = new Map
            {
                CRS = "EPSG:28992"
            };
            map.Layers.Add(await CreateLayer());
            map.Layers.Add(GeodanOfficesSample.CreateLayer());
            return map;
        }

        public static async Task<ILayer> CreateLayer()
        {
            var url = "http://geodata.nationaalgeoregister.nl/wmts/top10nl?VERSION=1.0.0&request=GetCapabilities";

            using (var httpClient = new HttpClient())
            using (var response = await httpClient.GetStreamAsync(url))
            {
                var tileSources = WmtsParser.Parse(response);
                var nature2000TileSource = tileSources.First(t => t.Name == "natura2000");
                return new TileLayer(nature2000TileSource) { Name = nature2000TileSource.Name };
            }
        }
    }
}