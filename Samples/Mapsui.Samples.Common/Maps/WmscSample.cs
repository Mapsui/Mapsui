using System;
using System.Collections.Generic;
using BruTile;
using BruTile.Predefined;
using BruTile.Web;
using BruTile.Wmsc;
using Mapsui.Layers;
using Mapsui.UI;
using Attribution = BruTile.Attribution;

namespace Mapsui.Samples.Common.Maps
{
    public class WmscSample // Disable because the WMSC service is down (or moved) : ISample
        // todo: Replace with another.
    {
        public string Name => "";

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
            return new TileLayer(new GeodanWorldWmscTileSource());
        }
    }

    public class GeodanWorldWmscTileSource : ITileSource
    {
        public GeodanWorldWmscTileSource()
        {
            Schema = new GlobalSphericalMercator(YAxis.TMS);
            Provider = GetTileProvider(Schema);
            Name = "Geodan WMS-C";
        }

        public ITileSchema Schema { get; }
        public string Name { get; }
        public Attribution Attribution { get; } = new Attribution();
        public ITileProvider Provider { get; }

        public byte[] GetTile(TileInfo tileInfo)
        {
            return Provider.GetTile(tileInfo);
        }

        private static ITileProvider GetTileProvider(ITileSchema schema)
        {
            return new HttpTileProvider(GetRequestBuilder(schema));
        }

        private static IRequest GetRequestBuilder(ITileSchema schema)
        {
            const string url = "http://geoserver.nl/tiles/tilecache.aspx?";
            var parameters = new Dictionary<string, string>();
            var request = new WmscRequest(new Uri(url), schema,
                new List<string>(new[] {"world_GM"}), new List<string>(), parameters);
            return request;
        }
    }
}