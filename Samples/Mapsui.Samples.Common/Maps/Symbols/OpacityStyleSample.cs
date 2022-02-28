﻿using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using NetTopologySuite.Geometries;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps
{
    public class OpacityStyleSample : ISample
    {
        public string Name => "OpacityStyle";
        public string Category => "Symbols";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map();
            map.Layers.Add(OpenStreetMap.CreateTileLayer());
            map.Layers.Add(CreatePolygonLayer());
            map.Layers.Add(CreateLineStringLayer());
            return map;
        }

        public static ILayer CreatePolygonLayer()
        {
            return new Layer("Polygons")
            {
                DataSource = new MemoryProvider<IFeature>(CreatePolygon().ToFeature()),
                Style = new VectorStyle
                {
                    Fill = new Brush(new Color(150, 150, 30)),
                    Outline = new Pen
                    {
                        Color = Color.Orange,
                        Width = 2,
                        PenStyle = PenStyle.Solid,
                        PenStrokeCap = PenStrokeCap.Round
                    },
                    Opacity = 0.7f,
                }
            };
        }
        public static ILayer CreateLineStringLayer()
        {
            return new Layer("Polygons")
            {
                DataSource = new MemoryProvider<IFeature>(CreateLineString().ToFeature()),
                Style = new VectorStyle
                {
                    Line = new Pen
                    {
                        Color = new Color(new Color(30, 150, 150)),
                        PenStrokeCap = PenStrokeCap.Round,
                        PenStyle = PenStyle.Solid,
                        Width = 10,
                    },
                    Opacity = 0.5f,
                }
            };
        }

        private static Polygon CreatePolygon()
        {
            return new Polygon(
                new LinearRing(new[] {
                    new Coordinate(0, 0),
                    new Coordinate(0, 10000000),
                    new Coordinate(10000000, 10000000),
                    new Coordinate(10000000, 0),
                    new Coordinate(0, 0)
                }),
                new[] { new LinearRing(new [] {
                    new Coordinate(1000000, 1000000),
                    new Coordinate(9000000, 1000000),
                    new Coordinate(9000000, 9000000),
                    new Coordinate(1000000, 9000000),
                    new Coordinate(1000000, 1000000)
                })}
            );
        }

        private static LineString CreateLineString()
        {
            return new LineString(new[] {
                new Coordinate(1000000, 1000000),
                new Coordinate(9000000, 1000000),
                new Coordinate(9000000, 9000000),
                new Coordinate(1000000, 9000000),
                new Coordinate(1000000, 1000000)
            });
        }
    }
}