using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public class SvgSample : ISample
    {
        public string Name => "Svg";
        public string Category => "Styles";

        public Task<Map> CreateMapAsync()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateSvgLayer(map.Extent));

            return Task.FromResult(map);
        }

        private static ILayer CreateSvgLayer(MRect? envelope)
        {
            return new MemoryLayer
            {
                Name = "Svg Layer",
                Features = CreateSvgFeatures(RandomPointGenerator.GenerateRandomPoints(envelope, 2000)),
                Style = null,
                IsMapInfoLayer = true
            };
        }

        private static IEnumerable<IFeature> CreateSvgFeatures(IEnumerable<MPoint> randomPoints)
        {
            var counter = 0;

            return randomPoints.Select(p =>
            {
                var feature = new PointFeature(p) { ["Label"] = counter.ToString() };
                feature.Styles.Add(CreateSvgStyle(@"Images.Pin.svg", 0.5));
                counter++;
                return feature;
            });
        }

        private static SymbolStyle CreateSvgStyle(string embeddedResourcePath, double scale)
        {
            var bitmapId = typeof(SvgSample).LoadSvgId(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new Offset(0.0, 0.5, true) };
        }
    }
}