using Mapsui.Features;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapsui.Layers;
public class MarkerLayer : MemoryLayer
{
    /// <summary>
    /// Create a new layer
    /// </summary>
    public MarkerLayer() : this(nameof(MemoryLayer)) { }

    /// <summary>
    /// Create layer with name
    /// </summary>
    /// <param name="layerName">Name to use for layer</param>
    public MarkerLayer(string layerName) : base(layerName) 
    {
        Style = null;
        IsMapInfoLayer = true;
    }

    public override IEnumerable<IFeature> GetFeatures(MRect? rect, double resolution)
    {
        var resultWithCallout = new List<IFeature>();
        var resultWithoutCallout = new List<IFeature>();

        foreach (var f in base.GetFeatures(rect, resolution))
        {
            if (f is Marker m && m.HasCallout)
                resultWithCallout.Add(f);
            else
                resultWithoutCallout.Add(f);
        }

        // TODO: Change if drawing order is corrected, so that last drawn feature is the topmost
        // Get the feature with callout style enabled as first, so that it is drawn topmost
        return resultWithoutCallout.Union(resultWithCallout);
    }

    internal void HandleInfo(object? sender, MapInfoEventArgs args)
    {
        {
            if (args.MapInfo?.Feature == null || args.MapInfo.Feature is not Marker marker) return;

            var hasCallout = marker.HasCallout;

            foreach (var m in Features.Where(f => f is Marker && ((Marker)f).HasCallout))
                ((Marker)m).HideCallout();

            if (!hasCallout)
                marker.ShowCallout();

            DataHasChanged();
        };
    }
}
