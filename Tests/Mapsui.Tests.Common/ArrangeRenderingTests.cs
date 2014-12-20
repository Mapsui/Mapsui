using System;
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
        public static readonly List<Func<Map>> Samples = new List<Func<Map>>();

        static ArrangeRenderingTests()
        {
            Samples.Add(PointsWithBitmapSymbols);
            Samples.Add(PointsWithDifferentSymbolTypes);
            Samples.Add(PointsWithVectorStyle);
            Samples.Add(PointsWithBitmapRotatedAndOffset);
            Samples.Add(PointsWithWorldUnits);
            Samples.Add(Polygon);
            Samples.Add(Line);
            Samples.Add(Tiles);
            Samples.Add(Labels);
        }

        public static Map PointsWithVectorStyle()
        {
            var map = new Map { Viewport = { Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1 } };
            map.Layers.Add(new MemoryLayer
            {
                Style = null,
                DataSource = Utilities.CreateProviderWithPointsWithVectorStyle()
                ,
                Name = "Points with VectorStyle"
            });
            return map;
        }

        public static Map PointsWithBitmapSymbols()
        {
            var map = new Map { Viewport = { Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1 } };
            var layer = new MemoryLayer
            {
                Style = null,
                DataSource = Utilities.CreateProviderWithPointsWithSymbolStyles(),
                Name = "Points with bitmaps"
            };
            map.Layers.Add(layer);
            return map;
        }

        public static Map PointsWithBitmapRotatedAndOffset()
        {
            var map = new Map { Viewport = { Center = new Point(80, 80), Width = 200, Height = 200, Resolution = 1 } };
            var layer = new MemoryLayer
            {
                DataSource = Utilities.CreateProviderWithRotatedBitmapSymbols(), 
                Name = "Points with rotated bitmaps",
                Style = null
            };
            map.Layers.Add(layer);
            return map;
        }

        public static Map PointsWithDifferentSymbolTypes()
        {
            var map = new Map { Viewport = { Center = new Point(0, 0), Width = 200, Height = 100, Resolution = 0.5 } };
            var features = new Features
            {
                Utilities.CreateSimplePointFeature(-20, 0, new SymbolStyle {Fill = new Brush { Color = Color.Gray}, Outline = new Pen(Color.Black), SymbolType = SymbolType.Ellipse}),
                Utilities.CreateSimplePointFeature(20, 0, new SymbolStyle {Fill = new Brush { Color = Color.Gray}, Outline = new Pen(Color.Black), SymbolType = SymbolType.Rectangle})
            };
            var layer = new MemoryLayer
            {
                DataSource = new MemoryProvider(features),
                Name = "Points with different symbol types"
            };
            map.Layers.Add(layer);
            return map;
        }

        public static Map PointsWithWorldUnits()
        {
            var map = new Map { Viewport = { Center = new Point(0, 0), Width = 200, Height = 100, Resolution = 0.5 } };
            var features = new Features
            {
                Utilities.CreateSimplePointFeature(-20, 0, new SymbolStyle {UnitType = UnitType.Pixel}),
                Utilities.CreateSimplePointFeature(20, 0, new SymbolStyle {UnitType = UnitType.WorldUnit})
            };
            var layer = new MemoryLayer { DataSource = new MemoryProvider(features), Name = "Points in world units"};
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

            var layer = new MemoryLayer
            {
                DataSource = Utilities.CreatePolygonProvider(), 
                Name = "Polygon"
            };
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

            var layer = new MemoryLayer
            {
                Style = null, 
                DataSource = Utilities.CreateLineProvider(), 
                Name = "Line"
            };

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
            map.Layers.Add(new MemoryLayer { DataSource = new MemoryProvider(features), Name = "Tiles"});
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
                    Geometry = new Raster(new MemoryStream(tileSource.GetTile(tileInfo)),
                            tileInfo.Extent.ToBoundingBox())
                };

                features.Add(feature);
            }
            return features;
        }

        public static Map Labels()
        {
            var map = new Map { Viewport = { Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1 } };
            map.Layers.Add(new MemoryLayer
            {
                Style = null,
                DataSource = CreateProviderWithLabels(),
                Name = "Labels"
            });
            return map;
        }

        public static MemoryProvider CreateProviderWithLabels()
        {
            var features = new Features
                {
                    new Feature
                        {
                            Geometry = new Point(50, 50),
                            Styles = new[] {new VectorStyle {Fill = new Brush(Color.Gray), Outline = new Pen(Color.Black)}}
                        },
                    new Feature
                        {
                            Geometry = new Point(50, 150),
                            Styles = new[]  { new LabelStyle{ Text = "Black Text", BackColor = null } } 
                        },
                    new Feature
                        {
                            Geometry = new Point(150, 50),
                            Styles = new[] { new LabelStyle{ Text = "Gray Backcolor", BackColor = new Brush(Color.Gray), ForeColor = Color.White} } 
                        },
                    new Feature
                        {
                            Geometry = new Point(150, 150),
                            Styles = new[]  { new LabelStyle{ Text = "Black Halo", ForeColor = Color.White, Halo = new Pen(Color.Red), BackColor = null} } 
                        }
                };
            var provider = new MemoryProvider(features);
            return provider;
        }


    }
}
