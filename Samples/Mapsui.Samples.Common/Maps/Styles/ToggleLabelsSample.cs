using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Samples.Common.DataBuilders;
using Mapsui.Styles;
using Mapsui.Styles.Thematics;
using Mapsui.Tiling;
using Mapsui.Widgets.ButtonWidgets;
using Mapsui.Widgets.InfoWidgets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Styles;

public class ToggleLabelsSample : ISample
{
    public string Name => "Toggle Labels";
    public string Category => "1";

    private const string _layerName = "My Layer";

    public Task<Map> CreateMapAsync()
    {
        return Task.FromResult(CreateMap());
    }

    public static Map CreateMap()
    {
        var map = new Map();
        map.Layers.Add(OpenStreetMap.CreateTileLayer());

        var points = RandomPointsBuilder.GenerateRandomPoints(map.Extent, 26, 9898);

        var themeStyle = new ThemeStyle((f) =>
        {
            return new LabelStyle
            {
                Text = f["label"]?.ToString() ?? string.Empty,
                Offset = new Offset(20, -56),
                Font = new Font { Size = 32 },
                BorderThickness = 1,
                BorderColor = Color.DimGray,
            };
        });

        map.Layers.Add(CreateLayer(CreateFeatures(points), themeStyle));

        map.Widgets.Add(new MapInfoWidget(map, l => l.Name == _layerName));
        map.Widgets.Add(new ButtonWidget()
        {
            Text = "Toggle Labels",
            Margin = new MRect(10),
            CornerRadius = 3,
            BackColor = new Color(0, 123, 255),
            TextColor = Color.White,
            Padding = new MRect(4),
            VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom,
            HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Left,
            WithTappedEvent = (s, e) =>
            {
                themeStyle.Enabled = !themeStyle.Enabled;
            },

        });

        return map;
    }



    private static MemoryLayer CreateLayer(IEnumerable<IFeature> features, ThemeStyle themeStyle) => new()
    {
        Name = _layerName,
        Features = features,
        Style = new StyleCollection
        {
            Styles = {
                CreatePinSymbol(),
                themeStyle,
            },
        },
    };

    private static List<IFeature> CreateFeatures(IEnumerable<MPoint> randomPoints)
    {
        var features = new List<IFeature>();
        var i = 0;

        foreach (var point in randomPoints)
        {
            var feature = new PointFeature(point);
            // Assign A..Z in sequence to the "label" field
            feature["label"] = ((char)('A' + (i % 26))).ToString();
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
}
