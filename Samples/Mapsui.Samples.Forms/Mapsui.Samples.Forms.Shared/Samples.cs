using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI.Forms;
using Mapsui.UI.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
    public class Samples
    {
        static int markerNum = 1;
        static Random rnd = new Random();

        public const string AddPin = "Forms: Add Pin";
        public const string DrawPolyline = "Forms: Add Polyline";
        public const string DrawPolygon = "Forms: Add Polygon";
        public const string DrawCircle = "Forms: Add Circle";
        public const string MyLocation = "Forms: MyLocation";

        public static Dictionary<string, Func<Map>> CreateList()
        {
            var commonSamples = AllSamples.CreateList();
            var allSamples = new Dictionary<string, Func<Map>>();

            allSamples.Add(AddPin, OsmSample.CreateMap);
            allSamples.Add(DrawPolyline, OsmSample.CreateMap);
            allSamples.Add(DrawPolygon, OsmSample.CreateMap);
            allSamples.Add(DrawCircle, OsmSample.CreateMap);
            allSamples.Add(MyLocation, OsmSample.CreateMap);

            commonSamples.ToList().ForEach(x => allSamples.Add(x.Key, x.Value));

            return allSamples;
        }

        public static Func<MapView, MapClickedEventArgs, bool> GetClicker(string sample)
        {

            switch (sample)
            {
                case Samples.AddPin:
                    return Samples.SetPins;
                case Samples.DrawPolyline:
                    return Samples.DrawPolylines;
                case Samples.DrawPolygon:
                    return Samples.DrawPolygons;
                case Samples.DrawCircle:
                    return Samples.DrawCircles;
                case Samples.MyLocation:
                    return Samples.MyLocationSample;
            }

            return null;
        }

        public static bool MyLocationSample(MapView mapView, MapClickedEventArgs e)
        {
            mapView.MyLocationLayer.IsMoving = mapView.MyLocationEnabled;
            mapView.MyLocationEnabled = true;

            return true;
        }

        public static bool DrawCircles(MapView mapView, MapClickedEventArgs e)
        {
            var circle = new Circle
            {
                Center = e.Point,
                Radius = Distance.FromMeters(rnd.Next(0,100)),
                StrokeColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0),
                FillColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0,255) / 255.0)
            };

            mapView.Drawables.Add(circle);

            return true;
        }

        public static bool DrawPolygons(MapView mapView, MapClickedEventArgs e)
        {
            var center = new Position(e.Point);
            var diffX = rnd.Next(0, 1000) / 100.0;
            var diffY = rnd.Next(0, 1000) / 100.0;

            var polygon = new Polygon { StrokeColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0),
                FillColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0) };

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
            polygon.Clicked += (s, args) =>
            {
                ((Polygon)s).FillColor = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0);
                args.Handled = true;
            };

            mapView.Drawables.Add(polygon);

            return true;
        }

        public static bool DrawPolylines(MapView mapView, MapClickedEventArgs e)
        {
            Drawable f;

            lock (mapView.Drawables)
            {
                if (mapView.Drawables.Count == 0)
                {
                    f = new Polyline { StrokeWidth = 4, StrokeColor = Color.Red };
                    mapView.Drawables.Add(f);
                }
                else
                {
                    f = mapView.Drawables.First();
                }

                if (f is Polyline)
                {
                    ((Polyline)f).Positions.Add(e.Point);
                }
            }

            return true;
        }

        public static bool SetPins(MapView mapView, MapClickedEventArgs e)
        {
            var assembly = typeof(MainPageLarge).GetTypeInfo().Assembly;
            foreach (var str in assembly.GetManifestResourceNames())
                System.Diagnostics.Debug.WriteLine(str);
            
            string device;

            switch (Device.RuntimePlatform)
            {
                case "Android":
                    device = "Droid.Images";
                    break;
                case "iOS":
                    device = "iOS";
                    break;
                default:
                    device = $"{Device.RuntimePlatform}.Images";
                    break;
            }

            switch (e.NumOfTaps)
            {
                case 1:
                    var pin = new Pin(mapView)
                    {
                        Label = $"PinType.Pin {markerNum++}",
                        Address = e.Point.ToString(),
                        Position = e.Point,
                        Type = PinType.Pin,
                        Color = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0),
                        Transparency = 0.5f,
                        Scale = rnd.Next(50, 130) / 100f,
                    };
                    pin.CalloutAnchor = new Point(0, pin.Height * pin.Scale);
                    pin.Callout.RectRadius = rnd.Next(0, 20);
                    pin.Callout.ArrowHeight = rnd.Next(0, 20);
                    pin.Callout.ArrowWidth = rnd.Next(0, 20);
                    pin.Callout.ArrowAlignment = (ArrowAlignment)rnd.Next(0, 4);
                    pin.Callout.ArrowPosition = rnd.Next(0, 100) / 100;
                    pin.Callout.SubtitleLabel.LineBreakMode = LineBreakMode.NoWrap;
                    mapView.Pins.Add(pin);
                    break;
                case 2:
                    var stream = assembly.GetManifestResourceStream($"Mapsui.Samples.Forms.{device}.Images.Ghostscript_Tiger.svg");
                    StreamReader reader = new StreamReader(stream);
                    string svgString = reader.ReadToEnd();
                    mapView.Pins.Add(new Pin(mapView)
                    {
                        Label = $"PinType.Svg {markerNum++}",
                        Position = e.Point,
                        Type = PinType.Svg,
                        Scale = 0.1f,
                        Svg = svgString
                    });
                    break;
                case 3:
                    var icon = assembly.GetManifestResourceStream($"Mapsui.Samples.Forms.{device}.Images.loc.png").ToBytes();
                    mapView.Pins.Add(new Pin(mapView)
                    {
                        Label = $"PinType.Icon {markerNum++}",
                        Position = e.Point,
                        Type = PinType.Icon,
                        Scale = 0.5f,
                        Icon = icon
                    });
                    break;
            }

            return true;
        }
    }
}
