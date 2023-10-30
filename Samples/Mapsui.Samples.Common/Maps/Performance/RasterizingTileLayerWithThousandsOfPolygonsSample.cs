using System;
using System.Collections.Generic;
using System.Linq;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Nts.Providers;
using Mapsui.Projections;
using Mapsui.Providers;
using Mapsui.Rendering;
using Mapsui.Rendering.Skia;
using Mapsui.Rendering.Skia.Cache;
using Mapsui.Styles;
using Mapsui.Tiling.Layers;
using Mapsui.UI;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
using NetTopologySuite.Geometries;

#pragma warning disable IDISP001 // Dispose created
#pragma warning disable IDISP004 // Don't ignore created IDisposable

namespace Mapsui.Samples.Common.Maps.Performance;

public sealed class RasterizingTileLayerWithThousandsOfPolygonsSample : IMapControlSample, IDisposable
{
    private Map? _map;
    public string Name => "RasterizingTileLayer with Thousands of Polygons";
    public string Category => "Performance";

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = CreateMap();
    }

    public Map CreateMap()
    {
        DefaultRendererFactory.Create = () => new MapRenderer(new RenderCache(900000));
        _map?.Dispose();
        _map = new Map();
        _map.Layers.Add(Tiling.OpenStreetMap.CreateTileLayer());
        _map.Layers.Add(new RasterizingTileLayer(CreatePolygonLayer()));
        var home = Mercator.FromLonLat(0, 0);
        _map.Home = n => n.CenterOnAndZoomTo(home, _map.Navigator.Resolutions[9]);
        var buttonWidget = new ButtonWidget
        {
            Text = "Change Color",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top
        };
        buttonWidget.WidgetTouched += ChangeColor;
        _map.Widgets.Enqueue(buttonWidget);

        return _map;
    }

    private void ChangeColor(object? sender, WidgetTouchedEventArgs e)
    {
        var layer = (_map?.Layers)?.First(f => f is RasterizingTileLayer) as RasterizingTileLayer;
        var random = new Random();
        // random color
        Color color = new Color(random.Next(255), random.Next(255), random.Next(255));
        layer!.SourceLayer.Style = new VectorStyle
        {
            Fill = new Brush(color),
        };
        layer.ClearCache();
    }

    public static ILayer CreatePolygonLayer()
    {
        return new Layer("Polygons")
        {
            DataSource = new IndexedMemoryProvider(CreatePolygon().ToFeatures()),
            Style = new VectorStyle
            {
                Fill = new Brush(Color.Red),
            }
        };
    }

    private static List<Polygon> CreatePolygon()
    {
        var result = new List<Polygon>();

        Polygon polygon1;
        int factor = 0;

        for (int i = 0; i < 900000; i++)
        {
            factor = i - 100*(int)Math.Round((double)(i / 100));
            polygon1 = new Polygon(
                new LinearRing(new[] {
                    new Coordinate(1000*(factor-1), 1000*(factor-1)-(Math.Round((double)(i/100))*1000)),
                    new Coordinate(1000*(factor-1), 1000*(factor)-(Math.Round((double)(i/100))*1000)),
                    new Coordinate(1000*(factor), 1000*(factor)-(Math.Round((double)(i/100))*1000)),
                    new Coordinate(1000*(factor), 1000*(factor-1)-(Math.Round((double)(i/100))*1000)),
                    new Coordinate(1000*(factor-1), 1000*(factor-1)-(Math.Round((double)(i/100))*1000))
                }));
            
            result.Add(polygon1);
        }
        return result;
    }

    public void Dispose()
    {
        _map?.Dispose();
    }
}
