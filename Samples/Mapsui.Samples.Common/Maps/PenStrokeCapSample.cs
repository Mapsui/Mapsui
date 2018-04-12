using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Utilities;

namespace Mapsui.Samples.Common.Maps
{
    public static class PenStrokeCapSample
    {
        private const int PolygonSize = 5000000;
        private const int PenWidth = 12;

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            return new Layer("Polygons")
            {
                DataSource = new MemoryProvider(CreatePolygon()),
                Style = null
            };
        }

        private static IEnumerable<IFeature> CreatePolygon()
        {
            return new[]
            {
                new Feature
                {
                    Geometry = new LineString(new[]
                    {
                        new Point(1 * PolygonSize, 1 * PolygonSize),
                        new Point(1 * PolygonSize, 2 * PolygonSize),
                        new Point(2 * PolygonSize, 2 * PolygonSize),
                        new Point(2 * PolygonSize, 1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Gray, PenWidth) {PenStrokeCap = PenStrokeCap.Butt, StrokeJoin = StrokeJoin.Miter}
                        }
                    },
                },
                new Feature
                {
                    Geometry = new LineString(new[]
                    {
                        new Point(1 * PolygonSize, 1 * PolygonSize),
                        new Point(1 * PolygonSize, 2 * PolygonSize),
                        new Point(2 * PolygonSize, 2 * PolygonSize),
                        new Point(2 * PolygonSize, 1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Red) {PenStrokeCap = PenStrokeCap.Square}
                        }
                    },
                },
                new Feature
                {
                    Geometry = new LineString(new[]
                    {
                        new Point(1 * PolygonSize, -1 * PolygonSize),
                        new Point(1 * PolygonSize, -2 * PolygonSize),
                        new Point(2 * PolygonSize, -2 * PolygonSize),
                        new Point(2 * PolygonSize, -1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Gray, PenWidth) {PenStrokeCap = PenStrokeCap.Round, StrokeJoin = StrokeJoin.Round}
                        }
                    },
                },
                new Feature
                {
                    Geometry = new LineString(new[]
                    {
                        new Point(1 * PolygonSize, -1 * PolygonSize),
                        new Point(1 * PolygonSize, -2 * PolygonSize),
                        new Point(2 * PolygonSize, -2 * PolygonSize),
                        new Point(2 * PolygonSize, -1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Red) {PenStrokeCap = PenStrokeCap.Square}
                        }
                    },
                },
                new Feature
                {
                    Geometry = new LineString(new[]
                    {
                        new Point(-1 * PolygonSize, 1 * PolygonSize),
                        new Point(-1 * PolygonSize, 2 * PolygonSize),
                        new Point(-2 * PolygonSize, 2 * PolygonSize),
                        new Point(-2 * PolygonSize, 1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Gray, PenWidth) {PenStrokeCap = PenStrokeCap.Square, StrokeJoin = StrokeJoin.Bevel}
                        }
                    },
                },
                new Feature
                {
                    Geometry = new LineString(new[]
                    {
                        new Point(-1 * PolygonSize, 1 * PolygonSize),
                        new Point(-1 * PolygonSize, 2 * PolygonSize),
                        new Point(-2 * PolygonSize, 2 * PolygonSize),
                        new Point(-2 * PolygonSize, 1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Red) {PenStrokeCap = PenStrokeCap.Square}
                        }
                    },
                }
            };
        }
    }
}