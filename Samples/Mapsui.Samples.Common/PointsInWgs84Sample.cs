using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;

namespace Mapsui.Samples.Common
{
    public static class PointsInWgs84Sample
    {
        public static ILayer CreateLayer()
        {
            return new Layer
            {
                DataSource = new MemoryProvider(new Point(4.643331, 52.433489)) { CRS = "EPSG:4326" },
                Name = "WGS84 Point"
            };
        }
    }
}
