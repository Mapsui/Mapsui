using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Nts.Providers;
using Mapsui.Projections;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Tiling.Fetcher;
using Mapsui.Tiling.Layers;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using NetTopologySuite.Geometries;

namespace Mapsui.Samples.Common.Maps.Performance;

public sealed class RasterizingTileLayerWithThousandsOfPolygonsSample : ISample, IDisposable
{
    private Map? _map;

    public string Name => "RasterizingTileLayerWithThousandsOfPolygons";
    public string Category => "Performance";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public Map CreateMap()
    {
        _map?.Dispose();
        _map = new Map();
        _map.RenderService = new RenderService(900000);
        _map.Layers.Add(Tiling.OpenStreetMap.CreateTileLayer());
        _map.Layers.Add(new RasterizingTileLayer(CreatePolygonLayer(), dataFetchStrategy: new MinimalDataFetchStrategy()));
        var home = SphericalMercator.FromLonLat(0, 0).ToMPoint();
        _map.Navigator.CenterOnAndZoomTo(home, _map.Navigator.Resolutions[9]);
        _map.Widgets.Enqueue(new ButtonWidget
        {
            Text = "Change Color",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Top,
            WithTappedEvent = ChangeColor
        });

        return _map;
    }

    private void ChangeColor(object? sender, WidgetEventArgs e)
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
        int factor;

        for (int i = 0; i < 900000; i++)
        {
            factor = i - 100 * (int)Math.Round((double)(i / 100));
            polygon1 = new Polygon(
                new LinearRing([
                    new Coordinate(1000 * (factor - 1), 1000 * (factor - 1) - (Math.Round((double)(i / 100)) * 1000)),
                    new Coordinate(1000 * (factor - 1), 1000 * (factor) - (Math.Round((double)(i / 100)) * 1000)),
                    new Coordinate(1000 * (factor), 1000 * (factor) - (Math.Round((double)(i / 100)) * 1000)),
                    new Coordinate(1000 * (factor), 1000 * (factor - 1) - (Math.Round((double)(i / 100)) * 1000)),
                    new Coordinate(1000 * (factor - 1), 1000 * (factor - 1) - (Math.Round((double)(i / 100)) * 1000))
                ]));

            result.Add(polygon1);
        }
        return result;
    }

    public void Dispose()
    {
        _map?.Dispose();
    }
}
