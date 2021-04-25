using System.Linq;
using System.Net;
using System.Net.Http;
using BruTile;
using BruTile.Web;
using BruTile.Wmts;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers.Wfs;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Wfs
{
    public class WfsSample : ISample
    {
        public string Name => "6 WFS Sample";
        public string Category => "Desktop";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            try
            {
                const string serviceUri = "https://geoservices.buergernetz.bz.it/geoserver/ows";

                var map = new Map() {CRS = "EPSG:25832"};
                map.Layers.Add(CreateTileLayer(CreateTileSource()));
                map.Layers.Add(CreateWfsLayer(serviceUri));
                
                var bb = new BoundingBox(550000, 5050000, 800000, 5400000);
                map.Limiter = new ViewportLimiterKeepWithin
                {
                    PanLimits = bb
                };
                
                return map;
                
            }
            catch (WebException ex)
            {
                Logger.Log(LogLevel.Warning, ex.Message, ex);
                throw;
            }
        }

        private static ILayer CreateWfsLayer(string getCapabilitiesUri)
        {
            var provider = CreateWfsProvider(getCapabilitiesUri);

            return new Layer("COMUNI_AMMINISTRATIVI")
            {
                Style = new VectorStyle {Fill = new Brush {Color = Color.Red}},
                DataSource = provider
            };
        }

        private static WFSProvider CreateWfsProvider(string getCapabilitiesUri)
        {
            var provider = new WFSProvider(getCapabilitiesUri, "p_bz-cadastre", "COMUNI_AMMINISTRATIVI",
                WFSProvider.WFSVersionEnum.WFS_1_1_0)
            {
                QuickGeometries = false,
                GetFeatureGetRequest = true,
                CRS = "EPSG:25832"
            };
            return provider;
        }
        
        public static HttpTileSource CreateTileSource()
        {
            using (var httpClient = new HttpClient())
            using (var response = httpClient.GetStreamAsync("https://geoservices.buergernetz.bz.it/mapproxy/service/ows?SERVICE=WMTS&REQUEST=GetCapabilities").Result)
            {
                var tileSources = WmtsParser.Parse(response);
                return tileSources.First(t =>
                    ((WmtsTileSchema) t.Schema).Layer == "P_BZ_OF_2014_2015_2017" && t.Schema.Srs == "EPSG:25832");
            }
        }

        public static ILayer CreateTileLayer(ITileSource tileSource, string name = null)
        {
            return new TileLayer(tileSource) {Name = name ?? tileSource.Name};
        }

    }
}