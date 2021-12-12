using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Layers.Tiling;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps
{
    public class VariousSample : ISample
    {
        public string Name => "5 Various geometries";
        public string Category => "Geometries";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();

            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(PolygonSample.CreateLayer());
            map.Layers.Add(LineStringSample.CreateLineStringLayer(LineStringSample.CreateLineStringStyle()));
            map.Layers.Add(CreateLayerWithStyleOnLayer(map.Extent, 10));
            map.Layers.Add(CreateLayerWithStyleOnFeature(map.Extent, 10));

            return map;
        }

        private static ILayer CreateLayerWithStyleOnLayer(MRect? envelope, int count = 25)
        {
            return new Layer("Style on Layer")
            {
                DataSource = new MemoryProvider<PointFeature>(RandomPointGenerator.GenerateRandomPoints(envelope, count).ToFeatures()),
                Style = CreateBitmapStyle("Mapsui.Samples.Common.Images.ic_place_black_24dp.png")
            };
        }

        private static ILayer CreateLayerWithStyleOnFeature(MRect? envelope, int count = 25)
        {
            var style = CreateBitmapStyle("Mapsui.Samples.Common.Images.loc.png");

            return new Layer("Style on feature")
            {
                DataSource = new MemoryProvider<IFeature>(GenerateRandomFeatures(envelope, count, style)),
                Style = null
            };
        }

        private static IEnumerable<IFeature> GenerateRandomFeatures(MRect? envelope, int count, IStyle style)
        {
            return RandomPointGenerator.GenerateRandomPoints(envelope, count, 123)
                .Select(p => new PointFeature(p) { Styles = new List<IStyle> { style } }).ToList();
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