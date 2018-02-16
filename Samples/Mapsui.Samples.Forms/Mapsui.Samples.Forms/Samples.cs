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

        public static Dictionary<string, Func<Map>> CreateList()
        {
            var commonSamples = AllSamples.CreateList();
            var allSamples = new Dictionary<string, Func<Map>>();

            allSamples.Add(AddPin, OsmSample.CreateMap);
            allSamples.Add(DrawPolyline, OsmSample.CreateMap);
            allSamples.Add(DrawPolygon, OsmSample.CreateMap);

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
            }

            return null;
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

            mapView.Features.Add(polygon);

            return true;
        }

        public static bool DrawPolylines(MapView mapView, MapClickedEventArgs e)
        {
            IFeatureProvider f;

            lock (mapView.Features)
            {
                if (mapView.Features.Count == 0)
                {
                    f = new Polyline { StrokeWidth = 4, StrokeColor = Color.Red };
                    mapView.Features.Add(f);
                }
                else
                {
                    f = mapView.Features.First();
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
            var device = Device.RuntimePlatform.Equals("Android") ? "Droid" : Device.RuntimePlatform;

            switch (e.NumOfTaps)
            {
                case 1:
                    mapView.Pins.Add(new Pin
                    {
                        Label = $"PinType.Pin {markerNum++}",
                        Position = e.Point,
                        Type = PinType.Pin,
                        Color = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0),
                        Transparency = 0.5f,
                        Scale = rnd.Next(50, 130) / 100f,
                    });
                    break;
                case 2:
                    var stream = assembly.GetManifestResourceStream($"Mapsui.Samples.Forms.{device}.Images.Ghostscript_Tiger.svg");
                    StreamReader reader = new StreamReader(stream);
                    string svgString = reader.ReadToEnd();
                    mapView.Pins.Add(new Pin
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
                    mapView.Pins.Add(new Pin
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
