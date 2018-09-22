using Mapsui.Projection;
using Mapsui.Styles;
using Mapsui.Utilities;
using Mapsui.Widgets.CenterCross;

namespace Mapsui.Samples.Common.Maps
{
    public static class CenterCrossSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                CRS = "EPSG:3857",
                Transformation = new MinimalTransformation()
            };
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            
            map.Widgets.Add(new CenterCrossWidget(map) { Color = Color.Red, Halo = Color.White, Width = 50, Height = 30 });

            return map;
        }
    }
}