using System;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;
using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
    public class CircleSample : IFormsSample
    {
        private static Random rnd = new Random();

        public string Name => "Add Circle Sample";

        public string Category => "Forms";

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            var circle = new Circle
            {
                Center = e.Point,
                Radius = Distance.FromMeters(rnd.Next(100000, 1000000)),
                Quality = rnd.Next(0, 60),
                StrokeColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0),
                FillColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0)
            };

            mapView.Drawables.Add(circle);

            return true;
        }

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = OsmSample.CreateMap();
        }
    }
}
