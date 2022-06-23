using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BruTile.Wmts;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps
{
    public class WmtsMichelinSample : ISample
    {
        public string Name => "5 WMTS Michelin";
        public string Category => "Data";

       public async Task<Map> CreateMapAsync()
        {
            var map = new Map();
            map.Layers.Add(await CreateLayerAsync());
            return map;
        }

        public static async Task<ILayer> CreateLayerAsync()
        {
            using var httpClient = new HttpClient();
            // When testing today (20-10-2021) tile 0,0,0 returned a 500. Perhaps this should be fixed in the xml.
            using var response = await httpClient.GetStreamAsync("https://bertt.github.io/wmts/capabilities/michelin.xml");
            var tileSource = WmtsParser.Parse(response).First();

            if (Michelin.DefaultCache != null)
            {
                tileSource.PersistentCache = Michelin.DefaultCache;
            }
            
            return new TileLayer(tileSource) { Name = tileSource.Name };
        }
    }
}