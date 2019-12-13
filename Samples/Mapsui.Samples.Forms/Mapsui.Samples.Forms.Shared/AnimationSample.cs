using System;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;

namespace Mapsui.Samples.Forms
{
    public class AnimationSample : IFormsSample
    {
        public string Name => "Animation Sample";

        public string Category => "Forms";

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            var navigator = (AnimatedNavigator)mapView.Navigator;
            navigator.FlyTo(e.Point.ToMapsui(), mapView.Viewport.Resolution * 2);

            return true;
        }

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = OsmSample.CreateMap();
            if (mapControl is MapView)
                ((MapView)mapControl).Navigator = new AnimatedNavigator(mapControl.Map, (IViewport)((MapView)mapControl).Viewport);
            else
                ((MapControl)mapControl).Navigator = new AnimatedNavigator(mapControl.Map, (IViewport)((MapControl)mapControl).Viewport);
        }
    }
}
