using System;

namespace Mapsui.UI
{
    public interface IMapControl
    {
        Map Map { get; set; }

        event EventHandler ViewportInitialized;

        void RefreshGraphics();

        void RefreshData();

        void Refresh();

        bool AllowPinchRotation { get; set; }
    }
}