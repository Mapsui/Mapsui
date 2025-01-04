using Mapsui.Manipulations;
using System;
using System.Threading.Tasks;

namespace Mapsui;

public class MapInfoEventArgs : BaseEventArgs
{
    public MapInfoEventArgs(ScreenPosition screenPosition, MPoint worldPosition, Func<MapInfo> getMapInfo,
        Func<Task<MapInfo>> getRemoteMapInfoAsync, TapType tapType, bool handled)
    {
        ScreenPosition = screenPosition;
        WorldPosition = worldPosition;
        GetMapInfo = getMapInfo;
        GetRemoteMapInfoAsync = getRemoteMapInfoAsync;
        TapType = tapType;
        Handled = handled;
    }

    /// <summary>
    /// Number of times the user tapped the location
    /// </summary>
    public TapType TapType { get; }

    /// <summary>
    /// If the interaction was handled by the event subscriber
    /// </summary>
    public bool Handled { get; set; }

    /// <summary>
    /// Function to get the map info
    /// </summary>
    public Func<MapInfo> GetMapInfo { get; }

    /// <summary>
    /// Function to get the remote map info
    /// </summary>
    public Func<Task<MapInfo>> GetRemoteMapInfoAsync { get; }

    /// <summary>
    /// The screen position of the event
    /// </summary>
    public ScreenPosition ScreenPosition { get; }

    /// <summary>
    /// The world position of the event
    /// </summary>
    public MPoint WorldPosition { get; }


}
