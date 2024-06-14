using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Common.Maps.Geometries;

public class GeometryCollectionSample : ISample
{
    public string Name => "GeometryCollections";
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
        return new Layer("GeometryCollection")
        {
            DataSource = new MemoryProvider(CreateGeometries())
        };
    }

    public static IEnumerable<IFeature> CreateGeometries()
    {
        yield return new GeometryFeature
        {
            Geometry = new GeometryCollection(
            new Geometry[] {
                new Point(-2000000, 2000000),
                new LineString(new[] {
                    new Coordinate(0, 0),
                    new Coordinate(0, 10000000),
                    new Coordinate(10000000, 10000000),
                    new Coordinate(10000000, 0),
                }),
                new LineString(new[] {
                    new Coordinate(1000000, 1000000),
                    new Coordinate(9000000, 1000000),
                    new Coordinate(9000000, 9000000),
                    new Coordinate(1000000, 9000000),
                }),
                new Polygon(
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
                    }
                )
            }),
            Styles = new IStyle[]
            {
                new VectorStyle
                {
                    Line = new Pen(Color.Red, 3),
                    Fill = new Brush(Color.Blue),
                    Outline = new Pen(Color.Purple, 5),
                },
            }
        };
    }
}
