using System;
using System.Collections.Generic;

namespace Mapsui.Tests.Common.Maps
{
    public static class AllSamples
    {
        public static List<Func<Map>> CreateList()
        {
            return new List<Func<Map>>
            {
                VectorStyleSample.CreateMap,
                CircleAndRectangleSymbolSample.CreateMap,
                BitmapSymbolSample.CreateMap,
                BitmapSymbolWithRotationAndOffsetSample.CreateMap,
                PointInWorldUnits.CreateMap,
                PolygonSample.CreateMap,
                LineSample.CreateMap,
                TilesSample.CreateMap,
                LabelSample.CreateMap
            };
        }
    }
}