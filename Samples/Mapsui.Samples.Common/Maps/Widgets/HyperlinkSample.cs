using Mapsui.Extensions;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class HyperlinkSample : ISample
{
    public string Name => "Hyperlink";
    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(CreateHyperlink("Open Mapsui FAQ", "https://mapsui.com/documentation/faq.html", 
            VerticalAlignment.Top, HorizontalAlignment.Left));
        map.Widgets.Add(CreateHyperlink("Open Mapsui readme.md", "https://github.com/Mapsui/Mapsui/blob/master/README.md", 
            VerticalAlignment.Bottom, HorizontalAlignment.Right));
        map.Widgets.Add(CreateHyperlink("Open Mapsui releases page", "https://github.com/Mapsui/Mapsui/releases/", 
            VerticalAlignment.Bottom, HorizontalAlignment.Left));
        map.Widgets.Add(CreateHyperlink("Open Mapsui nuget page", "https://www.nuget.org/packages/Mapsui", 
            VerticalAlignment.Top, HorizontalAlignment.Right));

        return Task.FromResult(map);
    }

    private static IWidget CreateHyperlink(string text, string url,
        VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
    {
        return new Hyperlink()
        {
            Text = text,
            Url = url,
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            MarginX = 30,
            MarginY = 30,
            PaddingX = 4,
            PaddingY = 4,
            CornerRadius = 4,
            BackColor = new Color(23, 162, 184),
            TextColor = Color.White,
        };
    }
}
