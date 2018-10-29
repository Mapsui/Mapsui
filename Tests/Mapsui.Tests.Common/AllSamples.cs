using System;
using System.Linq;
using System.Collections.Generic;
using Mapsui.Tests.Common.Maps;

namespace Mapsui.Tests.Common
{
    public static class AllSamples
    {
        public static IEnumerable<ITestSample> GetSamples()
        {
            var type = typeof(ITestSample);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.StartsWith("Mapsui"));

            return assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p) && !p.IsInterface)
                .Select(Activator.CreateInstance).Select(t => t as ITestSample).ToList();
        }

        private static List<Func<Map>> CreateList()
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