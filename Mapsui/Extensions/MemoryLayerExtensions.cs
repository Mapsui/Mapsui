using Mapsui.Features;
using Mapsui.Layers;
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
    public static MemoryLayer AddMarker(this MemoryLayer layer, double x, double y)
    {
        ((ConcurrentBag<IFeature>)layer.Features).Add(new Marker(x, y));

        return layer;
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, (double x, double y) position)
    {
        ((ConcurrentBag<IFeature>)layer.Features).Add(new Marker(position));

        return layer;
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="point">Point for position</param>
    public static void AddMarker(this MemoryLayer layer, MPoint position)
    {
        ((ConcurrentBag<IFeature>)layer.Features).Add(new Marker(position));
    }

}
