using System;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;

namespace Mapsui.Samples.Forms
{
    public class AnimationSample : IFormsSample
    {
        static int markerNum = 1;
        static Random rnd = new Random();

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
        }
    }
}
