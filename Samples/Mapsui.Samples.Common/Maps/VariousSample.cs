using System.Collections.Generic;
using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class VariousSample
    {
        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(PolygonSample.CreateLayer());
            map.Layers.Add(LineStringSample.CreateLineStringLayer(LineStringSample.CreateLineStringStyle()));
            map.Layers.Add(CreateLayerWithStyleOnLayer(map.Envelope, 10));
            map.Layers.Add(CreateLayerWithStyleOnFeature(map.Envelope, 10));

            return map;
        }

        private static ILayer CreateLayerWithStyleOnLayer(BoundingBox envelope, int count = 25)
        {
            return new Layer("Style on Layer")
            {
                DataSource = new MemoryProvider(RandomPointHelper.GenerateRandomPoints(envelope, count)),
                Style = CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png")
            };
        }

        private static ILayer CreateLayerWithStyleOnFeature(BoundingBox envelope, int count = 25)
        {
            var style = CreateBitmapStyle("Mapsui.Samples.Common.Images.loc.png");

            return new Layer("Style on feature")
            {
                DataSource = new MemoryProvider(GenerateRandomFeatures(envelope, count, style)),
                Style = null
            };
        }

        private static IEnumerable<IFeature> GenerateRandomFeatures(BoundingBox envelope, int count, IStyle style)
        {
            var result = new List<Feature>();
            var points = RandomPointHelper.GenerateRandomPoints(envelope, count, 123);
            foreach (var point in points)
            {
                result.Add(new Feature { Geometry = point, Styles = new List<IStyle> { style } });
            }
            return result;
        }

        private static SymbolStyle CreateBitmapStyle(string embeddedResourcePath)
        {
            var bitmapId = GetBitmapIdForEmbeddedResource(embeddedResourcePath);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = 0.75 };
        }

        private static int GetBitmapIdForEmbeddedResource(string imagePath)
        {
            var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
            var image = assembly.GetManifestResourceStream(imagePath);
            var bitmapId = BitmapRegistry.Instance.Register(image);
            return bitmapId;
        }
    }
}