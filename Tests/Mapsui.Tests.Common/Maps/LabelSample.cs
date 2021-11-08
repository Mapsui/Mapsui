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
    public class LabelSample : ISample
    {
        public string Name => "Label";
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
                Style = null,
                DataSource = CreateProviderWithLabels(),
                Name = "Labels"
            };
        }

        private static MemoryProvider<IFeature> CreateProviderWithLabels()
        {
            var features = new List<IFeature>
            {
                new GeometryFeature
                {
                    Geometry = new Point(100, 100),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Gray), Outline = new Pen(Color.Black)}}
                },
                new GeometryFeature
                {
                    Geometry = new Point(100, 200),
                    Styles = new[] {new LabelStyle {Text = "Black Text", BackColor = null}}
                },
                new GeometryFeature
                {
                    Geometry = new Point(100, 300),
                    Styles = new[]
                        {
                            new LabelStyle
                            {
                                Text = "Gray Backcolor",
                                BackColor = new Brush(Color.Gray),
                                ForeColor = Color.White
                            }
                        }
                },
                new GeometryFeature
                {
                    Geometry = new Point(300, 100),
                    Styles =
                        new[]
                        {
                            new LabelStyle
                            {
                                Text = "Black Halo",
                                ForeColor = Color.White,
                                Halo = new Pen(Color.Black),
                                BackColor = null
                            }
                        }
                },
                new GeometryFeature
                {
                    Geometry = new Point(300, 200),
                    Styles = new[]
                    {
                        new LabelStyle
                        {
                            Text = string.Empty,
                            BackColor = new Brush(Color.Black),
                            ForeColor = Color.White,
                            LabelMethod = f => null
                        }
                    }
                },
                new GeometryFeature
                {
                    Geometry = new Point(300, 300),
                    Styles = new[]
                    {
                        new LabelStyle
                        {
                            Text = "Multiline\nText",
                            BackColor = new Brush(Color.Gray),
                            ForeColor = Color.Black,
                            HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                        }
                    }
                },
            };
            var provider = new GeometryMemoryProvider<IFeature>(features);
            return provider;
        }
    }
}