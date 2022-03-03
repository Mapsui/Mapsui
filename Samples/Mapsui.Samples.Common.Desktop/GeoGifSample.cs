using System.IO;
using System.Reflection;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Desktop
{
    public class GeoGifSample : ISample
    {
        public string Name => "6 Geo Gif";
        public string Category => "Desktop";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            var gif = new GeoTiffProvider(GetAppDir() + "\\Images\\example.tif");
            map.Layers.Add(CreateGifLayer(gif));

            return map;
        }

        private static ILayer CreateGifLayer(IProvider<IFeature> gifSource)
        {
            return new Layer
            {
                Name = "GeoGif",
                DataSource = gifSource,
            };
        }

        private static string GetAppDir()
        {
            return Path.GetDirectoryName(Assembly.GetEntryAssembly()!.GetModules()[0].FullyQualifiedName)!;
        }
    }
}