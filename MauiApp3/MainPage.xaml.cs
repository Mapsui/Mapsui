using Mapsui;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.Widgets.InfoWidgets;
using NetTopologySuite.Geometries;
using Color = Mapsui.Styles.Color;
using Map = Mapsui.Map;

namespace MauiApp3;

public partial class MainPage : ContentPage
{
    int count = 0;

    public MainPage()
    {
        InitializeComponent();
        mapControl.Map = CreateMap();
    }

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
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
        Name = "PolygonLayer",
        Features = new List<IFeature> { CreatePolygonFeature(), CreateMultiPolygonFeature() },
        Style = null,
    };

    private static MemoryLayer CreateLineLayer() => new()
    {
        Name = "LineLayer",
        Features = [CreateLineFeature()],
        Style = null,
    };

    private static GeometryFeature CreateMultiPolygonFeature() => new()
    {
        Geometry = CreateMultiPolygon(),
        ["Name"] = "Multipolygon 1",
        Styles = [new VectorStyle { Fill = new Mapsui.Styles.Brush(Color.Gray), Outline = new Pen(Color.Black) }]
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

    private void mapControl_Info(object sender, MapInfoEventArgs e)
    {
        mainLabel.Text = $"Hello, World! Count: {++count}";
    }
}

