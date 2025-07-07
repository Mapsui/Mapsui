using Mapsui.Layers;
using System;

namespace Mapsui.Fetcher;

public interface IFetchJobSource
{
    /// <summary>
    /// The layer identifier.
    /// </summary>
    public int Id { get; }
    /// <summary>
    /// Gets the fetch requests for this layer. The requests are based on the viewport stored within the layer.
    /// </summary>
    /// <param name="activeFetchCount">Number of active fetches for this layer.</param>
    /// <returns></returns>
    public FetchJob[] GetFetchJobs(int activeFetchCount, int availableFetchSlots);

    /// <summary>
    /// Informs the layer that the viewport has changed and it should update its data accordingly.
    /// </summary>
    /// <param name="fetchInfo"></param>
    public void ViewportChanged(FetchInfo fetchInfo);

    public event EventHandler<FetchRequestedEventArgs>? FetchRequested;
}
