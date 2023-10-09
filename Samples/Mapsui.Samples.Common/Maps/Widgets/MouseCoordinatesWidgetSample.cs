using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.MouseCoordinatesWidget;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class MouseCoordinatesWidgetSample : ISample
{
    public string Name => "MouseCoordinatesWidget";
    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(CreateMouseCoordinatesWidget(map, VerticalAlignment.Top, HorizontalAlignment.Left));
        map.Widgets.Add(CreateMouseCoordinatesWidget(map, VerticalAlignment.Top, HorizontalAlignment.Right));
        map.Widgets.Add(CreateMouseCoordinatesWidget(map, VerticalAlignment.Bottom, HorizontalAlignment.Right));
        map.Widgets.Add(CreateMouseCoordinatesWidget(map, VerticalAlignment.Bottom, HorizontalAlignment.Left));

        return Task.FromResult(map);
    }

    private static MouseCoordinatesWidget CreateMouseCoordinatesWidget(Map map,
        VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
    {
        return new MouseCoordinatesWidget(map)
        {
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            MarginX = 20,
            MarginY = 20,
        };
    }
}
