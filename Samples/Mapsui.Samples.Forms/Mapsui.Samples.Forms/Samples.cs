using Mapsui.UI.Forms;
using System;
using System.IO;
using System.Reflection;
using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
    public class Samples
    {
        static int markerNum = 1;
        static Random rnd = new Random();

        public static bool HandlerMapClicked(MapView mapView, MapClickedEventArgs e)
        {
            if (e.NumOfTaps == 2)
            {
                var assembly = typeof(MainPageLarge).GetTypeInfo().Assembly;
                foreach (var str in assembly.GetManifestResourceNames())
                    System.Diagnostics.Debug.WriteLine(str);
                var device = Device.RuntimePlatform.Equals("Android") ? "Droid" : Device.RuntimePlatform;
                var stream = assembly.GetManifestResourceStream($"Mapsui.Samples.Forms.{device}.Images.Ghostscript_Tiger.svg");
                StreamReader reader = new StreamReader(stream);
                string svgString = reader.ReadToEnd();
                mapView.Pins.Add(new Pin
                {
                    Label = $"Tiger {markerNum++}",
                    Position = e.Point,
                    Type = PinType.Svg,
                    Scale = 0.1f,
                    Svg = svgString
                });
            }
            else
                mapView.Pins.Add(new Pin
                {
                    Label = $"Marker {markerNum++}",
                    Position = e.Point,
                    Type = PinType.Pin,
                    Color = new Color(rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0, rnd.Next(0, 255) / 255.0),
                    Scale = rnd.Next(50, 130) / 100f,
                });

            return true;
        }
    }
}
