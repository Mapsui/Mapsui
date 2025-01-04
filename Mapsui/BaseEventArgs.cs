using Mapsui.Layers;
using Mapsui.Manipulations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui;

public delegate Task<MapInfo> GetRemoteMapInfoAsyncDelegate(ScreenPosition screenPosition, IEnumerable<ILayer> layers);

public class BaseEventArgs(ScreenPosition screenPosition, MPoint worldPosition, TapType tapType,
    Func<MapInfo> getMapInfo, GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync) : EventArgs
{
    /// <summary>
    /// Screen Position of touch in device independent units (or DIP or DP)
    /// </summary>
    public ScreenPosition Position { get; } = screenPosition;

    /// <summary>
    /// World Position of touch in map coordinates
    /// </summary>
    public MPoint WorldPosition { get; } = worldPosition;

    /// <summary>
    /// Number of clicks on the Widget
    /// </summary>
    public TapType TapType { get; } = tapType;

    /// <summary>
    /// Function to get the MapInfo
    /// </summary>
    public Func<MapInfo> GetMapInfo { get; } = getMapInfo;

    /// <summary>
    /// Function to get the remote MapInfo
    /// </summary>
    public Func<IEnumerable<ILayer>, Task<MapInfo>> GetRemoteMapInfoAsync { get; } = (l) => getRemoteMapInfoAsync(screenPosition, l);
}
