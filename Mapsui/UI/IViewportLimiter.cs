using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.UI;

namespace Mapsui
{
    interface IViewportLimiter
    {
        void Limit(IViewport viewport,
            ZoomMode zoomMode, MinMax zoomLimits, IReadOnlyList<double> mapResolutions,
            PanMode panMode, BoundingBox panLimits, BoundingBox mapEnvelope);

        double LimitResolution(double resolution, double screenWidth, double screenHeight, ZoomMode zoomMode,
            MinMax zoomLimits,
            IReadOnlyList<double> mapResolutions, BoundingBox mapEnvelope);

        void LimitExtent(IViewport viewport, PanMode panMode, BoundingBox panLimits, BoundingBox mapEnvelope);
    }
}