using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.Tests;

public class EmojiSample : ISample
{
    public string Name => "Emoji";

    public string Category => "Tests";

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
            Name = "Emoji Text",
            Style = null
        };

    private static IEnumerable<IFeature> CreateFeatures() =>
        new List<IFeature>
        {
            // Label with emoji using the system (default) font — no FontSource.
            // This tests whether the renderer falls back to the emoji font for
            // glyphs not found in the primary typeface.
            new PointFeature(new MPoint(-50, 50))
            {
                Styles = new List<IStyle>
                {
                    new LabelStyle
                    {
                        Text = "Hello 🌍 World 🎉",
                        Font = { Size = 18 },
                        BackColor = new Brush(Color.White),
                        ForeColor = Color.Black,
                        HorizontalAlignment = LabelStyle.HorizontalAlignmentEnum.Center,
                    }
                }
            },
            // Callout with emoji in title and subtitle, also using the system font.
            new PointFeature(new MPoint(50, -50))
            {
                Styles = new List<IStyle>
                {
                    CreatePointStyle(),
                    new CalloutStyle
                    {
                        Title = "Hello 🌍 World",
                        TitleFont = { Size = 15 },
                        TitleFontColor = Color.Black,
                        Subtitle = "Party 🎉 and Stars ⭐",
                        SubtitleFont = { Size = 12 },
                        SubtitleFontColor = Color.Gray,
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
