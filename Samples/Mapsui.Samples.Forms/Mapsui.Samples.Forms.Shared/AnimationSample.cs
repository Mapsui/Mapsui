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

        Random random = new Random();

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            var navigator = (Navigator)mapView.Navigator;

            var newRot = random.NextDouble() * 360.0;

            //navigator.RotateTo(newRot, 500);
            navigator.FlyTo(e.Point.ToMapsui(), mapView.Viewport.Resolution * 8, 5000);

            return true;
        }

        public void Setup(IMapControl mapControl)
        {
            var mapView = mapControl as MapView;

            mapControl.Map = OsmSample.CreateMap();
        }
    }
}
