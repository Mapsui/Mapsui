﻿using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Tiling.Layers;
using Mapsui.UI;
using Mapsui.Widgets.InfoWidgets;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace Mapsui.Samples.Common.Maps.Performance;

public class RasterizingTileLayerWithLineStringSample : IMapControlSample
{
    public string Name => "RasterizingTileLayer with LineString";
    public string Category => "Performance";

    public void Setup(IMapControl mapControl)
    {
        // PixelDensity is not always known at startup. The RasterizingTileLayer should be initialized later.
        var pixelDensity = mapControl.GetPixelDensity() ?? 1;
        mapControl.Map = CreateMap(pixelDensity);
    }

    public static Map CreateMap(float pixelDensity)
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        var lineStringLayer = CreateLineStringLayer();
        map.Layers.Add(new RasterizingTileLayer(lineStringLayer, pixelDensity: pixelDensity));
        var extent = lineStringLayer.Extent!.Grow(lineStringLayer.Extent!.Width * 0.25);
        map.Navigator.ZoomToBox(extent);

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "LineString"));

        return map;
    }

    private static ILayer CreateLineStringLayer()
    {
        return new MemoryLayer
        {
            Name = "LineString",
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
            Outline = new Pen(Color.Gray, 1f),
            Line = new Pen(Color.White, 10f),
            Fill = new Brush(Color.White)
        };

        var vs = new VectorStyle
        {
            Outline = null,
            Line = new Pen(Color.Red, 5f),
            Fill = new Brush(Color.White)
        };

        feature.Styles.Add(vsout);
        feature.Styles.Add(vs);
    }
}
