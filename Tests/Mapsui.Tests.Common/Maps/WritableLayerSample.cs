using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class WritableLayerSample : ISample
    {
        public string Name => "WritableLayer";

        public string Category => "Tests";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        private Map CreateMap()
        {
            var map = new Map();
            var writableLayer = new WritableLayer();
            writableLayer.Add(new Feature());
            writableLayer.Add(new Feature { Geometry = new Point() });
            writableLayer.Add(new Feature { Geometry = new LineString() });
            writableLayer.Add(new Feature{Geometry = new Polygon()});
            map.Layers.Add(writableLayer);

            return map;
        }
    }
}
