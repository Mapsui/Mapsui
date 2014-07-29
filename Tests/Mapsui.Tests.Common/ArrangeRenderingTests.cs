using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Tests.Common
{
    public static class ArrangeRenderingTests
    {
        public static Map RenderPointsWithVectorStyle()
        {
            var map = new Map { Viewport = { Center = new Point(100, 100), Width = 200, Height = 200, Resolution = 1 } };
            map.Layers.Add(new MemoryLayer
                {
                    Style = null,
                    DataSource = Utilities.CreateProviderWithPointsWithVectorStyle()
                });
            return map;
        }

        public static Map RenderPointWithBitmapSymbols()
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

        public static Map RenderRotatedBitmapSymbolWithOffset()
        {
            var map = new Map { Viewport = { Center = new Point(80, 80), Width = 200, Height = 200, Resolution = 1 } };
            var layer = new MemoryLayer { DataSource = Utilities.CreateProviderWithRotatedBitmapSymbols() };
            map.Layers.Add(layer);
            return map;
        }

        public static Map RenderPointsWithDifferentSymbolTypes()
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

        public static Map RenderSymbolWithWorldUnits()
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

        public static Map RenderPolygon()
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

        public static Map RenderLine()
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
            var provider = Utilities.CreateLineProvider();
            layer.DataSource = provider;
            map.Layers.Add(layer);
            return map;
        }
    }
}
