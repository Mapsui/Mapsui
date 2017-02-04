using System.Collections.Generic;
using Mapsui.Geometries;
using Mapsui.Layers;

namespace Mapsui.UI
{
    public class InfoHelper
    {
        public static MouseInfoEventArgs GetInfoEventArgs(Map map, Point screenPosition, IEnumerable<ILayer> infoLayers)
        {
            var worldPosition = map.Viewport.ScreenToWorld(new Point(screenPosition.X, screenPosition.Y));

            var feature = map.GetFeatureInfo(infoLayers, worldPosition);

            if (feature == null) return null;

            return new MouseInfoEventArgs { LayerName = "", Feature = feature };
        }
    }
}