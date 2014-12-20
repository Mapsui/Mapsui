using System;
using System.Linq;
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

        public ITileSchema Schema { get; private set; }
        public string Name { get; private set; }
        public ITileProvider Provider { get; private set; }

        public byte[] GetTile(TileInfo tileInfo)
        {
            return Provider.GetTile(tileInfo);
        }
        
        public static ITileSchema GetTileSchema()
        {
            var schema = new GlobalSphericalMercator(YAxis.TMS);
            schema.Resolutions.Clear();
            schema.Resolutions["0"] = new Resolution { Id = "0", UnitsPerPixel = 156543.033900000 };
            schema.Resolutions["1"] = new Resolution { Id = "1", UnitsPerPixel = 78271.516950000 };
            return schema;
        }
    }
}
