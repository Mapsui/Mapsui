using System.IO;
using SharpMap;
using SharpMap.Data.Providers;
using SharpMap.Layers;
using SharpMap.Styles;
using Geom = SharpMap.Geometries;
using SharpMap.Providers;
using System;
using BruTile.Web;

namespace SilverlightSample
{
    public class MySample
    {
        public static Map InitializeMap(Stream image)
        {
            Map map = new Map();

            Layer geodanLayer = new Layer("Geodan");
            geodanLayer.DataSource = new MemoryProvider(new Geom.Point(4.9130567, 52.3422033));
            var style = new VectorStyle();
            
            style.Symbol = new Bitmap() { data = image };
            geodanLayer.Style = style;
            map.Layers.Add(geodanLayer);

            return map;
        }
    }
}
