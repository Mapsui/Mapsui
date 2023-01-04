using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Mapsui.Extensions.Provider;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common.Desktop.Utilities;
using Mapsui.Styles;
using Mapsui.UI;

#pragma warning disable IDISP001 // Dispose created

namespace Mapsui.Samples.Common.Desktop
{
    public class GeoTiffSample : IMapControlSample
    {
        static GeoTiffSample()
        {
            GeoTiffDeployer.CopyEmbeddedResourceToFile("example.shp");
        }
        
        public string Name => "10 Geo Tiff";
        public string Category => "Data";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            var gif = new GeoTiffProvider(GeoTiffDeployer.GeoTiffLocation + "\\example.tif", new List<Color> { Color.Red });
            map.Layers.Add(CreateGifLayer(gif));

            return map;
        }

        private static ILayer CreateGifLayer(IProvider gifSource)
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