using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Styles;

namespace Mapsui.Samples.Common.Maps.Tests;

public class CalloutWrapAroundSample : ISample
{
    public string Name => "Callout with wrap around";
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
            Name = "Callouts",
            Style = null
        };

    private static IEnumerable<IFeature> CreateFeatures() =>
        new List<IFeature>()
        {
            new PointFeature(new MPoint(-50, 50)) {
                Styles = new List<IStyle>
                {
                    CreatePointStyle(),
                    CreateCalloutStyle(
                        "Word Wrap Demo",
                        "This is a longer subtitle that should wrap around to multiple lines within the callout balloon")
                }
            },
            new PointFeature(new MPoint(50, -50)) {
                Styles = new List<IStyle>
                {
                    CreatePointStyle(),
                    CreateCalloutStyle(
                        "CJK and mixed text",
                        "Hello 你好世界 this mixes Latin and CJK characters for line breaking")
                }
            },
        };

    private static SymbolStyle CreatePointStyle() =>
        new()
        {
            Fill = new Brush { Color = Color.Gray },
            Outline = new Pen(Color.Black),
        };

    private static CalloutStyle CreateCalloutStyle(string title, string subtitle) =>
        new()
        {
            Title = title,
            TitleFont = { FontFamily = null, Size = 15, Italic = false, Bold = true },
            TitleFontColor = Color.Black,

            Subtitle = subtitle,
            SubtitleFont = { FontFamily = null, Size = 12, Italic = false, Bold = true },
            SubtitleFontColor = Color.Gray,

            Type = CalloutType.Detail,
            MaxWidth = 120,
            Enabled = true,
            Offset = new Offset(0, SymbolStyle.DefaultHeight * 1f),
            BalloonDefinition = new CalloutBalloonDefinition
            {
                RectRadius = 10,
                ShadowWidth = 4,
            },
        };
}
