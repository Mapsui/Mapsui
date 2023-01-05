using Mapsui.Extensions;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class CustomWidgetSample : ISample
{
    public string Name => "3 Custom Widget";

    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new CustomWidget.CustomWidget
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            MarginX = 20,
            MarginY = 20,
            Width = 100,
            Height = 20,
            Color = Color.FromString(Color.KnownColors["goldenrod"])
        });

        return Task.FromResult(map);
    }
}
