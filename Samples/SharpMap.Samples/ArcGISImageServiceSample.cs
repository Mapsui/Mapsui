using Mapsui.Layers;
using Mapsui.Providers.ArcGISImageService;
using Mapsui.Styles;

namespace Mapsui.Samples
{
    public static class ArcGISImageServiceSample
    {
        public static ILayer Create()
        {
            var provider = CreateProvider();
            var layer = new ImageLayer("ArcGISImageServiceLayer");
            layer.Styles.Add(new VectorStyle()); // This is ugly. I need to add a style to get it to render even though it is not used.
            layer.DataSource = provider;
            return layer;
        }

        private static ArcGISImageServiceProvider CreateProvider()
        {
            var info = new ArcGISImageServiceInfo();
            info.Url = "http://imagery.arcgisonline.com/ArcGIS/rest/services/LandsatGLS/FalseColor/ImageServer/exportImage";
            info.Format = "jpgpng";
            info.Interpolation = InterpolationType.NearestNeighbor;
            info.F = "image";
            info.ImageSR = "102100";
            info.BBoxSR = "102100";
            info.Time = "268211520000,1262217600000";
            return new ArcGISImageServiceProvider(info);
        }
    }
}
