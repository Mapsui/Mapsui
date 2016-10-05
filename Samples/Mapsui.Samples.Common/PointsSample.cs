using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common
{
    public static class PointsSample
    {
        private static readonly Random Random = new Random(0);

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OsmSample.CreateLayer());
            map.Layers.Add(CreateRandomPointLayer(map.Envelope));
            return map;
        }

        public static MemoryProvider CreateRandomPointsProvider(BoundingBox envelope)
        {
            var randomPoints = GenerateRandomPoints(envelope, 100);
            var features = new Features();
            var count = 0;
            foreach (var point in randomPoints)
            {
                var feature = new Feature
                {
                    Geometry = point,
                    ["Label"] = count.ToString(CultureInfo.InvariantCulture)
                };
                features.Add(feature);
                count++;
            }
            return new MemoryProvider(features);
        }

        private static Feature CreateBitmapPoint()
        {
            var feature = new Feature {Geometry = new Point(0, 1000000)};
            feature.Styles.Add(CreateBitmapStyle("Mapsui.Samples.Common.Images.loc.png"));
            return feature;
        }

        public static SymbolStyle CreateBitmapStyle(string embeddedResourcePath)
        {
            var bitmapId = GetBitmapIdForEmbeddedResource(embeddedResourcePath);
            return new SymbolStyle {BitmapId = bitmapId};
        }

        private static int GetBitmapIdForEmbeddedResource(string imagePath)
        {
            var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
            var image = assembly.GetManifestResourceStream(imagePath);
            var bitmapId = BitmapRegistry.Instance.Register(image);
            return bitmapId;
        }

        public static IEnumerable<IGeometry> GenerateRandomPoints(BoundingBox box, int count = 25)
        {
            var result = new List<IGeometry>();
            for (var i = 0; i < count; i++)
                result.Add(new Point(Random.NextDouble()*box.Width + box.Left,
                    Random.NextDouble()*box.Height - (box.Height - box.Top)));
            return result;
        }
        
        public static ILayer CreateRandomPointLayer(BoundingBox envelope, int count = 25, IStyle style = null)
        {
            return new Layer("Point Layer")
            {
                DataSource = new MemoryProvider(GenerateRandomPoints(envelope, count)),
                Style = style ?? new VectorStyle {Fill = new Brush(Color.White)}
            };
        }

        public static ILayer CreateBitmapPointLayer(IStyle style = null)
        {
            return new Layer("bitmapPointLayer")
            {
                DataSource = new MemoryProvider(CreateBitmapPoint()),
                Style = null
            };
        }
    }
}