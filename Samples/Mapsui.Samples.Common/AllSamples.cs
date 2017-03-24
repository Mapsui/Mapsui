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
                ["Simple Points"] = () => PointsSample.CreateMap(),
                ["Simple LineStrings"] = () => LineStringSample.CreateMap(),
                ["Simple Polygons"] = () => PolygonSample.CreateMap(),
                ["Various"] = () => VariousSample.CreateMap(),
                ["Labels"] = () => LabelsSample.CreateMap(),
                ["Center on location"] = () => CenterOnLocationSample.CreateMap(),
                ["Projected point"] = () => ProjectionSample.CreateMap(),
                ["Stacked labels"] = () => StackedLabelsSample.CreateMap(),
                ["Symbols"] = () => SymbolsSample.CreateMap(),
                ["Info"] = () => InfoLayersSample.CreateMap(),
                ["Animated point movement"] = () => AnimatedPointsSample.CreateMap(),
                ["TileSources: WMTS"] = () => WmtsSample.CreateMap(),
                ["TileSources: Bing maps"] = () => BingSample.CreateMap(),
                ["TileSources: TMS"] = () => TmsSample.CreateMap(),
                ["TileSources: WMS-C"] = () => WmscSample.CreateMap(),
                ["TileSources: regular WMS"] = () => TiledWmsSample.CreateMap(),
                ["Rasterizing Layer"] = () => RasterizingLayerSample.CreateMap(),
                ["Empty Map"] = () => EmptyMapSample.CreateMap(),
                ["Mutating triangle"] = () => MutatingTriangleSample.CreateMap(),
                ["Symbols in World Units"] = () => SymbolsInWorldUnitsSample.CreateMap(),

            };
        }
    }
}
