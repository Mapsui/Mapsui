using Mapsui.Layers;
using Mapsui.Manipulations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui;

public class MapInfoEventArgs(Func<MapInfo> getMapInfo, TapType tapType, bool handled) : EventArgs
{
    /// <summary>
    /// Number of times the user tapped the location
    /// </summary>
    public TapType TapType { get; } = tapType;

    /// <summary>
    /// If the interaction was handled by the event subscriber
    /// </summary>
    public bool Handled { get; set; } = handled;

    /// <summary>
    /// Function to get the map info
    /// </summary>
    public Func<MapInfo> GetMapInfo { get; } = getMapInfo;

    public async Task<MapInfo> GetRemoteMapInfoAsync(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers, int margin)
    {
        return await RemoteMapInfoFetcher.GetRemoteMapInfoAsync(screenPosition, viewport, layers, margin);
    }
}
