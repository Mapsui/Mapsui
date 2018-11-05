using System;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;

namespace Mapsui.Samples.Forms
{
    public class MyLocationSample : IFormsSample
    {
        static int markerNum = 1;
        static Random rnd = new Random();

        public string Name => "MyLocation Sample";

        public string Category => "Forms";

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            mapView.MyLocationLayer.IsMoving = mapView.MyLocationEnabled;
            mapView.MyLocationEnabled = true;

            return true;
        }

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = OsmSample.CreateMap();
        }
    }
}
