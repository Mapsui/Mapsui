using System;
using Mapsui.Geometries;
using Mapsui.Rendering;

namespace Mapsui.UI
{
    public interface IMapControl
    {
        Map Map { get; set; }

        event EventHandler ViewportInitialized;

        void RefreshGraphics();

        void RefreshData();

        void Refresh();

        bool RotationLock { get; set; }

        double UnSnapRotationDegrees { get; set; }

        double ReSnapRotationDegrees { get; set; }

        void Unsubscribe();

        /// <summary>
        /// The number of pixel per device independent unit
        /// </summary>
        float PixelDensity { get; }

        IRenderer Renderer { get; }

        /// <summary>
        /// The width of the map on screen in device independent units
        /// </summary>
        float ViewportWidth { get; }

        /// <summary>
        /// The height of the map on screen in device independent units
        /// </summary>
        float ViewportHeight { get; }

        void OpenBrowser(string url); //todo: remove when implemented an all platforms.

        /// <summary>
        /// Converts coordinates in pixels to device independent units (or DIP or DP).
        /// </summary>
        /// <param name="coordinateInPixels">Coordinate in pixels</param>
        /// <returns>Coordinate in device independent units (or DIP or DP)</returns>
        Point ToDeviceIndependentUnits(Point coordinateInPixels);

        /// <summary>
        /// Converts coordinates in device independent units (or DIP or DP) to pixels.
        /// </summary>
        /// <param name="coordinateInDeviceIndependentUnits">Coordinate in device independent units (or DIP or DP)</param>
        /// <returns>Coordinate in pixels</returns>
        Point ToPixels(Point coordinateInDeviceIndependentUnits);
    }
}