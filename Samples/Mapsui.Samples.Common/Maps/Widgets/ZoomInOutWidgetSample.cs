﻿using Mapsui.Extensions;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidgets;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Widgets;

public class ZoomInOutWidgetSample : ISample
{
    public string Name => "ZoomInOutWidget";
    public string Category => "Widgets";

    public Task<Map> CreateMapAsync()
    {
        var map = new Map();

        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(CreateZoomInOutWidget(Orientation.Vertical, VerticalAlignment.Top, HorizontalAlignment.Left));
        map.Widgets.Add(CreateZoomInOutWidget(Orientation.Horizontal, VerticalAlignment.Top, HorizontalAlignment.Right));
        map.Widgets.Add(CreateZoomInOutWidget(Orientation.Vertical, VerticalAlignment.Bottom, HorizontalAlignment.Right));
        map.Widgets.Add(CreateZoomInOutWidget(Orientation.Horizontal, VerticalAlignment.Bottom, HorizontalAlignment.Left));

        _ = Task.Run(async () =>
        {
            await Task.Delay(2000);
            map.Navigator.PanLock = true;
            map.Navigator.RotationLock = true;
        });

        return Task.FromResult(map);
    }

    private static ZoomInOutWidget CreateZoomInOutWidget(Orientation orientation,
        VerticalAlignment verticalAlignment, HorizontalAlignment horizontalAlignment)
    {
        return new ZoomInOutWidget
        {
            Orientation = orientation,
            VerticalAlignment = verticalAlignment,
            HorizontalAlignment = horizontalAlignment,
            Margin = new MRect(20),
        };
    }
}
