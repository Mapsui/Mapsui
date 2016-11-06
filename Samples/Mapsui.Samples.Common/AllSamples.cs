using System;
using System.Collections.Generic;
using Mapsui.Samples.Common.Maps;

namespace Mapsui.Samples.Common
{
    public static class AllSamples
    {
        public static Dictionary<string, Func<Map>> CreateList()
        {
            return new Dictionary<string, Func<Map>>
            {
                ["OpenStreetMap"] = () => OsmSample.CreateMap(),
                ["Projected point"] = () => ProjectionSample.CreateMap(),
                ["Animated point movement"] = () => AnimatedPointsSample.CreateMap(),
                ["Stacked labels"] = () => StackedLabelsSample.CreateMap(),
                ["Info"] = () => InfoLayersSample.CreateMap(),
                ["Tiled request to WMS"] = () => TiledWmsSample.CreateMap(),
                ["TMS"] = () => TmsSample.CreateMap(),
                ["Bing maps"] = () => BingSample.CreateMap(),
                ["WMS-C"] = () => WmscSample.CreateMap(),
                ["Symbols in World Units"] = () => SymbolsInWorldUnitsSample.CreateMap(),
                ["WMTS"] = () => WmtsSample.CreateMap(),
                ["Labels"] = () => LabelsSample.CreateMap(),
                ["Rasterizing Layer"] = () => RasterizingLayerSample.CreateMap(),
                ["Polygons"] = () => PolygonSample.CreateMap(),
                ["LineStrings"] = () => LineStringSample.CreateMap(),
                ["Points"] = () => PointsSample.CreateMap(),
                ["Various Layers"] = () => VariousSample.CreateMap(),
                ["Empty Map"] = () => EmptyMapSample.CreateMap()
            };
        }
    }
}
