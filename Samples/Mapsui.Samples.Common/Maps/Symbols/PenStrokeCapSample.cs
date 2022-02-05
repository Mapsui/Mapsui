using System.Collections.Generic;
using Mapsui.Layers;
using Mapsui.Layers.Tiling;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.UI;
using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Common.Maps
{
    public class PenStrokeCapSample : ISample
    {
        private const int PolygonSize = 5000000;
        private const int PenWidth = 12;

        public string Name => "Pen Stroke Cap";
        public string Category => "Symbols";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

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
                DataSource = new MemoryProvider<IFeature>(CreatePolygon()),
                Style = null
            };
        }

        private static IEnumerable<IFeature> CreatePolygon()
        {
            return new[]
            {
                new GeometryFeature
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(1 * PolygonSize, 1 * PolygonSize),
                        new Coordinate(1 * PolygonSize, 2 * PolygonSize),
                        new Coordinate(2 * PolygonSize, 2 * PolygonSize),
                        new Coordinate(2 * PolygonSize, 1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Gray, PenWidth) {PenStrokeCap = PenStrokeCap.Butt, StrokeJoin = StrokeJoin.Miter}
                        }
                    },
                },
                new GeometryFeature
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(1 * PolygonSize, 1 * PolygonSize),
                        new Coordinate(1 * PolygonSize, 2 * PolygonSize),
                        new Coordinate(2 * PolygonSize, 2 * PolygonSize),
                        new Coordinate(2 * PolygonSize, 1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Red) {PenStrokeCap = PenStrokeCap.Square}
                        }
                    },
                },
                new GeometryFeature
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(1 * PolygonSize, -1 * PolygonSize),
                        new Coordinate(1 * PolygonSize, -2 * PolygonSize),
                        new Coordinate(2 * PolygonSize, -2 * PolygonSize),
                        new Coordinate(2 * PolygonSize, -1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Gray, PenWidth) {PenStrokeCap = PenStrokeCap.Round, StrokeJoin = StrokeJoin.Round}
                        }
                    },
                },
                new GeometryFeature
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(1 * PolygonSize, -1 * PolygonSize),
                        new Coordinate(1 * PolygonSize, -2 * PolygonSize),
                        new Coordinate(2 * PolygonSize, -2 * PolygonSize),
                        new Coordinate(2 * PolygonSize, -1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Red) {PenStrokeCap = PenStrokeCap.Square}
                        }
                    },
                },
                new GeometryFeature
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(-1 * PolygonSize, 1 * PolygonSize),
                        new Coordinate(-1 * PolygonSize, 2 * PolygonSize),
                        new Coordinate(-2 * PolygonSize, 2 * PolygonSize),
                        new Coordinate(-2 * PolygonSize, 1 * PolygonSize)
                    }),
                    Styles = new[]
                    {
                        new VectorStyle
                        {
                            Line = new Pen(Color.Gray, PenWidth) {PenStrokeCap = PenStrokeCap.Square, StrokeJoin = StrokeJoin.Bevel}
                        }
                    },
                },
                new GeometryFeature
                {
                    Geometry = new LineString(new[]
                    {
                        new Coordinate(-1 * PolygonSize, 1 * PolygonSize),
                        new Coordinate(-1 * PolygonSize, 2 * PolygonSize),
                        new Coordinate(-2 * PolygonSize, 2 * PolygonSize),
                        new Coordinate(-2 * PolygonSize, 1 * PolygonSize)
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