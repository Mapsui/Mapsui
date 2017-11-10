using System.Collections.Generic;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Utilities;
using Mapsui.VectorTiles;

namespace Mapsui.Samples.Common.Desktop
{
    public static class VectorTileSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            // A regular tile layer with a tile source that rasterizes vector tiles
            map.Layers.Add(new TileLayer(CreateVectorTileTileSource()) { Name = "Rasterized Vector Tiles"});
            // A vector tile layer that parses the binary tiles returned by the regular tile layer
            map.Layers.Add(new VectorTileLayer(CreateRegularTileSource()) { Name = " Pure Vector Tiles", Style = new VectorStyle(), Enabled = false});

            return map;
        }

        static HashSet<string> Layers = new HashSet<string>();

        private static IStyle CreateStyle()
        {
            return new ThemeStyle(f =>
            {
                if (f.Geometry is Point) return null;
                string className = f["class"] as string;
                Layers.Add((string)f["id"]);
                if (className == "state" || className == "town")
                    return new VectorStyle
                    {
                        Fill = new Brush(new Color(255, 255, 255, 255)),
                        Outline= new Pen(Color.Red),
                    };
                return new VectorStyle
                {
                    Fill = new Brush(new Color(0, 128, 0, 128)),
                    Line = new Pen(Color.Gray),
                    Outline = new Pen(Color.Black)
                };
            });
        }

        private static HttpVectorTileSource CreateVectorTileTileSource()
        {
            return new HttpVectorTileSource(
                new GlobalSphericalMercator(),
                "https://free.tilehosting.com/data/v3/{z}/{x}/{y}.pbf.pict?key=RiS4gsgZPZqe",
                name: "vector tile")
            {
                Style = CreateStyle()
            };
        }

        private static HttpTileSource CreateRegularTileSource()
        {
            return new HttpTileSource(new GlobalSphericalMercator(),
                "https://free.tilehosting.com/data/v3/{z}/{x}/{y}.pbf.pict?key=RiS4gsgZPZqe",
                tileFetcher: VectorTileLayer.FetchTile);
        }
    }
}
