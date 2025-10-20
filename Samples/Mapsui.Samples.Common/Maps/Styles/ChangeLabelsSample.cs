using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Nts.Extensions;
using Mapsui.Providers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.InfoWidgets;
using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

public class ChangeLabelsSample : ISample
{
    public string Name => "ChangeLabels";
    public string Category => "Labels";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(CreateLayerWithBackgroundSquare());

        // Local (captured) state: 0 = Uppercase, 1 = Lowercase, 2 = Number, 3 = Null
        var labelMode = 0;

        var labelStyle = CreateAlphabeticLabelStyle(() => labelMode);
        var features = CreateFeatures(RandomPointsBuilder.GenerateRandomPoints(map.Extent, 26, 9898));
        map.Layers.Add(CreatePinLayer(features, labelStyle));

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == "Pins")
        {
            HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Right
        });
        map.Widgets.Add(new ButtonWidget()
        {
            Text = "Change Labels",
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
                labelMode = (labelMode + 1) % 4;
                map.RefreshData(); // Re-evaluate labels
            },
        });

        map.Navigator.ZoomToBox(map.Extent!.Grow(2000000));
        return map;
    }

    private static LabelStyle CreateAlphabeticLabelStyle(Func<int> getLabelMode) => new()
    {
        LabelMethod = f => getLabelMode() switch
        {
            0 => f["Uppercase"]?.ToString(),
            1 => f["Lowercase"]?.ToString(),
            2 => f["Number"]?.ToString(),
            3 => null,
            _ => null
        },
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
