using System;
using System.Linq;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;
using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
    public class PolylineSample : IFormsSample
    {
        public string Name => "Add Polyline Sample";

        public string Category => "Forms";

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            UI.Objects.Drawable f;

            lock (mapView.Drawables)
            {
                if (mapView.Drawables.Count == 0)
                {
                    f = new Polyline { StrokeWidth = 4, StrokeColor = Color.Red, IsClickable = true };
                    mapView.Drawables.Add(f);
                }
                else
                {
                    f = mapView.Drawables.First();
                }
                
                if (f is Polyline polyline)
                {
                    polyline.Positions.Add(e.Point);
                }
            }

            return true;
        }

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = OsmSample.CreateMap();

            ((MapView)mapControl).UseDoubleTap = false;
        }
    }
}
