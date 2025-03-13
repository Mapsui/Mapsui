using Mapsui.Layers;
using Mapsui.Manipulations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Mapsui;

public delegate Task<MapInfo> GetRemoteMapInfoAsyncDelegate(ScreenPosition screenPosition, Viewport viewport, IEnumerable<ILayer> layers);
public delegate MapInfo GetMapInfoDelegate(ScreenPosition screenPosition, IEnumerable<ILayer> layers);

public class BaseEventArgs(ScreenPosition screenPosition, MPoint worldPosition, GestureType gestureType, Map map,
    GetMapInfoDelegate getMapInfo, GetRemoteMapInfoAsyncDelegate getRemoteMapInfoAsync) : HandledEventArgs
{
    /// <summary>
    /// Screen Position of touch in device independent units (or DIP or DP)
    /// </summary>
    public ScreenPosition ScreenPosition { get; } = screenPosition;

    /// <summary>
    /// World Position of touch in map coordinates
    /// </summary>
    public MPoint WorldPosition { get; } = worldPosition;

    /// <summary>
    /// Number of clicks on the Widget
    /// </summary>
    public GestureType GestureType { get; } = gestureType;

    /// <summary>
    /// The map from which the event was triggered.
    /// </summary>
    public Map Map { get; } = map;

    /// <summary>
    /// Function to get the MapInfo
    /// </summary>
    public Func<IEnumerable<ILayer>, MapInfo> GetMapInfo { get; } = (l) => getMapInfo(screenPosition, l);

    /// <summary>
    /// Function to get the remote MapInfo
    /// </summary>
    public Func<IEnumerable<ILayer>, Task<MapInfo>> GetRemoteMapInfoAsync { get; } = (l) => getRemoteMapInfoAsync(screenPosition, map.Navigator.Viewport, l);
}
