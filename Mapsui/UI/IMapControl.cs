using System;
using Mapsui.Geometries;
using Mapsui.Rendering;

namespace Mapsui.UI
{
    public interface IMapControl
    {
        event EventHandler<MapInfoEventArgs> Info;

        Map Map { get; set; }

        event EventHandler ViewportInitialized;

        void RefreshGraphics();

        void RefreshData();

        void Refresh();

        double UnSnapRotationDegrees { get; set; }

        double ReSnapRotationDegrees { get; set; }

        void Unsubscribe();

        /// <summary>
        /// The number of pixel per device independent unit
        /// </summary>
        float PixelDensity { get; }

        IRenderer Renderer { get; }

        void OpenBrowser(string url); //todo: Perhaps remove

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

        /// <summary>
        /// Check, if a feature at a given screen position is hit
        /// </summary>
        /// <param name="screenPosition">Screen position to check for widgets and features</param>
        /// <param name="margin">An optional extra margin around the feature to enlarge the hit area.</param>
        MapInfo GetMapInfo(Point screenPosition, int margin = 0);

        INavigator Navigator { get; }

    }
}