using System;
using System.Collections.Generic;
using Mapsui.Tests.Common.Maps;

namespace Mapsui.Tests.Common
{
    public static class AllSamples
    {
        public static List<Func<Map>> CreateList()
        {
            return new List<Func<Map>>
            {
                StackedLabelsSample.CreateMap,
                VectorStyleSample.CreateMap,
                SymbolTypesSample.CreateMap,
                BitmapSymbolSample.CreateMap,
                BitmapSymbolWithRotationAndOffsetSample.CreateMap,
                PointInWorldUnits.CreateMap,
                PolygonSample.CreateMap,
                LineSample.CreateMap,
                TilesSample.CreateMap,
                LabelSample.CreateMap,
                BitmapSample.CreateMap
            };
        }
    }
}