using BruTile;
using BruTile.Cache;
using BruTile.FileSystem;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.UI;
using Attribution = BruTile.Attribution;

namespace Mapsui.Samples.Common.Desktop
{
    public class MapTilerSample : ISample
    {
        public string Name => "5 Map Tiler";
        public string Category => "Desktop";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            return new TileLayer(new MapTilerTileSource()) {Name = "True Marble in MapTiler"};
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
        public Attribution Attribution { get; } = new Attribution();
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
            schema.Resolutions[0] = new Resolution(0, 156543.033900000);
            schema.Resolutions[1] = new Resolution(1, 78271.516950000);
            return schema;
        }

        private static string GetAppDir()
        {
            return System.IO.Path.GetDirectoryName(
              System.Reflection.Assembly.GetEntryAssembly().GetModules()[0].FullyQualifiedName);
        }
    }
}
