using System.Threading.Tasks;
using Mapsui.ArcGIS;
using Mapsui.ArcGIS.ImageServiceProvider;
using Mapsui.Cache;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Desktop
{
    public class ArcGISImageServiceSample : ISample // disabled as sample because the service can not be reached : ISample
    {
        private ArcGISImageCapabilities? _capabilities;

        public string Name => "9 ArcGIS image";
        public string Category => "Desktop";
        
        public static IUrlPersistentCache? DefaultCache { get; set; }

        public async Task<ILayer> CreateLayerAsync()
        {
            return new ImageLayer("ArcGISImageServiceLayer") { DataSource = await CreateProviderAsync(DefaultCache) };
        }

        public async Task<Map> CreateMapAsync()
        {
            var map = new Map { Home = n => n.NavigateTo(new MPoint(0, 0), 1) };
            map.Layers.Add(await CreateLayerAsync());
            return map;
        }

        private async Task<ArcGISImageServiceProvider> CreateProviderAsync(IUrlPersistentCache? persistentCache = null)
        {
            //Get Capabilities from service
            var capabilitiesHelper = new CapabilitiesHelper(persistentCache);
            capabilitiesHelper.CapabilitiesReceived += CapabilitiesReceived;
            capabilitiesHelper.CapabilitiesFailed += capabilitiesHelper_CapabilitiesFailed;
            capabilitiesHelper.GetCapabilities(@"https://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/3", CapabilitiesType.ImageServiceCapabilities);

            //Create own
            ////return new ArcGISImageServiceProvider(
            ////    new ArcGISImageCapabilities("https://services.arcgisonline.com/arcgis/rest/services/World_Imagery/MapServer/3/exportImage", 268211520000, 1262217600000))
            ////{
            ////    CRS = "EPSG:102100"
            ////};

            while(_capabilities == null)
            {
                await Task.Delay(100);
            }

            return new ArcGISImageServiceProvider(
                _capabilities)
            {
                CRS = "EPSG:102100"
            };
        }

        private static void capabilitiesHelper_CapabilitiesFailed(object? sender, System.EventArgs e)
        {
            Logger.Log(LogLevel.Warning, "ArcGISImageService capabilities request failed");
        }

        private void CapabilitiesReceived(object? sender, System.EventArgs e)
        {
            // todo: make use of: 
            _capabilities = sender as ArcGISImageCapabilities;
        }
    }
}
