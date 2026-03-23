using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui.Samples.Common.Maps.Tests;

public class RightToLeftSample : ISample
{
    public string Name => "Right to Left";

    public string Category => "Tests";

    // Noto Sans Arabic covers Arabic script and is licensed under OFL.
    public const string NotoSansArabicSource = "embedded://Mapsui.Samples.Common.Resources.Fonts.NotoSansArabic-Regular.ttf";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var layer = CreateLayer();

        var map = new Map
        {
            BackColor = Color.WhiteSmoke,
        };

        map.Navigator.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2));
        map.Layers.Add(layer);

        return map;
    }

    private static MemoryLayer CreateLayer() =>
        new()
        {
            Features = CreateFeatures(),
            Name = "RTL Text",
            Style = null
        };

    private static IEnumerable<IFeature> CreateFeatures() =>
        new List<IFeature>
        {
            new PointFeature(new MPoint(-50, 50))
            {
                Styles = new List<IStyle>
                {
                    new LabelStyle
                    {
                        Text = "مرحبا بالعالم",
                        Font = { Size = 18, FontSource = NotoSansArabicSource },
                        BackColor = new Brush(Color.White),
                        ForeColor = Color.Black,
                        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                        TextAlignment = Alignment.Right,
                    }
                }
            },
            new PointFeature(new MPoint(50, -50))
            {
                Styles = new List<IStyle>
                {
                    CreatePointStyle(),
                    new CalloutStyle
                    {
                        Title = "مرحبا بالعالم",
                        TitleFont = { Size = 15, FontSource = NotoSansArabicSource },
                        TitleFontColor = Color.Black,
                        TitleTextAlignment = Alignment.Right,
                        Subtitle = "Hello مرحبا World",
                        SubtitleFont = { Size = 12, FontSource = NotoSansArabicSource },
                        SubtitleFontColor = Color.Gray,
                        SubtitleTextAlignment = Alignment.Right,
                        Type = CalloutType.Detail,
                        MaxWidth = 200,
                        Enabled = true,
                        Offset = new Offset(0, SymbolStyle.DefaultHeight * 1f),
                        BalloonDefinition = new CalloutBalloonDefinition
                        {
                            RectRadius = 10,
                            ShadowWidth = 4,
                        },
                    }
                }
            },
        };

    private static SymbolStyle CreatePointStyle() =>
        new()
        {
            Fill = new Brush { Color = Color.Gray },
            Outline = new Pen(Color.Black),
        };
}
