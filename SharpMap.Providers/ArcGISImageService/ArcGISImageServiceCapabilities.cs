using System;
using System.Linq;

namespace SharpMap.Providers.ArcGISImageService
{
    public enum InterpolationType
    {
        BilinearInterpolation,
        CubicConvolution,
        Majority,
        NearestNeighbor
    }

    public class ArcGISImageServiceCapabilities
    {
        public string Url { get; set; }
        public string Format { get; set; }
        public InterpolationType Interpolation { get; set; }
        public string F { get; set; }
        public string Time { get; set; }
        public string ImageSR { get; set; } // todo: get from Mapsui srid
        public string BBoxSR { get; set; } // todo: get from Mapsui srid
    }
}
