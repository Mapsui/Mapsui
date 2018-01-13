using System;
using Mapsui.Geometries;

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

        double UnSnapRotationDegrees { get; set; }

        double ReSnapRotationDegrees { get; set; }

        Point WorldToScreen(Point worldPosition);
        
        Point ScreenToWorld(Point screenPosition);

        void Unsubscribe();
    }
}