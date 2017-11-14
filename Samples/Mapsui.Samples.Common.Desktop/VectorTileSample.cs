using System.Collections.Generic;
using System.IO;
using System.Linq;
using BruTile.Predefined;
using BruTile.Web;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Utilities;
using Mapsui.VectorTiles;
using Newtonsoft.Json;

namespace Mapsui.Samples.Common.Desktop
{
    public static class VectorTileSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            // A regular tile layer with a tile source that rasterizes vector tiles
            map.Layers.Add(new TileLayer(CreateVectorTileTileSource()) { Name = "Rasterized Vector Tiles" });
            // A vector tile layer that parses the binary tiles returned by the regular tile layer
            map.Layers.Add(new VectorTileLayer(CreateRegularTileSource()) { Name = " Pure Vector Tiles", Style = new VectorStyle(), Enabled = false });

            return map;
        }

        // todo: remove. Used only for debugging while developing.
        private static readonly HashSet<string> Layers = new HashSet<string>();

        private static IStyle CreateStyle()
        {
            var stream = EmbeddedResourceLoader.Load("Resources.vector-tile-style.json", typeof(VectorTileSample));
            var vectorTileStyle = Deserialize<VectorTileStyle>(stream);

            return new ThemeStyle(f =>
            {
                if (f.Geometry is Point) return null;
                string name = f["layer-name"] as string;

                var layer = vectorTileStyle.layers.FirstOrDefault(l => l.id == name);
                if (layer == null) return null; //todo log
                var color = ToColor(layer.paint.fillcolor);
                Layers.Add(name);
                return new VectorStyle
                {
                    Fill = new Brush(color),
                    Line = new Pen(Color.Gray),
                    Outline = new Pen(Color.Black)
                };
            });
        }

        private static Color ToColor(string color)
        {
            if (color == null) return Color.Red;
            if (color.StartsWith("rgba"))
                return ColorParser.ToColorFromRgba(color);
            if (color.StartsWith("hsl"))
                return ColorParser.HslToColor(color);
            return null;
        }

        private static T Deserialize<T>(Stream stream)
        {
            var serializer = JsonSerializer.Create();
            using (var sr = new StreamReader(stream))
            using (var jsonTextReader = new JsonTextReader(sr))
            {
                return serializer.Deserialize<T>(jsonTextReader);
            }
        }

        private static HttpVectorTileSource CreateVectorTileTileSource()
        {
            return new HttpVectorTileSource(new GlobalSphericalMercator(),
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
