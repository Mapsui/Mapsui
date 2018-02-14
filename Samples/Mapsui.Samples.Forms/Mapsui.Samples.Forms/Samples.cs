using Mapsui.UI.Forms;
using Mapsui.UI.Objects;
using System;
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

        public static void DrawPolylines(MapView mapView, MapClickedEventArgs e)
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
