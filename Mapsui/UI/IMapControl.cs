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

        Point WorldToScreen(Point worldPosition);
        
        Point ScreenToWorld(Point screenPosition);

        void Unsubscribe();

        float GetDeviceIndependentUnits();

        IRenderer Renderer { get; }
    }
}