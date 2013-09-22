using Mapsui.Layers;
using Mapsui.Providers.ArcGISImageService;

namespace Mapsui.Samples.Desktop
{
    public static class ArcGISImageServiceSample
    {
        public static ILayer Create()
        {
            return new ImageLayer("ArcGISImageServiceLayer") { DataSource = CreateProvider() };
        }

        private static ArcGISImageServiceProvider CreateProvider()
        {
            return new ArcGISImageServiceProvider(
                new ArcGISImageServiceInfo
                    {
                        Url = @"http://imagery.arcgisonline.com/ArcGIS/rest/servi
                            ces/LandsatGLS/FalseColor/ImageServer/exportImage",
                        Format = "jpgpng",
                        Interpolation = InterpolationType.NearestNeighbor,
                        F = "image",
                        ImageSR = "102100",
                        BBoxSR = "102100",
                        Time = "268211520000,1262217600000"
                    });
        }
    }
}
