using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using BruTile;
using BruTile.Web;
using Mapsui.ArcGIS.DynamicProvider;
using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers.Wfs;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Data
{
    public class ArcGISSample : ISample
    {
        public string Name => "9. ArcGIS";
        public string Category => "Data";
        public static IUrlPersistentCache? DefaultCache { get; set; }

        private const string wfsUri = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Water_Network_Base_Map/MapServer";
        private const string crs = "EPSG:3857";
        private const string layerName = "ArcGis";

        public Task<Map> CreateMapAsync()
        {
            try
            {
                var map = new Map() { CRS = crs };
                var provider = CreateArcGisProvider(wfsUri);
                map.Layers.Add(OpenStreetMap.CreateTileLayer());
                map.Layers.Add(CreateArcgisLayer(provider));
                map.Layers.Add(CreateLabelLayer(provider));

                map.Home = n => n.NavigateTo(new MPoint(0.0, 5880000.0), map.Resolutions[3]);

                return Task.FromResult(map);

            }
            catch (WebException ex)
            {
                Logger.Log(LogLevel.Warning, ex.Message, ex);
                throw;
            }
        }

        private static ILayer CreateArcgisLayer(ArcGISDynamicProvider provider)
        {
            return new Layer(layerName)
            {
                Style = new VectorStyle { Fill = new Brush { Color = Color.FromArgb(192, 255, 0, 0) } },
                DataSource = provider,
                IsMapInfoLayer = true
            };
        }

        private static ArcGISDynamicProvider CreateArcGisProvider(string getCapabilitiesUri)
        {
            var provider = new ArcGISDynamicProvider(
                getCapabilitiesUri,
                persistentCache: DefaultCache)
            {
                CRS = crs,
            };
            return provider;
        }

        private static ILayer CreateLabelLayer(ArcGISDynamicProvider provider)
        {
            // Labels
            // Labels are collected when parsing the geometry. So there's just one 'GetFeature' call necessary.
            // Otherwise (when calling twice for retrieving labels) there may be an inconsistent read...
            // If a label property is set, the quick geometry option is automatically set to 'false'.
            // provider.Labels.Add(labelField);

            return new Layer("labels")
            {
                DataSource = provider,
                MaxVisible = 350,
                Style = new LabelStyle
                {
                    CollisionDetection = false,
                    ForeColor = Color.Black,
                    Font = new Font { FontFamily = "GenericSerif", Size = 10 },
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    LabelColumn = "labelField"
                }
            };
        }
    }
}