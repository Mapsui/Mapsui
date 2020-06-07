using System;
using System.Linq;
using BruTile;
using BruTile.Predefined;
using BruTile.Web;
using BruTile.Wmsc;
using Mapsui.Layers;
using Mapsui.UI;
using Attribution = BruTile.Attribution;

namespace Mapsui.Samples.Common.Maps
{
    /// <summary>
    /// An ordinary WMS service called through a tiled schema (WMS-C) 
    /// </summary>
    public class TiledWmsSample : ISample
    {
        public string Name => "4 WMS called tiled";
        public string Category => "Data";

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
            return new TileLayer(CreateTileSource()) {Name = "Omgevingswarmte (PDOK)"};
        }

        public static ITileSource CreateTileSource()
        {
            const string url = "http://geodata.nationaalgeoregister.nl/omgevingswarmte/wms?SERVICE=WMS&VERSION=1.1.1";
            // You need to know the schema. This can be a problem. Usally it is GlobalSphericalMercator
            var schema = new WkstNederlandSchema { Format = "image/png", Srs = "EPSG:28992" };
            var request = new WmscRequest(new Uri(url), schema, new[] { "koudegeslotenwkobuurt" }.ToList(), new string[0].ToList());
            var provider = new HttpTileProvider(request);
            return new TileSource(provider, schema) { Name = "Omgevingswarmte (PDOK)" };
        }
    }
}