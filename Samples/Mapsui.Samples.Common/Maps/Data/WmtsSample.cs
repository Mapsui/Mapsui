using System.Linq;
using BruTile.Cache;
using BruTile.Wmts;
using Mapsui.Cache;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Tiling.Layers;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Data
{
    public class WmtsSample : ISample
    {
        public string Name => "3 WMTS";
        public string Category => "Data";
        public static IPersistentCache<byte[]>? DefaultCache { get; set; }

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map
            {
                CRS = "EPSG:28992"
            };
            map.Layers.Add(CreateLayer());
            map.Layers.Add(GeodanOfficesSample.CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            var url = "http://geodata.nationaalgeoregister.nl/wmts/top10nl?VERSION=1.0.0&request=GetCapabilities";

            using var response = (DefaultCache as IUrlPersistentCache).UrlCachedStream(url);
            var tileSources = WmtsParser.Parse(response);
            var nature2000TileSource = tileSources.First(t => t.Name == "natura2000");
            nature2000TileSource.PersistentCache = DefaultCache;
            return new TileLayer(nature2000TileSource) { Name = nature2000TileSource.Name };
            
        }
    }
}