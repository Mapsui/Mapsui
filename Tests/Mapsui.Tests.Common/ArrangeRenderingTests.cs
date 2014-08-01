using System.Collections.Generic;
using System.IO;
using BruTile;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Tests.Common
{
    public static class ArrangeRenderingTests
    {
        public static Map PointsWithVectorStyle()
        {
            var map = new Map { Viewport = { Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1 } };
            map.Layers.Add(new MemoryLayer
                {
                    Style = null,
                    DataSource = Utilities.CreateProviderWithPointsWithVectorStyle()
                });
            return map;
        }

        public static Map PointWithBitmapSymbols()
        {
            var map = new Map { Viewport = { Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1 } };
            var layer = new MemoryLayer
                {
                    Style = null,
                    DataSource = Utilities.CreateProviderWithPointsWithSymbolStyles()
                };
            map.Layers.Add(layer);
            return map;
        }

        public static Map RotatedBitmapSymbolWithOffset()
        {
            var map = new Map { Viewport = { Center = new Point(80, 80), Width = 200, Height = 200, Resolution = 1 } };
            var layer = new MemoryLayer { DataSource = Utilities.CreateProviderWithRotatedBitmapSymbols() };
            map.Layers.Add(layer);
            return map;
        }

        public static Map PointsWithDifferentSymbolTypes()
        {
            var map = new Map { Viewport = { Center = new Point(0, 0), Width = 200, Height = 100, Resolution = 0.5 } };
            var features = new Features
                {
                    Utilities.CreateSimplePointFeature(-20, 0, new SymbolStyle {Fill = new Brush { Color = Color.Gray}, SymbolType = SymbolType.Ellipse}),
                    Utilities.CreateSimplePointFeature(20, 0, new SymbolStyle {Fill = new Brush { Color = Color.Gray}, SymbolType = SymbolType.Rectangle})
                };
            var layer = new MemoryLayer { DataSource = new MemoryProvider(features) };
            map.Layers.Add(layer);
            return map;
        }

        public static Map SymbolWithWorldUnits()
        {
            var map = new Map { Viewport = { Center = new Point(0, 0), Width = 200, Height = 100, Resolution = 0.5 } };
            var features = new Features
                {
                    Utilities.CreateSimplePointFeature(-20, 0, new SymbolStyle {UnitType = UnitType.Pixel}),
                    Utilities.CreateSimplePointFeature(20, 0, new SymbolStyle {UnitType = UnitType.WorldUnit})
                };
            var layer = new MemoryLayer { DataSource = new MemoryProvider(features) };
            map.Layers.Add(layer);
            return map;
        }

        public static Map Polygon()
        {
            var map = new Map
            {
                Viewport =
                    {
                        Center = new Point(0, 0),
                        Width = 600,
                        Height = 400,
                        Resolution = 63000
                    }
            };

            var layer = new MemoryLayer();
            var provider = Utilities.CreatePolygonProvider();
            layer.DataSource = provider;
            map.Layers.Add(layer);
            return map;
        }

        public static Map Line()
        {
            var map = new Map
            {
                Viewport =
                    {
                        Center = new Point(0, 0),
                        Width = 600,
                        Height = 400,
                        Resolution = 63000
                    }
            };

            var layer = new MemoryLayer { Style = null };
            var provider = Utilities.CreateLineProvider();
            layer.DataSource = provider;
            map.Layers.Add(layer);
            return map;
        }

        public static Map Tiles()
        {
            var map = new Map
            {
                Viewport =
                {
                    Center = new Point(-7641856, 4804912),
                    Width = 600,
                    Height = 400,
                    Resolution = 51116
                }
            };

            var tileIndexes = new[]
            {
                new TileIndex(0, 0, "1"),
                new TileIndex(1, 0, "1"),
                new TileIndex(0, 1, "1"),
                new TileIndex(1, 1, "1")
            };

            var features = TileIndexToFeatures(tileIndexes, new SampleTileSource());
            map.Layers.Add(new MemoryLayer{ DataSource = new MemoryProvider(features) });
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
                    Extent =
                        TileTransform.TileToWorld(new TileRange(tileIndex.Col, tileIndex.Row), tileIndex.Level,
                            tileSource.Schema)
                };

                var feature = new Feature
                {
                    Geometry = new Raster(new MemoryStream(tileSource.Provider.GetTile(tileInfo)),
                            tileInfo.Extent.ToBoundingBox())
                };

                features.Add(feature);
            }
            return features;
        }
    }
}
