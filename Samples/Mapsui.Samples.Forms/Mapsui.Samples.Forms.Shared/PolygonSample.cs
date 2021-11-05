using System;
using Mapsui.Samples.Common.Maps;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.UI;
using Mapsui.UI.Forms;
using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
    public class PolygonSample : IFormsSample
    {
        static readonly Random random = new Random();

        public string Name => "Add Polygon Sample";

        public string Category => "Forms";

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            var center = new Position(e.Point);
            var diffX = random.Next(0, 1000) / 100.0;
            var diffY = random.Next(0, 1000) / 100.0;

            var polygon = new Polygon
            {
                StrokeColor = new Color(random.Next(0, 255) / 255.0, random.Next(0, 255) / 255.0, random.Next(0, 255) / 255.0),
                FillColor = new Color(random.Next(0, 255) / 255.0, random.Next(0, 255) / 255.0, random.Next(0, 255) / 255.0)
            };

            polygon.Positions.Add(new Position { Longitude = center.Longitude - diffX, Latitude = center.Latitude - diffY });
            polygon.Positions.Add(new Position { Longitude = center.Longitude - diffX, Latitude = center.Latitude + diffY });
            polygon.Positions.Add(new Position { Longitude = center.Longitude + diffX, Latitude = center.Latitude + diffY });
            polygon.Positions.Add(new Position { Longitude = center.Longitude + diffX, Latitude = center.Latitude - diffY });

            // Be carefull: holes should have other direction of Positions.
            // If Positions is clockwise, than Holes should all be counter clockwise and the other way round.
            polygon.Holes.Add(new Position[] {
                new Position { Longitude = center.Longitude - diffX * 0.3, Latitude = center.Latitude - diffY * 0.3 },
                new Position { Longitude = center.Longitude + diffX * 0.3, Latitude = center.Latitude + diffY * 0.3 },
                new Position { Longitude = center.Longitude - diffX * 0.3, Latitude = center.Latitude + diffY * 0.3 },
            });

            polygon.IsClickable = true;
            polygon.Clicked += (s, a) => {
                ((Polygon)s).FillColor = new Color(random.Next(0, 255) / 255.0, random.Next(0, 255) / 255.0, random.Next(0, 255) / 255.0);
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
