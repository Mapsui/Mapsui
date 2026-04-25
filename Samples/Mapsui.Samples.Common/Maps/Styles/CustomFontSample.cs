using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.Styles;

public class CustomFontSample : ISample
{
    public string Name => "Custom Font";
    public string Category => "Styles";

    // FontSource URI for the OpenSans-Regular font embedded in Mapsui.Samples.Common
    public const string OpenSansSource = "embedded://Mapsui.Samples.Common.Resources.Fonts.OpenSans-Regular.ttf";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var map = new Map { BackColor = Color.WhiteSmoke };
        map.Layers.Add(CreateLayer());
        map.Navigator.ZoomToBox(map.Layers.First().Extent!.Grow(map.Layers.First().Extent!.Width));
        return map;
    }

    private static MemoryLayer CreateLayer() => new()
    {
        Name = "Custom Font Labels",
        Features = CreateFeatures(),
        Style = null,
    };

    private static IEnumerable<IFeature> CreateFeatures() =>
    [
        new PointFeature(new MPoint(-100, 50)) {
            Styles =
            [
                new LabelStyle
                {
                    Text = "System font",
                    Font = { Size = 18 },
                    BackColor = new Brush(Color.White),
                    ForeColor = Color.Black,
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                }
            ]
        },
        new PointFeature(new MPoint(100, 50)) {
            Styles =
            [
                new LabelStyle
                {
                    Text = "Custom font",
                    Font = { Size = 18, FontSource = OpenSansSource },
                    BackColor = new Brush(Color.LightBlue),
                    ForeColor = Color.Black,
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                }
            ]
        },
        new PointFeature(new MPoint(-100, -50)) {
            Styles =
            [
                new LabelStyle
                {
                    Text = "Bold system font",
                    Font = { Size = 18, Bold = true },
                    BackColor = new Brush(Color.White),
                    ForeColor = Color.Black,
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                }
            ]
        },
        new PointFeature(new MPoint(100, -50)) {
            Styles =
            [
                new LabelStyle
                {
                    Text = "Custom font wrap around to show multiple long words",
                    Font = { Size = 14, FontSource = OpenSansSource },
                    BackColor = new Brush(Color.LightBlue),
                    ForeColor = Color.Black,
                    MaxWidth = 10,
                    WordWrap = LabelStyle.LineBreakMode.WordWrap,
                    HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                }
            ]
        },
    ];
}
