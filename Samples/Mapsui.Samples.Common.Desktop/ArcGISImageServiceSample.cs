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
        public string Name => "4 ArcGIS image";
        public string Category => "Desktop";
        
        public static IUrlPersistentCache? DefaultCache { get; set; }

        public static ILayer CreateLayer()
        {
            return new ImageLayer("ArcGISImageServiceLayer") { DataSource = CreateProvider(DefaultCache) };
        }

        public static Task<Map> CreateMapAsync()
        {
            var map = new Map { Home = n => n.NavigateTo(new MPoint(0, 0), 1) };
            map.Layers.Add(CreateLayer());
            return Task.FromResult(map);
        }

        private static ArcGISImageServiceProvider CreateProvider(IUrlPersistentCache persistentCache = null)
        {
            //Get Capabilities from service
            var capabilitiesHelper = new CapabilitiesHelper(persistentCache);
            capabilitiesHelper.CapabilitiesReceived += CapabilitiesReceived;
            capabilitiesHelper.CapabilitiesFailed += capabilitiesHelper_CapabilitiesFailed;
            capabilitiesHelper.GetCapabilities(@"https://imagery.arcgisonline.com/ArcGIS/rest/services/LandsatGLS/FalseColor/ImageServer", CapabilitiesType.ImageServiceCapabilities);

            //Create own
            return new ArcGISImageServiceProvider(
                new ArcGISImageCapabilities("https://imagery.arcgisonline.com/ArcGIS/rest/services/LandsatGLS/FalseColor/ImageServer/exportImage", 268211520000, 1262217600000))
            {
                CRS = "EPSG:102100"
            };
        }

        private static void capabilitiesHelper_CapabilitiesFailed(object? sender, System.EventArgs e)
        {
            Logger.Log(LogLevel.Warning, "ArcGISImageService capabilities request failed");
        }

        private static void CapabilitiesReceived(object? sender, System.EventArgs e)
        {
            //todo: make use of: var capabilities = sender as ArcGISImageCapabilities;
        }
    }
}
