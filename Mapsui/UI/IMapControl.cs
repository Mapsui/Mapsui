using System;
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
        /// The map's screen width in device independent units
        /// </summary>
        float ScreenWidth { get; }

        /// <summary>
        /// The map's screen height in device independent units
        /// </summary>
        float ScreenHeight { get; }
    }
}