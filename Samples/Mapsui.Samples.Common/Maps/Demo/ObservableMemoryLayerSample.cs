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
    private static readonly Color _intermediateColor = new(192, 108, 132);
    private static readonly Color _lightColor = new(248, 177, 149);
    private static readonly Color _darkColor = new(108, 91, 123);

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
            CRS = "EPSG:3857",
            BackColor = Color.LightGray,
        };
        map.Layers.Add(CreateLayerWithBackgroundSquare());
        var observableCollection = CreateNewObservableCollection(3);
        map.Layers.Add(CreateNewObservableLayer(observableCollection));
        map.Widgets.Add(new ZoomInOutWidget { Margin = new MRect(20, 40) });
        map.Widgets.Add(CreateAddPointButton(observableCollection));
        map.Widgets.Add(CreateRemovePointButton(observableCollection));
        map.Navigator.ZoomToBox(map.Extent!.Grow(2000000));
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
    }

    private static ButtonWidget CreateAddPointButton(ObservableCollection<PointFeature> observableCollection)
    {
        return new ButtonWidget
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
                observableCollection.Add(new PointFeature(count * 1_000_000, count * 1_000_000));
            }
        };
    }

    private static ObservableMemoryLayer<PointFeature> CreateNewObservableLayer(ObservableCollection<PointFeature> observableCollection)
    {
        return new ObservableMemoryLayer<PointFeature>(f => f)
        {
            Name = "Points",
            Style = new SymbolStyle
            {
                SymbolType = SymbolType.Rectangle,
                Outline = new Pen(_lightColor, 1),
                Fill = new Brush(_intermediateColor)
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
}
