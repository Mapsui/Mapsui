using Mapsui.Features;
using Mapsui.Layers;
using Mapsui.Styles;
using System.Collections.Concurrent;

namespace Mapsui.Extensions;

/// <summary>
/// Extensions for MemoryLayer
/// </summary>
public static class MarkerLayerExtensions
{
    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    public static MarkerLayer AddMarker(this MarkerLayer layer, double x, double y, MarkerType type = MarkerType.Pin, string? title = null, Styles.Color? color = null, byte[]? icon = null, string? svg = null, double scale = 1.0, Offset? anchor = null, Offset? calloutAnchor = null)
    {
        var marker = new Marker(x, y);

        marker.MarkerType = type;
        marker.Scale = scale;
        marker.Title = title;
        if (anchor != null) marker.Anchor = anchor;
        if (calloutAnchor != null) marker.CalloutAnchor = calloutAnchor;
        if (color != null) marker.Color = color;
        if (icon != null) { marker.MarkerType = MarkerType.Icon; marker.Icon = icon; }
        if (svg != null) { marker.MarkerType = MarkerType.Svg; marker.Svg = svg; }

        ((ConcurrentBag<IFeature>)layer.Features).Add(marker);

        return layer;
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    public static MarkerLayer AddMarker(this MarkerLayer layer, (double x, double y) position, MarkerType type = MarkerType.Pin, string? title = null, Styles.Color? color = null, byte[]? icon = null, string? svg = null, double scale = 1.0, Offset? anchor = null, Offset? calloutAnchor = null)
    {
        return AddMarker(layer, position.x, position.y, type, title, color, icon, svg, scale, anchor, calloutAnchor);
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="point">Point for position</param>
    public static MarkerLayer AddMarker(this MarkerLayer layer, MPoint position, MarkerType type = MarkerType.Pin, string? title = null, Styles.Color? color = null, byte[]? icon = null, string? svg = null, double scale = 1.0, Offset? anchor = null, Offset? calloutAnchor = null)
    {
        return AddMarker(layer, position.X, position.Y, type, title, color, icon, svg, scale, anchor, calloutAnchor);
    }
}
