using Mapsui.Layers;
using Mapsui.Manipulations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mapsui;

public delegate Task<MapInfo> GetRemoteMapInfoAsyncDelegate(ScreenPosition screenPosition, IEnumerable<ILayer> layers);
public delegate MapInfo GetMapInfoDelegate(ScreenPosition screenPosition, IEnumerable<ILayer> layers);

public class BaseEventArgs(ScreenPosition screenPosition, MPoint worldPosition, TapType tapType, Viewport viewport,
    GetMapInfoDelegate getMapInfo, GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync) : EventArgs
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
    /// Viewport of the map at the moment of the event
    /// </summary>
    public Viewport Viewport { get; } = viewport;

    /// <summary>
    /// Function to get the MapInfo
    /// </summary>
    public Func<IEnumerable<ILayer>, MapInfo> GetMapInfo { get; } = (l) => getMapInfo(screenPosition, l);

    /// <summary>
    /// Function to get the remote MapInfo
    /// </summary>
    public Func<IEnumerable<ILayer>, Task<MapInfo>> GetRemoteMapInfoAsync { get; } = (l) => getRemoteMapInfoAsync(screenPosition, l);
}
