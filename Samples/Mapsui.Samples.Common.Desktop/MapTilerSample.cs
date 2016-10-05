using BruTile;
using BruTile.Cache;
using BruTile.FileSystem;
using BruTile.Predefined;
using Mapsui.Layers;

namespace Mapsui.Samples.Common.Desktop
{
    public static class MapTilerSample
    {
        public static ILayer CreateLayer()
        {
            return new TileLayer(new MapTilerTileSource()) {Name = "True Marble in MapTiler"};
        }
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            return map;
        }
    }
    public class MapTilerTileSource : ITileSource
    {
        public MapTilerTileSource()
        {
            Schema = GetTileSchema();
            Provider = GetTileProvider();
            Name = "MapTiler";
        }

        public ITileSchema Schema { get; }
        public string Name { get; }
        public ITileProvider Provider { get; }

        public byte[] GetTile(TileInfo tileInfo)
        {
            return Provider.GetTile(tileInfo);
        }
        
        public static ITileProvider GetTileProvider()
        {
            return new FileTileProvider(new FileCache(GetAppDir() + "\\GeoData\\TrueMarble", "png"));
        }

        public static ITileSchema GetTileSchema()
        {
            var schema = new GlobalSphericalMercator(YAxis.TMS);
            schema.Resolutions.Clear();
            schema.Resolutions["0"] = new Resolution("0", 156543.033900000);
            schema.Resolutions["1"] = new Resolution("1", 78271.516950000);
            return schema;
        }

        private static string GetAppDir()
        {
            return System.IO.Path.GetDirectoryName(
              System.Reflection.Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName);
        }
    }
}
