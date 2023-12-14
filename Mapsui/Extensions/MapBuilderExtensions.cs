using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using System;

namespace Mapsui.Extensions;

public static class MapBuilderExtensions
{
    /// <summary>
    /// Add buttons for zoom in and out to the map
    /// </summary>
    /// <param name="mapBuilder">MapBuilder to use</param>
    /// <param name="x">Center X coordinate of buttons</param>
    /// <param name="y">Center Y coordinate of buttons</param>
    /// <param name="Orientation">Orientation of the buttons</param>
    /// <returns>MapBuilder</returns>
    public static MapBuilder AddZoomButtons(this MapBuilder mapBuilder, float x, float y, Orientation orientation) 
    {
        var zoomInSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"40\" height=\"40\"><g stroke=\"#000\"><circle cx=\"20\" cy=\"20\" fill=\"#fff\" stroke-width=\"2\" r=\"18\"/><path d=\"M10 20h20M20 10v20\" stroke-width=\"5\"/></g></svg>";
        var zoomOutSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"40\" height=\"40\"><g stroke=\"#000\"><circle cx=\"20\" cy=\"20\" fill=\"#fff\" stroke-width=\"2\" r=\"18\"/><path d=\"M10 20h20\" stroke-width=\"5\"/></g></svg>";

        mapBuilder.Map.Widgets.Add(CreateButton(mapBuilder, orientation == Orientation.Horizontal ? x - 42 : x, orientation == Orientation.Vertical ? y -42 : y, zoomInSvg, 40, (s,e) => { mapBuilder.Map.Navigator.ZoomIn(500, Animations.Easing.CubicOut); e.Handled = true; }));
        mapBuilder.Map.Widgets.Add(CreateButton(mapBuilder, orientation == Orientation.Horizontal ? x + 2 : x, orientation == Orientation.Vertical ? y + 2 : y, zoomOutSvg, 40, (s, e) => { mapBuilder.Map.Navigator.ZoomOut(500, Animations.Easing.CubicOut); e.Handled = true; }));

        return mapBuilder;
    }

    /// <summary>
    /// Add a ScaleBar to the map
    /// </summary>
    /// <param name="mapBuilder">MapBuilder to use</param>
    /// <returns>MapBuilder</returns>
    public static MapBuilder AddScaleBar(this MapBuilder mapBuilder)
    {
        var scalebar = new ScaleBarWidget(mapBuilder.Map)
        {
            HorizontalAlignment = Mapsui.Widgets.HorizontalAlignment.Center, 
            VerticalAlignment = Mapsui.Widgets.VerticalAlignment.Bottom, 
            TextAlignment = Mapsui.Widgets.Alignment.Center,
        };

        mapBuilder.Map.Widgets.Add(scalebar);

        return mapBuilder;
    }

    private static ButtonWidget CreateButton(MapBuilder mapBuilder, float x, float y, string svg, int size, Action<object?, WidgetTouchedEventArgs> action)
    {
        var result = new ButtonWidget
        {
            SvgImage = svg,
            Envelope = new MRect(x, y, x + size, y + size),
            Rotation = 0,
            Enabled = true,
        };
        result.WidgetTouched += (s, e) => action(s, e);
        result.PropertyChanged += (s, e) => mapBuilder.Map.RefreshGraphics();

        return result;
    }
}
