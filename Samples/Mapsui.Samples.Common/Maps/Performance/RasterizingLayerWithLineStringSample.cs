using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.UI;
using Mapsui.Widgets.InfoWidgets;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Local
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Performance;

public class RasterizingLayerWithLineStringSample : IMapControlSample
{
    public string Name => "RasterizingLayer with LineString";
    public string Category => "Performance";

    public void Setup(IMapControl mapControl)
    {
        // PixelDensity is not always known at startup. The RasterizingLayer should be initialized later.
        var pixelDensity = mapControl.GetPixelDensity() ?? 1;
        mapControl.Map = CreateMap(pixelDensity);
    }

    public static Map CreateMap(float pixelDensity)
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Layers.Add(new RasterizingLayer(CreateLineStringLayer(), pixelDensity: pixelDensity));
        var extent = map.Layers.Get(1).Extent!.Grow(map.Layers.Get(1).Extent!.Width * 0.25);
        map.Navigator.ZoomToBox(extent);

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "LineString"));

        return map;
    }

    private static ILayer CreateLineStringLayer()
    {
        return new MemoryLayer
        {
            Name = "LineString",
            Features = [GetFeature()]
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
            Outline = null
        };

        var vs = new VectorStyle
        {
            Line = new Pen(Color.Red, 5f),
            Outline = null
        };

        feature.Styles.Add(vsout);
        feature.Styles.Add(vs);
    }
}
