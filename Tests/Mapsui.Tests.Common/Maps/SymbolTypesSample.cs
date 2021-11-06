using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.GeometryLayer;
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
            var layer = CreateLayer();

            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateTo(layer.Extent.Grow(layer.Extent.Width * 2))
            };

            map.Layers.Add(layer);

            return map;
        }

        private static MemoryLayer CreateLayer()
        {
            return new MemoryLayer
            {
                DataSource = new GeometryMemoryProvider<IGeometryFeature>(CreateFeatures()),
                Name = "Symbol Types",
                Style = null
            };
        }

        private static IEnumerable<IGeometryFeature> CreateFeatures()
        {
            return new List<IGeometryFeature>()
            {
                new GeometryFeature
                {
                    Geometry = new Point(0, 00),
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
                new GeometryFeature
                {
                    Geometry = new Point(50, 0),
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
                new GeometryFeature
                {
                    Geometry = new Point(0, 50),
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