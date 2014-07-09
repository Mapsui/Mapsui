using Mapsui.Layers;
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
            throw new System.NotImplementedException();
        }

        private static void CapabilitiesReceived(object sender, System.EventArgs e)
        {
            var capabilities = sender as ArcGISImageCapabilities;
        }
    }
}
