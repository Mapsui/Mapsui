using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.UI;

#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Demo;

public class InfoLayersSample : ISample, ISampleTest
{
    private const string InfoLayerName = "Info Layer";
    private const string PolygonLayerName = "Polygon Layer";
    private const string LineLayerName = "Line Layer";

    public string Name => "2 Map Info";
    public string Category => "Demo";

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateInfoLayer(map.Extent));
        map.Layers.Add(CreatePolygonLayer());
        map.Layers.Add(new WritableLayer());
        map.Layers.Add(CreateLineLayer());

        return map;
    }

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    private static ILayer CreatePolygonLayer()
    {
        var features = new List<IFeature> { CreatePolygonFeature(), CreateMultiPolygonFeature() };

        var layer = new MemoryLayer
        {
            Name = PolygonLayerName,
            Features = features,
            Style = null,
            IsMapInfoLayer = true
        };

        return layer;
    }

    private static ILayer CreateLineLayer()
    {
        return new MemoryLayer
        {
            Name = LineLayerName,
            Features = new[] { CreateLineFeature() },
            Style = null,
            IsMapInfoLayer = true
        };
    }

    private static GeometryFeature CreateMultiPolygonFeature()
    {
        var feature = new GeometryFeature
        {
            Geometry = CreateMultiPolygon(),
            ["Name"] = "Multipolygon 1"
        };
        feature.Styles.Add(new VectorStyle { Fill = new Brush(Color.Gray), Outline = new Pen(Color.Black) });
        return feature;
    }

    private static GeometryFeature CreatePolygonFeature()
    {
        var feature = new GeometryFeature
        {
            Geometry = CreatePolygon(),
            ["Name"] = "Polygon 1"
        };
        feature.Styles.Add(new VectorStyle());
        return feature;
    }

    private static GeometryFeature CreateLineFeature()
    {
        return new GeometryFeature
        {
            Geometry = CreateLine(),
            ["Name"] = "Line 1",
            Styles = new List<IStyle> { new VectorStyle { Line = new Pen(Color.Violet, 6) } }
        };
    }

    private static MultiPolygon CreateMultiPolygon()
    {
        return new MultiPolygon(new[] {
            new Polygon(new LinearRing(new[] {
                new Coordinate(4000000, 3000000),
                new Coordinate(4000000, 2000000),
                new Coordinate(3000000, 2000000),
                new Coordinate(3000000, 3000000),
                new Coordinate(4000000, 3000000)
            })),

            new(new LinearRing(new[] {
                new Coordinate(4000000, 5000000),
                new Coordinate(4000000, 4000000),
                new Coordinate(3000000, 4000000),
                new Coordinate(3000000, 5000000),
                new Coordinate(4000000, 5000000)
            }))
        });
    }

    private static Polygon CreatePolygon()
    {
        return new Polygon(new LinearRing(new[]
        {
            new Coordinate(1000000, 1000000),
            new Coordinate(1000000, -1000000),
            new Coordinate(-1000000, -1000000),
            new Coordinate(-1000000, 1000000),
            new Coordinate(1000000, 1000000)
        }));
    }

    private static LineString CreateLine()
    {
        var offsetX = -2000000;
        var offsetY = -2000000;
        var stepSize = -2000000;

        return new LineString(new[]
        {
            new Coordinate(offsetX + stepSize,      offsetY + stepSize),
            new Coordinate(offsetX + stepSize * 2,  offsetY + stepSize),
            new Coordinate(offsetX + stepSize * 2,  offsetY + stepSize * 2),
            new Coordinate(offsetX + stepSize * 3,  offsetY + stepSize * 2),
            new Coordinate(offsetX + stepSize * 3,  offsetY + stepSize * 3)
        });
    }

    private static ILayer CreateInfoLayer(MRect? envelope)
    {
        var random = new Random(7);

        return new Layer(InfoLayerName)
        {
            DataSource = RandomPointsBuilder.CreateProviderWithRandomPoints(envelope, 25, random),
            Style = CreateSymbolStyle(),
            IsMapInfoLayer = true
        };
    }

    private static SymbolStyle CreateSymbolStyle()
    {
        return new SymbolStyle
        {
            SymbolScale = 0.8,
            Fill = new Brush(new Color(213, 234, 194)),
            Outline = { Color = Color.Gray, Width = 1 }
        };
    }

    public async Task InitializeTestAsync(IMapControl mapControl)
    {
        await Task.Delay(1000).ConfigureAwait(true);
    }
}
