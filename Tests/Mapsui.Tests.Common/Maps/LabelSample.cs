using System.Collections.Generic;
using Mapsui.Geometries;
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
            var map = new Map
            {
                BackColor = Color.FromString("WhiteSmoke"),
                Home = n => n.NavigateTo(new MPoint(100, 100), 1)
            };
            map.Layers.Add(new MemoryLayer
            {
                Style = null,
                DataSource = CreateProviderWithLabels(),
                Name = "Labels"
            });
            return map;
        }

        private static MemoryProvider<IGeometryFeature> CreateProviderWithLabels()
        {
            var features = new List<IGeometryFeature>
            {
                new Feature
                {
                    Geometry = new Point(100, 100),
                    Styles = new[] {new VectorStyle {Fill = new Brush(Color.Gray), Outline = new Pen(Color.Black)}}
                },
                new Feature
                {
                    Geometry = new Point(100, 200),
                    Styles = new[] {new LabelStyle {Text = "Black Text", BackColor = null}}
                },
                new Feature
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
                new Feature
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
                new Feature
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
                new Feature
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
            var provider = new MemoryProvider<IGeometryFeature>(features);
            return provider;
        }
    }
}