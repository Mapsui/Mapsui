using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Providers.ArcGIS;
using Mapsui.Providers.ArcGIS.Image;

namespace Mapsui.Samples.Common.Desktop
{
    public static class ArcGISImageServiceSample
    {
        public static ILayer CreateLayer()
        {
            return new ImageLayer("ArcGISImageServiceLayer") { DataSource = CreateProvider() };
        }

        public static Map CreateMap()
        {
            var map = new Map {Home = n => n.NavigateTo(new Point(0, 0), 1)};
            map.Layers.Add(CreateLayer());
            return map;
        }

        private static ArcGISImageServiceProvider CreateProvider()
        {
            //Get Capabilities from service
            var capabilitiesHelper = new CapabilitiesHelper();
            capabilitiesHelper.CapabilitiesReceived += CapabilitiesReceived;
            capabilitiesHelper.CapabilitiesFailed += capabilitiesHelper_CapabilitiesFailed;
            capabilitiesHelper.GetCapabilities(@"http://imagery.arcgisonline.com/ArcGIS/rest/services/LandsatGLS/FalseColor/ImageServer", CapabilitiesType.ImageServiceCapabilities);
           
            //Create own
            return new ArcGISImageServiceProvider(
                new ArcGISImageCapabilities("http://imagery.arcgisonline.com/ArcGIS/rest/services/LandsatGLS/FalseColor/ImageServer/exportImage", 268211520000, 1262217600000))
            {
                CRS = "EPSG:102100"
            };
        }

        private static void capabilitiesHelper_CapabilitiesFailed(object sender, System.EventArgs e)
        {
            Logger.Log(LogLevel.Warning, "ArcGISImageService capabilities request failed");
        }

        private static void CapabilitiesReceived(object sender, System.EventArgs e)
        {
            //todo: make use of: var capabilities = sender as ArcGISImageCapabilities;
        }
    }
}
