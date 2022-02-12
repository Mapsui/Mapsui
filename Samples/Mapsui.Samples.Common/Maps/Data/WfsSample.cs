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

        private const string wfsUri = "https://geoservices1.civis.bz.it/geoserver/p_bz-AdministrativeUnits/ows";
        private const string crs = "EPSG:3857";  // originally: "EPSG:25832"
        private const string layerName = "Districts";
        private const string nsPrefix = "p_bz-AdministrativeUnits";
        private const string labelField = "NAME_DE";

        public static Map CreateMap()
        {
            try
            {
                var map = new Map() {CRS = crs};
                var provider = CreateWfsProvider(wfsUri);
                map.Layers.Add(CreateTileLayer(CreateTileSource()));
                map.Layers.Add(CreateWfsLayer(provider));
                map.Layers.Add(CreateLabelLayer(provider));

                map.Home = n => n.NavigateTo(new Point(1270000.0, 5880000.0), map.Resolutions[9]);

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
            return new Layer(layerName)
            {
                Style = new VectorStyle { Fill = new Brush { Color = Color.FromArgb(192, 255, 0, 0) } },
                DataSource = provider,
                IsMapInfoLayer = true
            };
        }

        private static WFSProvider CreateWfsProvider(string getCapabilitiesUri)
        {
            var provider = new WFSProvider(getCapabilitiesUri, nsPrefix, layerName,
                WFSProvider.WFSVersionEnum.WFS_1_1_0)
            {
                QuickGeometries = false,
                GetFeatureGetRequest = true,
                CRS = crs,
                Labels = new List<string> { labelField }
            };
            return provider;
        }
        
        private static ILayer CreateLabelLayer(WFSProvider provider)
        {
            // Labels
            // Labels are collected when parsing the geometry. So there's just one 'GetFeature' call necessary.
            // Otherwise (when calling twice for retrieving labels) there may be an inconsistent read...
            // If a label property is set, the quick geometry option is automatically set to 'false'.
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
        
        
        
        
        //public static HttpTileSource CreateTileSource()
        //{
        //    using (var httpClient = new HttpClient())
        //    using (var response = httpClient.GetStreamAsync("https://geoservices.buergernetz.bz.it/mapproxy/service/ows?SERVICE=WMTS&REQUEST=GetCapabilities").Result)
        //    {
        //        var tileSources = WmtsParser.Parse(response);
        //        return tileSources.First(t =>
        //            ((WmtsTileSchema) t.Schema).Layer == "P_BZ_OF_2014_2015_2017" && t.Schema.Srs == "EPSG:25832");
        //    }
        //}

        public static HttpTileSource CreateTileSource()
        {
            return BruTile.Predefined.KnownTileSources.Create(BruTile.Predefined.KnownTileSource.OpenStreetMap);
        }

        public static ILayer CreateTileLayer(ITileSource tileSource, string name = null)
        {
            return new TileLayer(tileSource) { Name = name ?? tileSource.Name, CRS = crs };
        }

    }
}