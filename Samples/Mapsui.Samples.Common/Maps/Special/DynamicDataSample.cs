// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Special;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using NetTopologySuite.Geometries;

namespace Mapsui.Tests.Common.Maps
{
    public class DynamicDataSample : ISample
    {
        public string Name => "Dynamic Data Sample";
        public string Category => "Special";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        private Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());

            var layer = new Layer();
            var memoryProvider = new DynamicMemoryProvider();
            layer.DataSource = memoryProvider;
            map.Layers.Add(layer);

            map.Info += (s, e) =>
                {
                    if (e.MapInfo?.WorldPosition == null) return;

                    memoryProvider?.Add(new GeometryFeature
                    {
                        Geometry = new Point(e.MapInfo.WorldPosition.X, e.MapInfo.WorldPosition.Y)
                    });
                    // To notify the map that a redraw is needed.
                    memoryProvider?.DataHasChanged();
                    return;
                };

            return map;
        }
    }
}
