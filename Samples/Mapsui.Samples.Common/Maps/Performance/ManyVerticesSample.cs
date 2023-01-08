using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable CS8670 // Object or collection initializer implicitly dereferences possibly null member.
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Performance;

public class ManyVerticesSample : IMapControlSample
{
    public string Name => "Many Vertices";
    public string Category => "Performance";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap(mapControl.PixelDensity);
    }

    public static Map CreateMap(float pixelDensity)
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new RasterizingLayer(CreatePointLayer(), pixelDensity: pixelDensity));
        var extent = map.Layers[1].Extent!.Grow(map.Layers[1].Extent!.Width * 0.25);
        map.Home = n => n.NavigateTo(extent);
        return map;
    }

    private static ILayer CreatePointLayer()
    {
        return new MemoryLayer
        {
            Name = "Points",
            IsMapInfoLayer = true,
            Features = new List<IFeature>() { GetFeature() }
        };
    }

    private static IFeature GetFeature()
    {
        var lineString = CreateLineStringWithManyVertices();
        var feature = new GeometryFeature();
        AddStyles(feature);
        feature.Geometry = lineString;
        feature["Name"] = $"LineString with {lineString.Coordinates.Length} vertices";
        return feature;
    }

    private static LineString CreateLineStringWithManyVertices()
    {
        var startPoint = new Coordinate(1623484, 7652571);

        var points = new List<Coordinate>();

        for (var i = 0; i < 10000; i++)
        {
            points.Add(new Coordinate(startPoint.X + i, startPoint.Y + i));
        }

        return new LineString(points.ToArray());
    }

    private static void AddStyles(IFeature feature)
    {
        // route outline style
        var vsout = new VectorStyle
        {
            Opacity = 0.5f,
            Line = new Pen(Color.White, 10f),
        };

        var vs = new VectorStyle
        {
            Fill = null,
            Outline = null,
            Line = { Color = Color.Red, Width = 5f }
        };

        feature.Styles.Add(vsout);
        feature.Styles.Add(vs);
    }
}
