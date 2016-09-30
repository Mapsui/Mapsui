using Mapsui.Projection;

namespace Mapsui.Samples.Common
{
    public static class ProjectionSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Transformation = new MinimalTransformation();
            map.CRS = "EPSG:3857";
            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(PointsInWgs84Sample.CreateLayer());
            return map;
        }
    }
}
