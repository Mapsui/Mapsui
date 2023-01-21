using System;
using System.Collections.Generic;
using Mapsui.Layers;
using Mapsui.Rendering;
using Mapsui.Utilities;

namespace Mapsui.UI;

public interface IMapControl
{
    event EventHandler<MapInfoEventArgs> Info;

    Map? Map { get; set; }

    event EventHandler? ViewportInitialized;

    void RefreshGraphics();

    void RefreshData(ChangeType changeType = ChangeType.Discrete);

    void Refresh(ChangeType changeType = ChangeType.Discrete);

    double UnSnapRotationDegrees { get; set; }

    double ReSnapRotationDegrees { get; set; }

    void Unsubscribe();

    IRenderer Renderer { get; }

    void OpenBrowser(string url); //todo: Perhaps remove

    /// <summary>
    /// The number of pixel per device independent unit
    /// </summary>
    float PixelDensity { get; }

    /// <summary>
    /// Converts coordinates in pixels to device independent units (or DIP or DP).
    /// </summary>
    /// <param name="coordinateInPixels">Coordinate in pixels</param>
    /// <returns>Coordinate in device independent units (or DIP or DP)</returns>
    MPoint ToDeviceIndependentUnits(MPoint coordinateInPixels);

    /// <summary>
    /// Converts coordinates in device independent units (or DIP or DP) to pixels.
    /// </summary>
    /// <param name="coordinateInDeviceIndependentUnits">Coordinate in device independent units (or DIP or DP)</param>
    /// <returns>Coordinate in pixels</returns>
    MPoint ToPixels(MPoint coordinateInDeviceIndependentUnits);

    /// <summary>
    /// Check, if a feature at a given screen position is hit
    /// </summary>
    /// <param name="screenPosition">Screen position to check for widgets and features</param>
    /// <param name="margin">An optional extra margin around the feature to enlarge the hit area.</param>
    MapInfo? GetMapInfo(MPoint screenPosition, int margin = 0);

    /// <summary>
    /// Create a snapshot form map as PNG image
    /// </summary>
    /// <param name="layers">Layers that should be included in snapshot</param>
    /// <returns>Byte array with snapshot in png format. If there are any problems than returns null.</returns>
    byte[] GetSnapshot(IEnumerable<ILayer>? layers = null);

    INavigator? Navigator { get; }

    Performance? Performance { get; set; }

    IReadOnlyViewport Viewport { get; }
}
