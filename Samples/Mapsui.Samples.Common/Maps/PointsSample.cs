using System.Reflection;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Helpers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class PointsSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateRandomPointLayer(map.Envelope));
            return map;
        }
        
        public static ILayer CreateRandomPointLayer(BoundingBox envelope)
        {
            return new MemoryLayer
            {
                DataSource = new MemoryProvider(RandomPointHelper.GenerateRandomPoints(envelope)),
                Style = CreateBitmapStyle()
            };
        }
        
        private static SymbolStyle CreateBitmapStyle()
        {
            // For this sample we get the bitmap from an embedded resouce
            // but you could get the data stream from the web or anywhere
            // else.
            var path = "Mapsui.Samples.Common.Images.ic_place_black_24dp.png";
            var bitmapId = GetBitmapIdForEmbeddedResource(path);
            return new SymbolStyle { BitmapId = bitmapId, SymbolScale = 0.75 };
        }

        private static int GetBitmapIdForEmbeddedResource(string imagePath)
        {
            var assembly = typeof(PointsSample).GetTypeInfo().Assembly;
            var image = assembly.GetManifestResourceStream(imagePath);
            return BitmapRegistry.Instance.Register(image);
        }
    }
}