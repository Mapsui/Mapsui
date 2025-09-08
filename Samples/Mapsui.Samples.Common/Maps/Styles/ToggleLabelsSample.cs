using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.InfoWidgets;
using NetTopologySuite.Geometries;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

public class ToggleLabelsSample : ISample
{
    public string Name => "Toggle Labels";
    public string Category => "1";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(CreateLayerWithBackgroundSquare());

        var labelStyle = CreateAlphabetLabelStyle();
        var points = RandomPointsBuilder.GenerateRandomPoints(map.Extent, 26, 9898);
        map.Layers.Add(CreatePinLayer(CreateFeatures(points), labelStyle));

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "Pins"));
        map.Widgets.Add(new ButtonWidget()
        {
            Text = "Toggle Labels",
            TextSize = 24,
            Margin = new MRect(10),
            CornerRadius = 6,
            BackColor = new Color(204, 85, 51),
            TextColor = Color.White,
            Padding = new MRect(4),
            VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom,
            HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
            WithTappedEvent = (s, e) =>
            {
                // Currently just toggles visibility of the active (default Uppercase) label style
                labelStyle.Enabled = !labelStyle.Enabled;
            },
        });

        map.Navigator.ZoomToBox(map.Extent!.Grow(2000000));

        return map;
    }

    // Default selected column should be Uppercase
    private static LabelStyle CreateAlphabetLabelStyle() => new()
    {
        LabelColumn = "Uppercase",
        Offset = new Offset(20, -56),
        Font = new Font { Size = 32 },
        BorderThickness = 1,
        BorderColor = Color.DimGray,
    };

    private static MemoryLayer CreatePinLayer(IEnumerable<IFeature> features, LabelStyle labelStyle) => new()
    {
        Name = "Pins",
        Features = features,
        Style = new StyleCollection
        {
            Styles = {
                CreateSmallCircleSymbol(),
                CreatePinSymbol(),
                labelStyle,
            },
        },
    };

    private static SymbolStyle CreateSmallCircleSymbol() => new()
    {
        SymbolType = SymbolType.Ellipse,
        SymbolScale = 0.5,
        Outline = new Pen(new Color(8, 8, 8)),
        Fill = null,
    };

    private static List<IFeature> CreateFeatures(IEnumerable<MPoint> randomPoints)
    {
        var features = new List<IFeature>();
        var i = 0;

        foreach (var point in randomPoints)
        {
            var feature = new PointFeature(point);

            // Add three columns:
            // Uppercase: A..Z
            // lower-case: a..z (with dash in name as requested)
            // Number: 1..26
            var uppercaseChar = (char)('A' + (i % 26));
            var lowercaseChar = (char)('a' + (i % 26));
            feature["Uppercase"] = uppercaseChar.ToString();
            feature["Lowercase"] = lowercaseChar.ToString();
            feature["Number"] = (i % 26 + 1).ToString();

            i++;
            features.Add(feature);
        }
        return features;
    }

    private static ImageStyle CreatePinSymbol() => new()
    {
        Image = new Image
        {
            Source = "embedded://Mapsui.Resources.Images.pin.svg",
            SvgFillColor = Color.FromString("#4193CF"),
            SvgStrokeColor = Color.DimGray,
        },
        RelativeOffset = new RelativeOffset(0.0, 0.5), // The symbols point should be at the geolocation.
    };

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
