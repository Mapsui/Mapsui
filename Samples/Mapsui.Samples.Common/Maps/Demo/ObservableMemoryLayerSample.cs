using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Logging;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using Mapsui.Styles;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using NetTopologySuite.Geometries;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Demo;

public class ObservableMemoryLayerSample : ISample
{
    // Add these color definitions at the top of the class
    private static readonly Color _green = new(60, 180, 75);
    private static readonly Color _red = new(230, 50, 50);

    public string Name => $"{nameof(ObservableMemoryLayer<PointFeature>)}";
    public string Category => "1";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        var map = new Map
        {
            CRS = "EPSG:3857"
        };
        map.Layers.Add(CreateLayerWithBackgroundSquare());
        var observableCollection = CreateNewObservableCollection(3);
        map.Layers.Add(CreateNewObservableLayer(observableCollection));
        map.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });
        map.Widgets.Add(CreateAddPointButton(observableCollection));
        map.Widgets.Add(CreateRemovePointButton(observableCollection));
        return map;
    }

    // Update CreateRemovePointButton to use WarmRed
    private static ButtonWidget CreateRemovePointButton(ObservableCollection<PointFeature> observableCollection)
    {
        return new ButtonWidget
        {
            Text = "Remove point",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new MRect(10, 10),
            Padding = new MRect(6, 6),
            BackColor = _red,
            TextColor = Color.White,
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
    }

    // Update CreateAddPointButton to use WarmGreen
    private static ButtonWidget CreateAddPointButton(ObservableCollection<PointFeature> observableCollection)
    {
        return new ButtonWidget
        {
            Text = "Add point",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new MRect(10, 44),
            Padding = new MRect(6, 6),
            BackColor = _green,
            TextColor = Color.White,
            TextSize = 16,
            CornerRadius = 4,
            WithTappedEvent = (s, e) =>
            {
                var count = observableCollection.Count;
                observableCollection.Add(new PointFeature(count * 1_000_000, count * 1_000_000));
            }
        };
    }

    // Update CreateNewObservableLayer to use WarmGreen for the point color
    private static ObservableMemoryLayer<PointFeature> CreateNewObservableLayer(ObservableCollection<PointFeature> observableCollection)
    {
        return new ObservableMemoryLayer<PointFeature>(f => f)
        {
            Name = "Points",
            Style = new SymbolStyle
            {
                SymbolType = SymbolType.Triangle,
                SymbolScale = 1.5,
                Outline = new Pen(Color.White, 1),
                Fill = new Brush(_green)
            },
            ObservableCollection = observableCollection,
        };
    }

    private static ObservableCollection<PointFeature> CreateNewObservableCollection(int pointCount)
    {
        var collection = new ObservableCollection<PointFeature>();
        for (int i = 0; i < pointCount; i++)
            collection.Add(new PointFeature(i * 1_000_000, i * 1_000_000));
        return collection;
    }

    public static Layer CreateLayerWithBackgroundSquare() => new("Background")
    {
        DataSource = new MemoryProvider(CreateSquarePolygon(5000000).ToFeature()),
        Style = new VectorStyle
        {
            Fill = new Brush(Color.LightGray),
            Outline = new Pen
            {
                Color = Color.DimGray,
                Width = 1,
            }
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
}
