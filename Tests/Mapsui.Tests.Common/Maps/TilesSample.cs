using System.Collections.Generic;
using System.IO;
using BruTile;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class TilesSample : ISample
    {
        public string Name => "Tiles";
        public string Category => "Tests";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var layer = CreateLayer();

            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateToFullEnvelope()
            };

            map.Layers.Add(layer);

            return map;
        }

        private static MemoryLayer CreateLayer()
        {
            var tileIndexes = new[]
            {
                new TileIndex(0, 0, 1),
                new TileIndex(1, 0, 1),
                new TileIndex(0, 1, 1),
                new TileIndex(1, 1, 1)
            };

            var features = TileIndexToFeatures(tileIndexes, new SampleTileSource());
            var layer = new MemoryLayer { DataSource = new MemoryProvider<RasterFeature>(features), Name = "Tiles" };
            return layer;
        }

        private static List<RasterFeature> TileIndexToFeatures(TileIndex[] tileIndexes, ITileSource tileSource)
        {
            var features = new List<RasterFeature>();
            foreach (var tileIndex in tileIndexes)
            {
                var tileInfo = new TileInfo
                {
                    Index = tileIndex,
                    Extent = TileTransform.TileToWorld(
                        new TileRange(tileIndex.Col, tileIndex.Row), tileIndex.Level, tileSource.Schema)
                };

                var raster = new MRaster(new MemoryStream(tileSource.GetTile(tileInfo)), tileInfo.Extent.ToMRect());
                features.Add(new RasterFeature(raster));
            }
            return features;
        }
    }
}