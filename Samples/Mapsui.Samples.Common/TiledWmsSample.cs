using System;
using System.Linq;
using BruTile;
using BruTile.Web;
using BruTile.Predefined;
using BruTile.Wmsc;
using Mapsui.Layers;

namespace Mapsui.Samples.Common
{
    /// <summary>
    /// An ordinary WMS service called through a tiled schema (WMS-C) 
    /// </summary>
    public static class TiledWmsSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(CreateLayer());
            return map;
        }
        public static ILayer CreateLayer()
        { 
            return new TileLayer(new GeodanWorldWmsTileSource()) { Name = "WMS called as WMS-C" };
        }
    }

    public class GeodanWorldWmsTileSource : ITileSource
    {
        public GeodanWorldWmsTileSource()
        {
            var schema = new GlobalSphericalMercator(YAxis.TMS) { Srs = "EPSG:900913"};
            Provider = new HttpTileProvider(CreateWmsRequest(schema));
            Schema = schema;
        }

        public byte[] GetTile(TileInfo tileInfo)
        {
            return Provider.GetTile(tileInfo);
        }
        
        private static WmscRequest CreateWmsRequest(ITileSchema schema)
        {
            const string url = "http://geoserver.nl/world/mapserv.cgi?map=world/world.map&VERSION=1.1.1";
            return new WmscRequest(new Uri(url), schema, new[] {"world"}.ToList(), new string[0].ToList());
        }

        public ITileProvider Provider { get; }
        public ITileSchema Schema { get; }

        public string Name => "GeodanWorldWmsTileSource";
    }
}
