using System.Collections.Generic;
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

namespace Mapsui.Samples.Common.Maps.Data
{
    public class WfsSample : ISample
    {
        public string Name => "7. WFS";
        public string Category => "Data";

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
                var provider = CreateWfsProvider(serviceUri);
                map.Layers.Add(CreateTileLayer(CreateTileSource()));
                map.Layers.Add(CreateWfsLayer(provider));
                map.Layers.Add(CreateLabelLayer(provider));
                
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

        private static ILayer CreateWfsLayer(WFSProvider provider)
        {
            return new Layer("COMUNI_AMMINISTRATIVI")
            {
                Style = new VectorStyle {Fill = new Brush {Color = Color.Red}},
                DataSource = provider,
                IsMapInfoLayer = true
            };
        }

        private static WFSProvider CreateWfsProvider(string getCapabilitiesUri)
        {
            var provider = new WFSProvider(getCapabilitiesUri, "p_bz-cadastre", "COMUNI_AMMINISTRATIVI",
                WFSProvider.WFSVersionEnum.WFS_1_1_0)
            {
                QuickGeometries = false,
                GetFeatureGetRequest = true,
                CRS = "EPSG:25832",
                Labels = new List<string> {"CAMM_NOME_DE"}
            };
            return provider;
        }
        
        private static ILayer CreateLabelLayer(WFSProvider provider)
        {
            // Labels
            // Labels are collected when parsing the geometry. So there's just one 'GetFeature' call necessary.
            // Otherwise (when calling twice for retrieving labels) there may be an inconsistent read...
            // If a label property is set, the quick geometry option is automatically set to 'false'.
            const string labelField = "CAMM_NOME_DE";
            provider.Labels.Add(labelField);

            return new Layer("labels")
            {
                DataSource = provider,
                MaxVisible = 350,
                Style = new LabelStyle
                {
                    CollisionDetection = false,
                    ForeColor = Color.Black,
                    Font = new Font {FontFamily = "GenericSerif", Size = 10},
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    LabelColumn = labelField
                }
            };
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