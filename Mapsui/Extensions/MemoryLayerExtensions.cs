using Mapsui.Features;
using Mapsui.Layers;
using Mapsui.Widgets.ScaleBar;
using System.Collections.Concurrent;
using System.Drawing;
using System.Reflection.Metadata;

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
    public static MemoryLayer AddMarker(this MemoryLayer layer, double x, double y, MarkerType type = MarkerType.Pin, Styles.Color? color = null, byte[]? icon = null, string? svg = null, double scale = 1.0)
    {
        var marker = new Marker(x, y);

        marker.MarkerType = type;
        marker.Scale = scale;
        if (color != null) marker.Color = color;
        if (icon != null) marker.Icon = icon;
        if (svg != null) marker.Svg = svg;

        ((ConcurrentBag<IFeature>)layer.Features).Add(marker);

        return layer;
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, (double x, double y) position, MarkerType type = MarkerType.Pin, Styles.Color? color = null, byte[]? icon = null, string? svg = null, double scale = 1.0)
    {
        return AddMarker(layer, position.x, position.y, type, color, icon, svg, scale);
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="point">Point for position</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, MPoint position, MarkerType type = MarkerType.Pin, Styles.Color? color = null, byte[]? icon = null, string? svg = null, double scale = 1.0)
    {
        return AddMarker(layer, position.X, position.Y, type, color, icon, svg, scale);
    }
}
