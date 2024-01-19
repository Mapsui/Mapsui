using Mapsui.Features;
using Mapsui.Layers;
using Mapsui.Styles;
using System.Collections.Concurrent;

namespace Mapsui.Extensions;

/// <summary>
/// Extensions for MemoryLayer
/// </summary>
public static class MemoryLayerExtensions
{
    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, double x, double y, MarkerType type = MarkerType.Pin_Circle, string? title = null, string? subtitle = null, Styles.Color? color = null, double scale = 1.0, Offset? anchor = null, Offset? calloutAnchor = null)
    {
        var marker = new Marker(x, y, type);

        marker.MarkerType = type;
        marker.Scale = scale;
        marker.Title = title;
        marker.Subtitle = subtitle;
        if (anchor != null) marker.Anchor = anchor;
        if (calloutAnchor != null) marker.CalloutAnchor = calloutAnchor;
        if (color != null) marker.Color = color;

        ((ConcurrentBag<IFeature>)layer.Features).Add(marker);

        return layer;
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, (double x, double y) position, MarkerType type = MarkerType.Pin_Circle, string? title = null, string? subtitle = null, Styles.Color? color = null, double scale = 1.0, Offset? anchor = null, Offset? calloutAnchor = null)
    {
        return AddMarker(layer, position.x, position.y, type, title, subtitle, color, scale, anchor, calloutAnchor);
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="point">Point for position</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, MPoint position, MarkerType type = MarkerType.Pin_Circle, string? title = null, string? subtitle = null, Styles.Color? color = null, double scale = 1.0, Offset? anchor = null, Offset? calloutAnchor = null)
    {
        return AddMarker(layer, position.X, position.Y, type, title, subtitle, color, scale, anchor, calloutAnchor);
    }
}
