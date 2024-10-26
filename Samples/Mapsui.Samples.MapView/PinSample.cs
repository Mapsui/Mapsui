using System;
using System.Reflection;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.PersistentCaches;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.UI.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui;
using Mapsui.Manipulations;
using BruTile.Predefined;
using Mapsui.Tiling.Layers;
using Mapsui.Tiling.Fetcher;
using Color = Microsoft.Maui.Graphics.Color;

namespace Mapsui.Samples.Maui;

public class PinSample : IMapViewSample
{
    int _markerNum = 1;
    readonly Random _random = new(4);

    public string Name => "Add Pin Sample";

    public string Category => "MapView";

    public bool UpdateLocation => true;

    public bool OnTap(object? sender, EventArgs args)
    {
        // The namespace prefix is somehow necessary on Linux.
        var mapClickedArgs = (MapClickedEventArgs)args;

        if (sender is not UI.Maui.MapView mapView)
            return false;

        var assembly = typeof(AllSamples).GetTypeInfo().Assembly;
        foreach (var str in assembly.GetManifestResourceNames())
            System.Diagnostics.Debug.WriteLine(str);

        var position = mapClickedArgs.Point;
        switch (mapClickedArgs.TapType)
        {
            case TapType.Single:
                var pin = new Pin(mapView)
                {
                    Label = $"PinType.Pin {_markerNum++}",
                    Position = position,
                    Address = position.ToString(),
                    Type = PinType.Pin,
                    Color = new Color(_random.Next(0, 256) / 256.0f, _random.Next(0, 256) / 256.0f, _random.Next(0, 256) / 256.0f),
                    Transparency = 0.5f,
                    Scale = _random.Next(50, 130) / 100f,
                    RotateWithMap = true,
                };
                pin.Callout.Anchor = new Point(0, pin.Height * pin.Scale);
                pin.Callout.RectRadius = _random.Next(0, 10);
                pin.Callout.TailHeight = _random.Next(5, 20);
                pin.Callout.TailWidth = _random.Next(0, 20);
                pin.Callout.TailAlignment = (TailAlignment)_random.Next(0, 4);
                pin.Callout.TailPosition = _random.Next(0, 100) / 100.0;
                pin.Callout.StrokeWidth = _random.Next(0, 10);
                pin.Callout.Padding = new Thickness(_random.Next(0, 20), _random.Next(0, 20));
                pin.Callout.BackgroundColor = Colors.White;
                pin.Callout.RotateWithMap = true;
                pin.Callout.IsClosableByClick = true;
                pin.Callout.Color = pin.Color;
                if (_random.Next(0, 3) < 2)
                {
                    pin.Callout.Type = CalloutType.Detail;
                    pin.Callout.TitleFontSize = _random.Next(15, 30);
                    pin.Callout.TitleTextAlignment = TextAlignment.Center;
                    pin.Callout.SubtitleFontSize = pin.Callout.TitleFontSize - 5;
                    pin.Callout.TitleFontColor = new Color(_random.Next(0, 256) / 256.0f, _random.Next(0, 256) / 256.0f, _random.Next(0, 256) / 256.0f);
                    pin.Callout.SubtitleFontColor = pin.Color;
                    pin.Callout.SubtitleTextAlignment = TextAlignment.Center;
                    pin.Callout.Spacing = _random.Next(0, 10);
                    pin.Callout.MaxWidth = _random.Next(100, 200);
                }
                else
                {
                    pin.Callout.Type = CalloutType.Detail;
                    pin.Callout.ContentId = "1";
                }
                pin.Callout.CalloutClicked += (s, e) =>
                {
                    if (e.TapType == TapType.Double)
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
            case TapType.Double:
                var resourceName = "embedded://Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg";
                mapView.Pins.Add(new Pin(mapView)
                {
                    Label = $"PinType.Svg {_markerNum++}",
                    Position = position,
                    Type = PinType.ImageSource,
                    Scale = 0.1f,
                    RotateWithMap = true,
                    ImageSource = resourceName
                });

                break;
        }

        return true;
    }

    public void Setup(IMapControl mapControl) => mapControl.Map = CreateMap();

    public static Map CreateMap()
    {
        var map = new Map();

        map.Layers.Add(CreateLayer());
        map.Navigator.CenterOnAndZoomTo(new MPoint(1059114.80157058, 5179580.75916194), map.Navigator.Resolutions[14]);
        map.BackColor = Styles.Color.FromString("#000613");

        return map;
    }

    private static TileLayer CreateLayer()
    {
        var apiKey = "Enter your api key here"; // Contact Microsoft about how to use this
        var tileSource = KnownTileSources.Create(KnownTileSource.BingHybrid, apiKey, BingHybrid.DefaultCache);
        return new TileLayer(tileSource, dataFetchStrategy: new DataFetchStrategy()) // DataFetchStrategy prefetches tiles from higher levels
        {
            Name = "Bing Aerial",
        };
    }
}
