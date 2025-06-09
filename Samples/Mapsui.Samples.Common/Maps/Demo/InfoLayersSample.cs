using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI;
using Mapsui.Widgets.InfoWidgets;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Demo;

public class InfoLayersSample : ISample, ISampleTest
{
    private const string _infoLayerName = "Info Layer";
    private const string _polygonLayerName = "Polygon Layer";
    private const string _lineLayerName = "Line Layer";

    public string Name => "Map Info";
    public string Category => "Demo";

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(CreateInfoLayer(map.Extent));
        map.Layers.Add(CreatePolygonLayer());
        map.Layers.Add(CreateLineLayer());

        map.Widgets.Add(new MapInfoWidget(map, l => l is not TileLayer));

        return map;
    }

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    private static MemoryLayer CreatePolygonLayer() => new()
    {
        Name = _polygonLayerName,
        Features = new List<IFeature> { CreatePolygonFeature(), CreateMultiPolygonFeature() },
        Style = null,
    };

    private static MemoryLayer CreateLineLayer() => new()
    {
        Name = _lineLayerName,
        Features = [CreateLineFeature()],
        Style = null,
    };

    private static GeometryFeature CreateMultiPolygonFeature() => new()
    {
        Geometry = CreateMultiPolygon(),
        ["Name"] = "Multipolygon 1",
        Styles = [new VectorStyle { Fill = new Brush(Color.Gray), Outline = new Pen(Color.Black) }]
    };

    private static GeometryFeature CreatePolygonFeature() => new()
    {
        Geometry = CreatePolygon(),
        ["Name"] = "Polygon 1",
        Styles = [new VectorStyle()]
    };

    private static GeometryFeature CreateLineFeature()
    {
        return new GeometryFeature
        {
            Geometry = CreateLine(),
            ["Name"] = "Line 1",
            Styles = [new VectorStyle { Line = new Pen(Color.Violet, 6) }]
        };
    }

    private static MultiPolygon CreateMultiPolygon()
    {
        return new MultiPolygon([
            new Polygon(new LinearRing([
                new Coordinate(4000000, 3000000),
                new Coordinate(4000000, 2000000),
                new Coordinate(3000000, 2000000),
                new Coordinate(3000000, 3000000),
                new Coordinate(4000000, 3000000)
            ])),

            new(new LinearRing([
                new Coordinate(4000000, 5000000),
                new Coordinate(4000000, 4000000),
                new Coordinate(3000000, 4000000),
                new Coordinate(3000000, 5000000),
                new Coordinate(4000000, 5000000)
            ]))
        ]);
    }

    private static Polygon CreatePolygon()
    {
        return new Polygon(new LinearRing(
        [
            new Coordinate(1000000, 1000000),
            new Coordinate(1000000, -1000000),
            new Coordinate(-1000000, -1000000),
            new Coordinate(-1000000, 1000000),
            new Coordinate(1000000, 1000000)
        ]));
    }

    private static LineString CreateLine()
    {
        var offsetX = -2000000;
        var offsetY = -2000000;
        var stepSize = -2000000;

        return new LineString(
        [
            new Coordinate(offsetX + stepSize,      offsetY + stepSize),
            new Coordinate(offsetX + stepSize * 2,  offsetY + stepSize),
            new Coordinate(offsetX + stepSize * 2,  offsetY + stepSize * 2),
            new Coordinate(offsetX + stepSize * 3,  offsetY + stepSize * 2),
            new Coordinate(offsetX + stepSize * 3,  offsetY + stepSize * 3)
        ]);
    }

    private static Layer CreateInfoLayer(MRect? envelope) => new(_infoLayerName)
    {
        DataSource = RandomPointsBuilder.CreateProviderWithRandomPoints(envelope, 25, new Random(7)),
        Style = CreateSymbolStyle(),
    };

    private static SymbolStyle CreateSymbolStyle() => new()
    {
        SymbolScale = 0.8,
        Fill = new Brush(new Color(213, 234, 194)),
        Outline = new Pen(Color.Gray, 1f),
    };

    public async Task InitializeTestAsync(IMapControl mapControl)
    {
        await Task.Delay(1000).ConfigureAwait(true);
    }
}
