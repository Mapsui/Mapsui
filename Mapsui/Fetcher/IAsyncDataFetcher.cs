// Copyright (c) The Mapsui authors.
// The Mapsui authors licensed this file under the MIT license.
// See the LICENSE file in the project root for full license information.

// This file was originally created by Paul den Dulk (Geodan) as part of SharpMap

using Mapsui.Layers;
using System;

namespace Mapsui.Fetcher;

public interface IAsyncDataFetcher
{
    /// <summary>
    /// Aborts the tile fetches that are in progress. If this method is not called
    /// the threads will terminate naturally. It will just take a little longer.
    /// </summary>
    void AbortFetch();

    /// <summary>
    /// Clear cache of layer
    /// </summary>
    void ClearCache();

    /// <summary>
    /// Indicates that there has been a change in the view of the map
    /// </summary>
    /// If Discrete an implementation should always refresh it's data. If Continuous the
    /// implementation could ignore it. Example: During dragging a map a WMS layer would not want
    /// to fetch data, only on the drag end.
    /// <param name="fetchInfo">FetchInfo</param>
    void RefreshData(FetchInfo fetchInfo);
}

public delegate void DataChangedEventHandler(object sender, DataChangedEventArgs e);

public class DataChangedEventArgs : EventArgs
{
    public DataChangedEventArgs() : this(null, false, null)
    {
    }

    public DataChangedEventArgs(Exception? error, bool cancelled, object? info)
        : this(error, cancelled, info, string.Empty)
    {
    }

    public DataChangedEventArgs(Exception? error, bool cancelled, object? info, string layerName)
    {
        Error = error;
        Cancelled = cancelled;
        Info = info;
        LayerName = layerName;
    }

    public Exception? Error { get; }
    public bool Cancelled { get; }
    public object? Info { get; }
    public string LayerName { get; }
}
