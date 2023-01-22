using System;
using System.IO;
using System.Reflection;
using Mapsui.Extensions;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.UI.Forms;
using Xamarin.Forms;
using Color = Xamarin.Forms.Color;

namespace Mapsui.Samples.Forms.Shared;

public class PinSample : IFormsSample
{
    int _markerNum = 1;
    readonly Random _random = new Random(4);

    public string Name => "Add Pin Sample";

    public string Category => "Forms";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as MapView;
        var mapClickedArgs = (MapClickedEventArgs)args;

        if (mapView == null)
            return false;

        var assembly = typeof(AllSamples).GetTypeInfo().Assembly;
        foreach (var str in assembly.GetManifestResourceNames())
            System.Diagnostics.Debug.WriteLine(str);

        switch (mapClickedArgs.NumOfTaps)
        {
            case 1:
                var pin = new Pin(mapView)
                {
                    Label = $"PinType.Pin {_markerNum++}",
                    Position = mapClickedArgs.Point,
                    Address = mapClickedArgs.Point.ToString(),
                    Type = PinType.Pin,
                    Color = new Color(_random.Next(0, 256) / 256.0, _random.Next(0, 256) / 256.0, _random.Next(0, 256) / 256.0),
                    Transparency = 0.5f,
                    Scale = _random.Next(50, 130) / 100f,
                    RotateWithMap = true,
                };
                pin.Callout.Anchor = new Point(0, pin.Height * pin.Scale);
                pin.Callout.RectRadius = _random.Next(0, 10);
                pin.Callout.ArrowHeight = _random.Next(5, 20);
                pin.Callout.ArrowWidth = _random.Next(0, 20);
                pin.Callout.ArrowAlignment = (ArrowAlignment)_random.Next(0, 4);
                pin.Callout.ArrowPosition = _random.Next(0, 100) / 100.0;
                pin.Callout.StrokeWidth = _random.Next(0, 10);
                pin.Callout.Padding = new Thickness(_random.Next(0, 20), _random.Next(0, 20));
                pin.Callout.BackgroundColor = Color.White;
                pin.Callout.RotateWithMap = true;
                pin.Callout.IsClosableByClick = true;
                pin.Callout.Color = pin.Color;
                if (_random.Next(0, 3) < 2)
                {
                    pin.Callout.Type = CalloutType.Detail;
                    pin.Callout.TitleFontSize = _random.Next(15, 30);
                    pin.Callout.TitleTextAlignment = TextAlignment.Center;
                    pin.Callout.SubtitleFontSize = pin.Callout.TitleFontSize - 5;
                    pin.Callout.TitleFontColor = new Color(_random.Next(0, 256) / 256.0, _random.Next(0, 256) / 256.0, _random.Next(0, 256) / 256.0);
                    pin.Callout.SubtitleFontColor = pin.Color;
                    pin.Callout.SubtitleTextAlignment = TextAlignment.Center;
                    pin.Callout.Spacing = _random.Next(0, 10);
                    pin.Callout.MaxWidth = _random.Next(100, 200);
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
                        var p = e.Callout?.Pin;
                        if (p != null)
                        {
                            p.Position = new Position(p.Position.Latitude + 0.01, p.Position.Longitude);
                            e.Handled = true;
                        }

                        return;
                    }
                    if (e.Callout != null && e.Callout.Title != "You clicked me!")
                    {
                        e.Callout.Type = CalloutType.Single;
                        e.Callout.Title = "You clicked me!";
                        e.Handled = true;
                    }
                };
                mapView.Pins.Add(pin);
                pin.ShowCallout();
                break;
            case 2:
                var resourceName = "Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg";
                var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream == null) throw new Exception($"Could not find EmbeddedResource {resourceName}");
                using (var reader = new StreamReader(stream))
                {
                    string svgString = reader.ReadToEnd();
                    mapView.Pins.Add(new Pin(mapView)
                    {
                        Label = $"PinType.Svg {_markerNum++}",
                        Position = mapClickedArgs.Point,
                        Type = PinType.Svg,
                        Scale = 0.1f,
                        RotateWithMap = true,
                        Svg = svgString
                    });
                }

                break;
            case 3:
                using (var manifestResourceStream = assembly.GetManifestResourceStream("Mapsui.Samples.Common.Images.loc.png"))
                {
                    var icon = manifestResourceStream!.ToBytes();
                    mapView.Pins.Add(new Pin(mapView)
                    {
                        Label = $"PinType.Icon {_markerNum++}",
                        Position = mapClickedArgs.Point,
                        Type = PinType.Icon,
                        Scale = 0.5f,
                        Icon = icon
                    });
                }
                break;
        }

        return true;
    }

    public void Setup(IMapControl mapControl)
    {
        //OSM never displays....
        //mapControl.Map = OsmSample.CreateMap();

        //I like bing Hybrid
        mapControl.Map = BingSample.CreateMap(BingHybrid.DefaultCache, BruTile.Predefined.KnownTileSource.BingHybrid);

        ((MapView)mapControl).UseDoubleTap = true;
        //((MapView)mapControl).UniqueCallout = true;
    }
}
