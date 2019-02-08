using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.UI;
using Mapsui.Utilities;

namespace Mapsui.Tests.Common.Maps
{
    public class WritableLayerSample : ISample
    {
        public string Name => "WritableLayer";

        public string Category => "Tests";

        private WritableLayer _writableLayer;
        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
            _writableLayer = (WritableLayer) mapControl.Map.Layers[1];
            mapControl.Info += MapControlOnInfo;
        }

        private void MapControlOnInfo(object sender, MapInfoEventArgs e)
        {
            _writableLayer.Add(new Feature{ Geometry =
                new Point(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y)});
            _writableLayer.DataHasChanged();
        }

        private Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
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
