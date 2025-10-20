using Mapsui.Experimental.Layers;
using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using NetTopologySuite.Geometries;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Special;

public class ObservableCollectionLayerSample : ISample
{
    private static readonly Color _darkColor = new(231, 111, 81);
    private static readonly Color _intermediateColor = new(234, 152, 87);
    private static readonly Color _lightColor = new(253, 216, 126);

    public string Name => $"{nameof(ObservableCollectionLayer<BusStop>)}";
    public string Category => "Special";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        // The busStops ObservableCollection represents something in your app that is mostly unrelated to mapping but
        // that you want to display in the map. Perhaps it was already in your app before you added Mapsui.
        var busStops = CreateObservableCollectionOfBusStops(3);

        var map = new Map
        {
            CRS = "EPSG:3857",
            BackColor = Color.LightGray,
        };
        map.Layers.Add(CreateBackgroundWithGraySquare());

        // The function to create a feature is the link from your BusStop to a Mapsui feature.
        map.Layers.Add(CreateObservableCollectionLayer(busStops, (b) => new PointFeature(b.X, b.Y)));
        map.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });
        map.Navigator.ZoomToBox(map.Extent!.Grow(10000000));

        // For this sample we need Mapsui buttons but in your app adding and removing could be done elsewhere.
        map.Widgets.Add(CreateAddBusStopButton(busStops));
        map.Widgets.Add(CreateRemoveBusStopButton(busStops));

        return map;
    }

    private static ButtonWidget CreateRemoveBusStopButton(ObservableCollection<BusStop> observableCollection) => new()
    {
        Text = "Remove point",
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new MRect(10, 10),
        Padding = new MRect(6, 6),
        BackColor = _intermediateColor,
        TextColor = Color.WhiteSmoke,
        TextSize = 16,
        CornerRadius = 4,
        WithTappedEvent = (s, e) =>
        {
            if (observableCollection.Count == 0)
            {
                Logger.Log(LogLevel.Information, "No more points to remove");
                return;
            }
            observableCollection.RemoveAt(observableCollection.Count - 1);
        }
    };

    private static ButtonWidget CreateAddBusStopButton(ObservableCollection<BusStop> observableCollection) => new()
    {
        Text = "Add point",
        HorizontalAlignment = HorizontalAlignment.Left,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new MRect(10, 48),
        Padding = new MRect(6, 6),
        BackColor = _intermediateColor,
        TextColor = Color.WhiteSmoke,
        TextSize = 16,
        CornerRadius = 4,
        WithTappedEvent = (s, e) =>
        {
            var count = observableCollection.Count;
            observableCollection.Add(new BusStop("BusStop", count * 1_000_000, count * 1_000_000));
        }
    };

    private static ObservableCollectionLayer<BusStop> CreateObservableCollectionLayer(
        ObservableCollection<BusStop> observableCollection, Func<BusStop, IFeature?> createFeature) => new(createFeature)
        {
            Name = "BusStops",
            Style = new SymbolStyle
            {
                SymbolType = SymbolType.Rectangle,
                Outline = new Pen(_lightColor, 1),
                Fill = new Brush(_intermediateColor)
            },
            ObservableCollection = observableCollection,
        };

    private static ObservableCollection<BusStop> CreateObservableCollectionOfBusStops(int pointCount)
    {
        var busStops = new ObservableCollection<BusStop>();
        for (var i = 0; i < pointCount; i++)
            busStops.Add(new BusStop("BusStop", i * 1_000_000, i * 1_000_000));
        return busStops;
    }

    public static Layer CreateBackgroundWithGraySquare() => new("Background")
    {
        DataSource = new MemoryProvider(CreateSquarePolygon(5000000).ToFeature()),
        Style = new VectorStyle
        {
            Fill = new Brush(_darkColor),
            Outline = null,
        }
    };

    private static Polygon CreateSquarePolygon(int halfWidth) => new(new LinearRing(new[]
{
        new Coordinate(-halfWidth, -halfWidth),
        new Coordinate(-halfWidth, halfWidth),
        new Coordinate(halfWidth, halfWidth),
        new Coordinate(halfWidth, -halfWidth),
        new Coordinate(-halfWidth, -halfWidth) // Closing the ring
    }));

    // This is some class in your app that you want to visualize in the map.
    // It has no dependency on Mapsui and that is no problem.
    public class BusStop(string name, double x, double y)
    {
        public string Name { get; } = name;
        public double X { get; } = x;
        public double Y { get; } = y;
    }
}
