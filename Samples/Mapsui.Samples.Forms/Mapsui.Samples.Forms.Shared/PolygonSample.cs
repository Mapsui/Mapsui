using System;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;
using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
    public class PolygonSample : IFormsSample
    {
        static int markerNum = 1;
        static Random rnd = new Random();

        public string Name => "Add Polygon Sample";

        public string Category => "Forms";

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            var center = new Position(e.Point);
            var diffX = rnd.Next(0, 1000) / 100.0;
            var diffY = rnd.Next(0, 1000) / 100.0;

            var polygon = new Polygon
            {
                StrokeColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0),
                FillColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0)
            };

            polygon.Positions.Add(new Position(center.Latitude - diffY, center.Longitude - diffX));
            polygon.Positions.Add(new Position(center.Latitude + diffY, center.Longitude - diffX));
            polygon.Positions.Add(new Position(center.Latitude + diffY, center.Longitude + diffX));
            polygon.Positions.Add(new Position(center.Latitude - diffY, center.Longitude + diffX));

            // Be carefull: holes should have other direction of Positions.
            // If Positions is clockwise, than Holes should all be counter clockwise and the other way round.
            polygon.Holes.Add(new Position[] {
                new Position(center.Latitude - diffY * 0.3, center.Longitude - diffX * 0.3),
                new Position(center.Latitude + diffY * 0.3, center.Longitude + diffX * 0.3),
                new Position(center.Latitude + diffY * 0.3, center.Longitude - diffX * 0.3),
            });

            polygon.IsClickable = true;
            polygon.Clicked += (s, a) =>
            {
                ((Polygon)s).FillColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0);
                a.Handled = true;
            };

            mapView.Drawables.Add(polygon);

            return true;
        }

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = OsmSample.CreateMap();
        }
    }
}
