using Mapsui.Manipulations;
using System;

namespace Mapsui;

public class MapInfoEventArgs(MapInfo mapInfo, TapType tapType, bool handled) : EventArgs
{
    public MapInfo MapInfo { get; } = mapInfo;

    /// <summary>
    /// Number of times the user tapped the location
    /// </summary>
    public TapType TapType { get; } = tapType;

    /// <summary>
    /// If the interaction was handled by the event subscriber
    /// </summary>
    public bool Handled { get; set; } = handled;
}
