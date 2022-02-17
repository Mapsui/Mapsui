using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Samples.Common;
using Mapsui.Tiling;
using Mapsui.UI;
using NetTopologySuite.Geometries;

#pragma warning disable IDISP004 // Don't ignore created IDisposable
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Tests.Common.Maps
{
    public class WritableLayerSample : ISample
    {
        public string Name => "WritableLayer";
        public string Category => "Special";

        private WritableLayer? _writableLayer;

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
            _writableLayer = (WritableLayer)mapControl.Map.Layers[1];
            mapControl.Info += MapControlOnInfo;
        }

        private void MapControlOnInfo(object sender, MapInfoEventArgs e)
        {
            if (e.MapInfo?.WorldPosition == null)
                return;

            _writableLayer?.Add(new GeometryFeature
            {
                Geometry = new Point(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y)
            });
            _writableLayer?.DataHasChanged();
        }

        private Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            var writableLayer = new WritableLayer();

            writableLayer.Add(new GeometryFeature());
            map.Layers.Add(writableLayer);

            return map;
        }
    }
}
