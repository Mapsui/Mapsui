using System;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
#if __MAUI__
using Mapsui.UI.Maui;
#else
using Mapsui.UI.Forms;
#endif

#if __MAUI__
namespace Mapsui.Samples.Maui
#else
namespace Mapsui.Samples.Forms
#endif
{
    public class AnimationSample : IFormsSample
    {
        public string Name => "Animation Sample";

        public string Category => "Forms";

        Random random = new Random();

        public bool OnClick(object? sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            if (e == null)
                return false;

            if (mapView == null)
                return false;

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
