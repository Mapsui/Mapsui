using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class LineStringSample
    {
        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateLineStringLayer(CreateLineStringStyle()));
            return map;
        }

        public static ILayer CreateLineStringLayer(IStyle style = null)
        {
            return new MemoryLayer
            {
                DataSource = new MemoryProvider(new List<Feature> { new Feature { Geometry =
                            new LineString(new[]
                            {
                                new Point(0, 0),
                                new Point(1000000, 0),
                                new Point(1000000, 1000000),
                                new Point(2000000, 1000000),
                                new Point(2000000, 2000000),
                                new Point(3000000, 2000000),
                                new Point(3000000, 3000000),
                                new Point(4000000, 3000000),
                                new Point(4000000, 4000000),
                                new Point(5000000, 4000000)
                            })
                    },
                        new Feature { Geometry =
                                new LineString(new[]
                                {
                                    new Point(2000000, 0),
                                    new Point(5000000, 3000000),
                                    new Point(5500000, 3500000),
                                    new Point(6000000, 3000000),
                                    new Point(6500000, 3500000),
                                    new Point(7000000, 3000000),
                                    new Point(7000000, 2000000),
                                    new Point(6500000, 2500000),
                                    new Point(6000000, 2000000),
                                    new Point(5500000, 2500000),
                                    new Point(5000000, 2000000),
                                }),
                        }
                    }
                ),
                Name = "LineStringLayer",
                Style = style
            };
        }

        public static IStyle CreateLineStringStyle()
        {
            return new VectorStyle
            {
                Fill = null,
                Outline = null,
                Line = {Color = Color.Red, Width = 4, PenStyle = PenStyle.UserDefined, DashArray = new float[] { 6, 4, 12, 4 } }
            };
        }
    }
}