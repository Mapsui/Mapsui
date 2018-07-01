using System.Collections.Generic;
using System.IO;
using BruTile;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps
{
    public static class TilesSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                BackColor = Color.Transparent,
                Home = n => n.NavigateTo(new Point(-7641856, 4804912), 51116)
            };

            var tileIndexes = new[]
            {
                new TileIndex(0, 0, "1"),
                new TileIndex(1, 0, "1"),
                new TileIndex(0, 1, "1"),
                new TileIndex(1, 1, "1")
            };

            var features = TileIndexToFeatures(tileIndexes, new SampleTileSource());
            map.Layers.Add(new MemoryLayer {DataSource = new MemoryProvider(features), Name = "Tiles"});
            return map;
        }

        private static List<IFeature> TileIndexToFeatures(TileIndex[] tileIndexes, ITileSource tileSource)
        {
            var features = new List<IFeature>();
            foreach (var tileIndex in tileIndexes)
            {
                var tileInfo = new TileInfo
                {
                    Index = tileIndex,
                    Extent = TileTransform.TileToWorld(
                        new TileRange(tileIndex.Col, tileIndex.Row), tileIndex.Level, tileSource.Schema)
                };

                var feature = new Feature
                {
                    Geometry = new Raster(new MemoryStream(
                        tileSource.GetTile(tileInfo)), tileInfo.Extent.ToBoundingBox())
                };

                features.Add(feature);
            }
            return features;
        }
    }
}