using System.Collections.Generic;
using System.Threading.Tasks;
using Mapsui.Layers;
using Mapsui.Samples.Common;
using Mapsui.Styles;

namespace Mapsui.Tests.Common.Maps;

#pragma warning disable IDISP001 // Dispose created

public class CalloutSample : ISample
{
    public string Name => "Callout";
    public string Category => "Tests";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        var layer = CreateLayer();

        var map = new Map
        {
            BackColor = Color.FromString("WhiteSmoke"),
            Home = n => n.ZoomToBox(layer.Extent!.Grow(layer.Extent.Width * 2))
        };

        map.Layers.Add(layer);

        return map;
    }

    private static MemoryLayer CreateLayer()
    {
        return new MemoryLayer
        {
            Features = CreateFeatures(),
            Name = "Callouts",
            Style = null
        };
    }

    private static IEnumerable<IFeature> CreateFeatures()
    {
        return new List<IFeature>()
        {
            new PointFeature(new MPoint(0, 50)) {
                Styles = new List<IStyle>
                {
                    CreatePointStyle(),
                    CreateCalloutStyle("Say", "Hello")
                }
            },
            new PointFeature(new MPoint(50, 0)) {
                Styles = new List<IStyle>
                {
                    CreatePointStyle(),
                    CreateCalloutStyle("Say", "World")
                }
            }
        };
    }

    private static SymbolStyle CreatePointStyle()
    {
        return new SymbolStyle
        {
            Fill = new Brush { Color = Color.Gray },
            Outline = new Pen(Color.Black),
        };
    }

    private static CalloutStyle CreateCalloutStyle(string title, string subtitle)
    {
        return new CalloutStyle
        {
            Title = title,
            TitleFont = { FontFamily = null, Size = 15, Italic = false, Bold = true },
            TitleFontColor = Color.Black,
            
            Subtitle = subtitle,
            SubtitleFont = { FontFamily = null, Size = 12, Italic = false, Bold = true },
            SubtitleFontColor = Color.Gray,

            Type = CalloutType.Detail,
            MaxWidth = 120,
            RectRadius = 10,
            ShadowWidth = 4,
            Enabled = true,
            SymbolOffset = new Offset(0, SymbolStyle.DefaultHeight * 1f)
        };
    }
}
