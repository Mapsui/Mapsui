using System.IO;
using System.Reflection;
using BruTile.Web;
using SharpMap.Geometries;
using SharpMap.Layers;
using SharpMap.Providers;
using SharpMap.Styles;

namespace SharpMap.Samples
{
    public static class TileLayerSample
    {
        public static Map InitializeMap()
        {
            var map = new Map();
            var osm = new Layer("OSM");
            osm.DataSource = new TileProvider(new OsmTileSource());
            map.Layers.Add(osm);
            map.Layers.Add(CreateGeodanLayer());
            return map;
        }

        public static ILayer CreateGeodanLayer()
        {
            var pointLayer = new Layer("Geodan");
            pointLayer.DataSource = new MemoryProvider(new Point(546919, 6862238)); // lonlat: 4.9130567, 52.3422033
            pointLayer.Styles.Add(new SymbolStyle { Symbol = new Bitmap { data = GetImageStreamFromResource("SharpMap.Samples.Images.icon.png") } });
            return pointLayer;
        }

        private static Stream GetImageStreamFromResource(string resourceString)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string icon = resourceString;
            Stream imageStream = assembly.GetManifestResourceStream(icon);
            return imageStream;
        }
    }
}
