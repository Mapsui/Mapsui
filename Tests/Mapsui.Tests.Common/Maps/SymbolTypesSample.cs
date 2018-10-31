using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Samples.Common;
using Mapsui.Styles;
using Mapsui.UI;

namespace Mapsui.Tests.Common.Maps
{
    public class SymbolTypesSample : ISample
    {
        public string Name => "Symbol Types";
        public string Category => "Tests";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map
            {
                BackColor = Color.Transparent,
                Home = n => n.NavigateTo(new Point(0, 0), 0.5)
            };

            map.Layers.Add(new MemoryLayer
            {
                DataSource = new MemoryProvider(CreateFeatures()),
                Name = "Symbol Types",
                Style = null
            });

            return map;
        }

        private static Features CreateFeatures()
        {
            return new Features
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
                },
                new Feature
                {
                    Geometry = new Point(-20, 20),
                    Styles = new List<IStyle>
                    {
                        new SymbolStyle
                        {
                            Fill = new Brush {Color = Color.Gray},
                            Outline = new Pen(Color.Black),
                            SymbolType = SymbolType.Triangle
                        }
                    }
                }
            };
        }
    }
}