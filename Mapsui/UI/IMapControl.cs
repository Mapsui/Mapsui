using System;
using System.Collections.Generic;
using System.ComponentModel;
using Mapsui.Layers;
using Mapsui.Manipulations;
using Mapsui.Rendering;

namespace Mapsui.UI;

public interface IMapControl : IDisposable
{
    event EventHandler<MapInfoEventArgs> Info;

    Map Map { get; set; }

    void RefreshGraphics();

    void RefreshData(ChangeType changeType = ChangeType.Discrete);

    void Refresh(ChangeType changeType = ChangeType.Discrete);

    void Unsubscribe();

    IRenderer Renderer { get; }

    void OpenInBrowser(string url);  // Todo: Perhaps remove. This is only to force the platform specific implementation

    /// <summary>
    /// Returns the number of pixels per device independent unit
    /// </summary>
    float? GetPixelDensity();

    /// <summary>
    /// Returns the number of pixels per device independent unit from the UI framework.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    float? GetPixelDensityFromFramework();

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
    /// Check, if a feature at a given screen position is hit.
    /// </summary>
    /// <param name="screenPosition">Screen position to check for widgets and features.</param>
    /// <param name="layers">The layers to query.</param>
    MapInfo GetMapInfo(ScreenPosition screenPosition, IEnumerable<ILayer> layers);

    /// <summary>
    /// Create a snapshot form map as PNG image
    /// </summary>
    /// <param name="layers">Layers that should be included in snapshot</param>
    /// <param name="renderFormat">render format</param>
    /// <param name="quality">default quality is 90 is applicable for webp and jpg</param>
    /// <returns>Byte array with snapshot in png format. If there are any problems than returns null.</returns>
    byte[] GetSnapshot(IEnumerable<ILayer>? layers = null, RenderFormat renderFormat = RenderFormat.Png, int quality = 100);
}
