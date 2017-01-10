using System;
using Mapsui.Geometries;
using Mapsui.Utilities;

namespace Mapsui.UI
{
    public class ViewportHelper
    {
        public static bool TryInitializeViewport(Map map, double screenWidth, double screenHeight)
        {
            if (screenWidth.IsNanOrZero()) return false;
            if (screenHeight.IsNanOrZero()) return false;

            if (double.IsNaN(map.Viewport.Resolution)) // only when not set yet
            {
                if (!map.Envelope.IsInitialized()) return false;
                if (map.Envelope.GetCentroid() == null) return false;

                if (Math.Abs(map.Envelope.Width) > Constants.Epsilon)
                    map.Viewport.Resolution = map.Envelope.Width / screenWidth;
                else
                    // An envelope width of zero can happen when there is no data in the Maps' layers (yet).
                    // It should be possible to start with an empty map.
                    map.Viewport.Resolution = Constants.DefaultResolution;
            }
            if (double.IsNaN(map.Viewport.Center.X) || double.IsNaN(map.Viewport.Center.Y)) // only when not set yet
            {
                if (!map.Envelope.IsInitialized()) return false;
                if (map.Envelope.GetCentroid() == null) return false;

                map.Viewport.Center = map.Envelope.GetCentroid();
            }

            map.Viewport.Width = screenWidth;
            map.Viewport.Height = screenHeight;
            map.Viewport.RenderResolutionMultiplier = 1.0;

            return true;
        }
    }
}
