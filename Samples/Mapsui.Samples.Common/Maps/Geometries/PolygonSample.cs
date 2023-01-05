using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Geometries;

public class PolygonSample : ISample
{
    public string Name => "3 Polygons";
    public string Category => "Geometries";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateLayer());
        return Task.FromResult(map);
    }

    public static ILayer CreateLayer()
    {
        return new Layer("Polygons")
        {
            DataSource = new MemoryProvider(CreatePolygon().ToFeatures()),
            Style = new VectorStyle
            {
                Fill = new Brush(new Color(150, 150, 30, 128)),
                Outline = new Pen
                {
                    Color = Color.Orange,
                    Width = 2,
                    PenStyle = PenStyle.DashDotDot,
                    PenStrokeCap = PenStrokeCap.Round
                }
            }
        };
    }

    private static List<Polygon> CreatePolygon()
    {
        var result = new List<Polygon>();

        var polygon1 = new Polygon(
            new LinearRing(new[] {
                new Coordinate(0, 0),
                new Coordinate(0, 10000000),
                new Coordinate(10000000, 10000000),
                new Coordinate(10000000, 0),
                new Coordinate(0, 0)
            }),
            new[] {
                new LinearRing(new[] {
                    new Coordinate(1000000, 1000000),
                    new Coordinate(9000000, 1000000),
                    new Coordinate(9000000, 9000000),
                    new Coordinate(1000000, 9000000),
                    new Coordinate(1000000, 1000000)
                })
            });

        result.Add(polygon1);

        var polygon2 = new Polygon(
            new LinearRing(new[] {
                new Coordinate(-10000000, 0),
                new Coordinate(-15000000, 5000000),
                new Coordinate(-10000000, 10000000),
                new Coordinate(-5000000, 5000000),
                new Coordinate(-10000000, 0)
            }),
            new[] {
                new LinearRing(new[] {
                    new Coordinate(-10000000, 1000000),
                    new Coordinate(-6000000, 5000000),
                    new Coordinate(-10000000, 9000000),
                    new Coordinate(-14000000, 5000000),
                    new Coordinate(-10000000, 1000000)
                })
            });

        result.Add(polygon2);

        return result;
    }
}
