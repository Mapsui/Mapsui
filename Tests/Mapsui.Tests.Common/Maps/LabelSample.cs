using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps
{
    public static class LabelSample
    {
        public static Map CreateMap()
        {
            var map = new Map
            {
                BackColor = Color.Transparent,
                Home = n => n.NavigateTo(new Point(100, 100), 1)
            };
            map.Layers.Add(new MemoryLayer
            {
                Style = null,
                DataSource = CreateProviderWithLabels(),
                Name = "Labels"
            });
            return map;
        }

        private static MemoryProvider CreateProviderWithLabels()
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
                    Styles = new[] {new LabelStyle {Text = "Black Text", BackColor = null}}
                },
                new Feature
                {
                    Geometry = new Point(150, 50),
                    Styles =
                        new[]
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
                    Geometry = new Point(150, 150),
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
                    Geometry = new Point(50, -50),
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
                    Geometry = new Point(100, 100),
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
            var provider = new MemoryProvider(features);
            return provider;
        }
    }
}