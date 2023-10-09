using Mapsui.Extensions;
using Mapsui.Styles;
using Mapsui.Tiling;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class CustomWidgetSample : ISample
{
    public string Name => "Custom Widget";

    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new CustomWidget.CustomWidget
        {
            MarginX = 20,
            MarginY = 20,
            Width = 100,
            Height = 20,
            Color = Color.GoldenRod
        });

        return Task.FromResult(map);
    }
}
