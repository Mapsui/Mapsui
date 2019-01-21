using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;
using Mapsui.UI.Objects;
using System;
using System.IO;
using System.Reflection;
using Xamarin.Forms;

namespace Mapsui.Samples.Forms
{
    public class PinSample : IFormsSample
    {
        static int markerNum = 1;
        static Random rnd = new Random();

        public string Name => "Add Pin Sample";

        public string Category => "Forms";

        public bool OnClick(object sender, EventArgs args)
        {
            var mapView = sender as MapView;
            var e = args as MapClickedEventArgs;

            var assembly = typeof(MainPageLarge).GetTypeInfo().Assembly;
            foreach (var str in assembly.GetManifestResourceNames())
                System.Diagnostics.Debug.WriteLine(str);

            string device;

            switch (Device.RuntimePlatform)
            {
                case "Android":
                    device = "Droid";
                    break;
                case "iOS":
                    device = "iOS";
                    break;
                case "macOS":
                    device = "Mac";
                    break;
                default:
                    device = $"{Device.RuntimePlatform}";
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
                    foreach (var r in assembly.GetManifestResourceNames())
                        System.Diagnostics.Debug.WriteLine(r);

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

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = OsmSample.CreateMap();

            ((MapView)mapControl).UseDoubleTap = true;
        }
    }
}
