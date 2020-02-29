using Mapsui.Rendering.Skia;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using Mapsui.UI.Forms;
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
            var mapClickedArgs = args as MapClickedEventArgs;

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

            switch (mapClickedArgs.NumOfTaps)
            {
                case 1:
                    var pin = new Pin(mapView)
                    {
                        Label = $"PinType.Pin {markerNum++}",
                        Position = mapClickedArgs.Point,
                        Address = mapClickedArgs.Point.ToString(),
                        Type = PinType.Pin,
                        Color = new Xamarin.Forms.Color(rnd.Next(0, 256) / 256.0, rnd.Next(0, 256) / 256.0, rnd.Next(0, 256) / 256.0),
                        Transparency = 0.5f,
                        Scale = rnd.Next(50, 130) / 100f,
                    };
                    pin.Callout.Anchor = new Point(0, pin.Height * pin.Scale);
                    pin.Callout.RectRadius = rnd.Next(0, 10);
                    pin.Callout.ArrowHeight = rnd.Next(5, 20);
                    pin.Callout.ArrowWidth = rnd.Next(0, 20);
                    pin.Callout.ArrowAlignment = (ArrowAlignment)rnd.Next(0, 4);
                    pin.Callout.ArrowPosition = rnd.Next(0, 100) / 100.0;
                    pin.Callout.StrokeWidth = rnd.Next(0, 10);
                    pin.Callout.Padding = new Thickness(rnd.Next(0, 20), rnd.Next(0, 20));
                    pin.Callout.BackgroundColor = Color.White;
                    pin.Callout.RotateWithMap = true;
                    pin.Callout.Color = pin.Color;
                    if (rnd.Next(0, 3) < 2)
                    {
                        pin.Callout.Type = CalloutType.Detail;
                        pin.Callout.TitleFontSize = rnd.Next(15, 30);
                        pin.Callout.TitleTextAlignment = TextAlignment.Center;
                        pin.Callout.SubtitleFontSize = pin.Callout.TitleFontSize - 5;
                        pin.Callout.TitleFontColor = new Xamarin.Forms.Color(rnd.Next(0, 256) / 256.0, rnd.Next(0, 256) / 256.0, rnd.Next(0, 256) / 256.0);
                        pin.Callout.SubtitleFontColor = pin.Color;
                        pin.Callout.SubtitleTextAlignment = TextAlignment.Center;
                        pin.Callout.Spacing = rnd.Next(0, 10);
                        pin.Callout.MaxWidth = rnd.Next(100, 200);
                    }
                    else
                    {
                        pin.Callout.Type = CalloutType.Detail;
                        pin.Callout.Content = 1;
                    }
                    pin.Callout.CalloutClicked += (s, e) =>
                    {
                        if (e.NumOfTaps == 2)
                        {
                            // Double click on callout moves pin
                            var p = e.Callout.Pin;
                            p.Position = new Position(p.Position.Latitude + 0.01, p.Position.Longitude);
                            e.Handled = true;
                            return;
                        }
                        if (e.Callout.Title != "You clicked me!")
                        {
                            e.Callout.Type = CalloutType.Single;
                            e.Callout.Title = "You clicked me!";
                            e.Handled = true;
                            return;
                        }
                    };
                    mapView.Pins.Add(pin);
                    pin.ShowCallout();
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
                        Position = mapClickedArgs.Point,
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
                        Position = mapClickedArgs.Point,
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
            //OSM never displays....
            //mapControl.Map = OsmSample.CreateMap();

            //I like bing Hybrid
            mapControl.Map = BingSample.CreateMap(BruTile.Predefined.KnownTileSource.BingHybrid);

            ((MapView)mapControl).UseDoubleTap = true;
            //((MapView)mapControl).UniqueCallout = true;
        }
    }
}
