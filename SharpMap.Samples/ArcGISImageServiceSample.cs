using System;
using System.Linq;
using SharpMap.Layers;
using SharpMap.Providers.ArcGISImageService;
using SharpMap.Styles;

namespace SharpMap.Samples
{
    public static class ArcGISImageServiceSample
    {
        public static Layer Create()
        {
            var provider = CreateProvider();
            var layer = new Layer("ArcGISImageServiceLayer");
            layer.Styles.Add(new VectorStyle()); // This is ugly. I need to add a style to get it to render even though it is not used.
            layer.DataSource = provider;
            return layer;
        }

        private static ArcGISImageServiceProvider CreateProvider()
        {
            var capabilities = new ArcGISImageServiceCapabilities();
            capabilities.Url = "http://imagery.arcgisonline.com/ArcGIS/rest/services/LandsatGLS/FalseColor/ImageServer/exportImage";
            capabilities.Format = "jpgpng";
            capabilities.Interpolation = InterpolationType.NearestNeighbor;
            capabilities.F = "image";
            capabilities.ImageSR = "102100";
            capabilities.BBoxSR = "102100";
            capabilities.Time = "268211520000,1262217600000";
            return new ArcGISImageServiceProvider(capabilities);
        }
    }
}
