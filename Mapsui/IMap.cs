using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using Mapsui.Animations;
using Mapsui.Fetcher;
using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Widgets;

namespace Mapsui;

public interface IMap : IAnimatable
{
    /// <summary>
    /// List of Widgets belonging to map
    /// </summary>
    ConcurrentQueue<IWidget> Widgets { get; }

    /// <summary>
    /// Projection type of Map. Normally in format like "EPSG:3857"
    /// </summary>
    string? CRS { get; set; }

    /// <summary>
    /// A collection of layers. The first layer in the list is drawn first, the last one on top.
    /// </summary>
    LayerCollection Layers { get; }

    /// <summary>
    /// Map background color (defaults to transparent)
    ///  </summary>
    Color BackColor { get; set; }

    /// <summary>
    /// Gets the extent of the map based on the extent of all the layers in the layers collection
    /// </summary>
    /// <returns>Full map extent</returns>
    MRect? Extent { get; }

    /// <summary>
    /// List of all native resolutions of this map
    /// </summary>
    IReadOnlyList<double> Resolutions { get; }

    Action<INavigator> Home { get; set; }

    /// <summary>
    /// Called whenever a property changed
    /// </summary>
    event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// DataChanged should be triggered by any data changes of any of the child layers
    /// </summary>
    event DataChangedEventHandler DataChanged;

    /// <summary>
    /// Abort fetching of all layers
    /// </summary>
    void AbortFetch();

    /// <summary>
    /// Clear cache of all layers
    /// </summary>
    void ClearCache();

    void RefreshData(FetchInfo fetchInfo);
}
