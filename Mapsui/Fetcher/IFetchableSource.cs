using Mapsui.Layers;
using System;

namespace Mapsui.Fetcher;

public interface IFetchableSource
{
    /// <summary>
    /// The layer identifier.
    /// </summary>
    public int Id { get; }
    /// <summary>
    /// Gets the fetch requests for this layer. The requests are based on the viewport stored within the layer.
    /// </summary>
    /// <param name="activeFetchCount">Number of active fetches for this layer. It is relevant if the layer itself
    /// has a maximum on the number of active fetches for itself.</param>
    /// <param name="availableFetchSlots">Number of available fetch slots in the caller. It is the maximum number
    /// of fetch jobs the method should return.</param>
    /// <returns></returns>
    public FetchJob[] GetFetchJobs(int activeFetchCount, int availableFetchSlots);

    /// <summary>
    /// Informs the layer that the viewport has changed and it should update its data accordingly.
    /// </summary>
    /// <param name="fetchInfo"></param>
    public void ViewportChanged(FetchInfo fetchInfo);

    /// <summary>
    /// Clears the cache of this layer. Call this if source data has was invalidated or the layer is 
    /// removed.
    /// </summary>
    public void ClearCache();

    /// <summary>
    /// Indicates to the listener that it should fetch data again. This event is raised when there was
    /// a change in source data, so only relevant for dynamic data. The fetches triggered by viewport 
    /// changes do not depend on it.
    /// </summary>
    public event EventHandler<FetchRequestedEventArgs>? FetchRequested;
}
