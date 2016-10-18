using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps
{
    public static class CircleAndRectangleSymbolSample
    {
        public static Map CreateMap()
        {
            var map = new Map {Viewport = {Center = new Point(0, 0), Width = 200, Height = 100, Resolution = 0.5}};
            var features = new Features
            {
                new Feature
                {
                    Geometry = new Point(-20, 0),
                    Styles = new List<IStyle>
                    {
                        new SymbolStyle
                        {
                            Fill = new Brush {Color = Color.Gray},
                            Outline = new Pen(Color.Black),
                            SymbolType = SymbolType.Ellipse
                        }
                    }
                },
                new Feature
                {
                    Geometry = new Point(20, 0),
                    Styles = new List<IStyle>
                    {
                        new SymbolStyle
                        {
                            Fill = new Brush {Color = Color.Gray},
                            Outline = new Pen(Color.Black),
                            SymbolType = SymbolType.Rectangle
                        }
                    }
                }
            };
            var layer = new MemoryLayer
            {
                DataSource = new MemoryProvider(features),
                Name = "Points with different symbol types"
            };
            map.Layers.Add(layer);
            return map;
        }
    }
}