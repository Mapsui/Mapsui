using Mapsui.Layers;
using Mapsui.Styles;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mapsui.Extensions;

public static class MemoryLayerExtensions
{
    /// <summary>
    /// Add a marker to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="color">Color of marker</param>
    /// <param name="scale">Scale of marker</param>
    /// <param name="title">Title for callout</param>
    /// <param name="subtitle">Subtitle for callout</param>
    /// <param name="touched">Action called when marker is touched</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, double x, double y, Color? color = null, double scale = 1.0, string? title = null, string? subtitle = null, Action<ILayer, IFeature, MapInfoEventArgs>? touched = null)
    {
        var marker = new PointFeature(x, y);

        marker.InitMarker(color, scale, title, subtitle, touched);

        ((ConcurrentBag<IFeature>)layer.Features).Add(marker);

        return layer;
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="color">Color of marker</param>
    /// <param name="scale">Scale of marker</param>
    /// <param name="title">Title for callout</param>
    /// <param name="subtitle">Subtitle for callout</param>
    /// <param name="touched">Action called when marker is touched</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, (double x, double y) position, Color? color = null, double scale = 1.0, string? title = null, string? subtitle = null, Action<ILayer, IFeature, MapInfoEventArgs>? touched = null)
    {
        return AddMarker(layer, position.x, position.y, color, scale, title, subtitle, touched);
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="position">Point for position</param>
    /// <param name="color">Color of marker</param>
    /// <param name="scale">Scale of marker</param>
    /// <param name="title">Title for callout</param>
    /// <param name="subtitle">Subtitle for callout</param>
    /// <param name="touched">Action called when marker is touched</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, MPoint position, Color? color = null, double scale = 1.0, string? title = null, string? subtitle = null, Action<ILayer, IFeature, MapInfoEventArgs>? touched = null)
    {
        return AddMarker(layer, position.X, position.Y, color, scale, title, subtitle, touched);
    }

    /// <summary>
    /// Hide all callouts on this layer
    /// </summary>
    /// <param name="layer"></param>
    public static void HideAllCallouts(this MemoryLayer layer)
    {
        foreach (var m in layer.Features.Where(f => f.Fields.Contains(PointFeatureExtensions.MarkerKey) && ((PointFeature)f).HasCallout()))
            ((PointFeature)m).HideCallout();
    }
}
