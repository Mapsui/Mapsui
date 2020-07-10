using BruTile;
using BruTile.Predefined;

namespace Mapsui.Tests.Common
{
    class SampleTileSource : ITileSource
    {
        public SampleTileSource()
        {
            Schema = GetTileSchema();
            Provider = new SampleTileProvider();
        }

        public ITileSchema Schema { get; }
        public string Name { get; } = "TileSource";
        public Attribution Attribution { get; } = new Attribution();
        public ITileProvider Provider { get; }

        public byte[] GetTile(TileInfo tileInfo)
        {
            return Provider.GetTile(tileInfo);
        }
        
        public static ITileSchema GetTileSchema()
        {
            var schema = new GlobalSphericalMercator(YAxis.TMS);
            schema.Resolutions.Clear();
            schema.Resolutions[0] = new Resolution(0, 156543.033900000);
            schema.Resolutions[1] = new Resolution(1, 78271.516950000);
            return schema;
        }
    }
}
