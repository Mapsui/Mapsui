using Mapsui.Widgets;
using Mapsui.Widgets.ButtonWidget;
using Mapsui.Widgets.ScaleBar;
using Mapsui.Widgets.Zoom;
using System;

namespace Mapsui.Extensions;
public static class MapExtensions
{
    /// <summary>
    /// Add a scale bar to the map
    /// </summary>
    /// <param name="map">Map to add scale bar</param>
    /// <returns>Map</returns>
    public static Map AddScaleBar(this Map map, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, VerticalAlignment verticalAlignment = VerticalAlignment.Bottom)
    {
        var scaleBar = new ScaleBarWidget(map)
        {
            HorizontalAlignment = horizontalAlignment, 
            VerticalAlignment = verticalAlignment, 
            TextAlignment = Mapsui.Widgets.Alignment.Center,
        };

        map.Widgets.Add(scaleBar);

        return map;
    }

    /// <summary>
    /// Add buttons for zoom in and out to map
    /// </summary>
    /// <param name="map">Map to use</param>
    /// <param name="marginX">Margin X coordinate of buttons</param>
    /// <param name="marginY">Margin Y coordinate of buttons</param>
    /// <param name="orientation">Orientation of the buttons</param>
    /// <returns>Map</returns>
    public static Map AddZoomButtons(this Map map, Orientation orientation, float marginX, float marginY, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Right, VerticalAlignment verticalAlignment = VerticalAlignment.Top)
    {
        var zoomInSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"40\" height=\"40\"><g stroke=\"#000\"><circle cx=\"20\" cy=\"20\" fill=\"#fff\" stroke-width=\"2\" r=\"18\"/><path d=\"M10 20h20M20 10v20\" stroke-width=\"5\"/></g></svg>";
        var zoomOutSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"40\" height=\"40\"><g stroke=\"#000\"><circle cx=\"20\" cy=\"20\" fill=\"#fff\" stroke-width=\"2\" r=\"18\"/><path d=\"M10 20h20\" stroke-width=\"5\"/></g></svg>";

        if (horizontalAlignment == HorizontalAlignment.Center) 
            marginX = orientation == Orientation.Horizontal ? -42.5f : -20f;
        if (verticalAlignment == VerticalAlignment.Center)
            marginY = orientation == Orientation.Vertical ? -42.5f : -20f;

        map.Widgets.Add(CreateButton(map, marginX, marginY, horizontalAlignment, verticalAlignment, zoomInSvg, 40, (s, e) => 
            { map.Navigator.ZoomIn(500, Animations.Easing.CubicOut); e.Handled = true; }));
        map.Widgets.Add(CreateButton(map, orientation == Orientation.Horizontal ? marginX + 45 : marginX, 
            orientation == Orientation.Vertical ? marginY + 45 : marginY, horizontalAlignment, verticalAlignment, zoomOutSvg, 40, (s, e) => 
            { map.Navigator.ZoomOut(500, Animations.Easing.CubicOut); e.Handled = true; }));

        return map;
    }

    private static ButtonWidget CreateButton(Map map, float marginX, float marginY, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment, string svg, int size, Action<object?, WidgetTouchedEventArgs> action)
    {
        var result = new ButtonWidget
        {
            SvgImage = svg,
            Envelope = new MRect(marginX, marginY, marginX + size, marginY + size),
            HorizontalAlignment = horizontalAlignment, 
            VerticalAlignment = verticalAlignment,
            Rotation = 0,
            Enabled = true,
        };
        result.WidgetTouched += (s, e) => action(s, e);
        result.PropertyChanged += (s, e) => map.RefreshGraphics();
        map.Navigator.ViewportChanged += (s, e) =>
        {
            if (result.HorizontalAlignment == HorizontalAlignment.Center)
            {
                result.Envelope = new MRect(map.Navigator.Viewport.Width / 2.0 - size / 2.0 + marginX, result.Envelope.MinY, map.Navigator.Viewport.Width / 2.0 + size / 2.0 + marginX, result.Envelope.MaxY);
            }
            if (result.HorizontalAlignment == HorizontalAlignment.Right)
            {
                result.Envelope = new MRect(map.Navigator.Viewport.Width - size - marginX, result.Envelope.MinY, map.Navigator.Viewport.Width - marginX, result.Envelope.MaxY);
            }
            if (result.VerticalAlignment == VerticalAlignment.Center)
            {
                result.Envelope = new MRect(result.Envelope.MinX, map.Navigator.Viewport.Height / 2.0 - size / 2.0 + marginY, result.Envelope.MaxX, map.Navigator.Viewport.Height / 2.0 + size / 2.0 + marginY);
            }
            if (result.VerticalAlignment == VerticalAlignment.Bottom)
            {
                result.Envelope = new MRect(result.Envelope.MinX, map.Navigator.Viewport.Height - size - marginY, result.Envelope.MaxX, map.Navigator.Viewport.Height -marginY);
            }
        };

        return result;
    }
}
