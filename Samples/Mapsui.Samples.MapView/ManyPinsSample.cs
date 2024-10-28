using Mapsui.Extensions;
using Mapsui.Manipulations;
using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.Styles;
using Mapsui.UI;
using Mapsui.UI.Maui;
using Mapsui.Utilities;
using Mapsui.Widgets.InfoWidgets;
using Microsoft.Maui.Graphics;
using System;
using System.Diagnostics;
using System.Reflection;
using Color = Microsoft.Maui.Graphics.Color;
using KnownColor = Mapsui.UI.Maui.KnownColor;

namespace Mapsui.Samples.Maui;

public class ManyPinsSample : IMapViewSample
{
    static int markerNum = 1;
    static Random rnd = new Random(1);

    public string Name => "Add many Pins Sample";

    public string Category => "MapView";

    public bool UpdateLocation => true;

    public bool OnTap(object? sender, EventArgs args)
    {
        var mapView = sender as UI.Maui.MapView;
        var e = args as MapClickedEventArgs;

        if (mapView == null)
            return false;

        var assembly = typeof(AllSamples).GetTypeInfo().Assembly;
        foreach (var str in assembly.GetManifestResourceNames())
            System.Diagnostics.Debug.WriteLine(str);

        switch (e?.TapType)
        {
            case TapType.Single:
                var pin = new Pin(mapView)
                {
                    Label = $"PinType.Pin {markerNum++}",
                    Address = e.Point.ToString(),
                    Position = e.Point,
                    Type = PinType.Pin,
                    Color = new Color(rnd.Next(0, 256) / 256.0f, rnd.Next(0, 256) / 256.0f, rnd.Next(0, 256) / 256.0f),
                    Transparency = 0.5f,
                    Scale = rnd.Next(50, 130) / 100f,
                };
                pin.Callout.Anchor = new Point(0, pin.Height * pin.Scale);
                pin.Callout.RectRadius = rnd.Next(0, 30);
                pin.Callout.TailHeight = rnd.Next(0, 20);
                pin.Callout.TailWidth = rnd.Next(0, 20);
                pin.Callout.TailAlignment = (TailAlignment)rnd.Next(0, 4);
                pin.Callout.TailPosition = rnd.Next(0, 100) / 100;
                pin.Callout.BackgroundColor = KnownColor.White;
                pin.Callout.Color = pin.Color;
                if (rnd.Next(0, 3) < 2)
                {
                    pin.Callout.Type = CalloutType.Detail;
                    pin.Callout.TitleFontSize = rnd.Next(15, 30);
                    pin.Callout.SubtitleFontSize = pin.Callout.TitleFontSize - 5;
                    pin.Callout.TitleFontColor = new Color(rnd.Next(0, 256) / 256.0f, rnd.Next(0, 256) / 256.0f, rnd.Next(0, 256) / 256.0f);
                    pin.Callout.SubtitleFontColor = pin.Color;
                }
                else
                {
                    pin.Callout.Type = CalloutType.Detail;
                    pin.Callout.ContentId = "1";
                }
                mapView.Pins.Add(pin);
                pin.ShowCallout();
                break;
            case TapType.Double:
                foreach (var r in assembly.GetManifestResourceNames())
                    System.Diagnostics.Debug.WriteLine(r);

                var resourceName = "embedded://Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg";
                mapView?.Pins.Add(new Pin(mapView)
                {
                    Label = $"PinType.Svg {markerNum++}",
                    Position = e.Point,
                    Type = PinType.ImageSource,
                    Scale = 0.1f,
                    ImageSource = resourceName
                });
                break;
            default:
                throw new Exception("Unknown TapType. This is bug in Mapsui.");
        }

        return true;
    }

    public void Setup(IMapControl mapControl)
    {
        mapControl.Map = OsmSample.CreateMap();

        if (mapControl.Performance == null)
            mapControl.Performance = new Performance();

        mapControl.Map.Widgets.Add(CreatePerformanceWidget(mapControl));
        mapControl.Renderer.WidgetRenders[typeof(PerformanceWidget)] = new Rendering.Skia.SkiaWidgets.PerformanceWidgetRenderer();

        ((UI.Maui.MapView)mapControl).UniqueCallout = true;

        var sw = new Stopwatch();
        sw.Start();

        // Add 1000 pins
        var list = new System.Collections.Generic.List<Pin>();
        for (var i = 0; i < 1000; i++)
        {
            list.Add(CreatePin(i));
        }

        _ = sw.Elapsed;

        ((ObservableRangeCollection<Pin>)((UI.Maui.MapView)mapControl).Pins).AddRange(list);

        _ = sw.Elapsed;

        sw.Stop();
    }

    private static PerformanceWidget CreatePerformanceWidget(IMapControl mapControl)
    {
        ArgumentNullException.ThrowIfNull(mapControl.Performance);
        return new PerformanceWidget(mapControl.Performance)
        {
            Tapped = (sender, args) =>
            {
                mapControl?.Performance.Clear();
                mapControl?.RefreshGraphics();
                return true;
            }
        };
    }

    private Pin CreatePin(int num)
    {
        var position = new Position(0 + rnd.Next(-85000, +85000) / 1000.0, 0 + rnd.Next(-180000, +180000) / 1000.0);

        var pin = new Pin()
        {
            Label = $"PinType.Pin {num++}",
            Address = position.ToString(),
            Position = position,
            Type = PinType.Pin,
            Color = new Color(rnd.Next(0, 256) / 256.0f, rnd.Next(0, 256) / 256.0f, rnd.Next(0, 256) / 256.0f),
            Transparency = 0.5f,
            Scale = rnd.Next(50, 130) / 100f,
        };
        pin.Callout.Anchor = new Point(0, pin.Height * pin.Scale);
        pin.Callout.RectRadius = rnd.Next(0, 30);
        pin.Callout.TailHeight = rnd.Next(0, 20);
        pin.Callout.TailWidth = rnd.Next(0, 20);
        pin.Callout.TailAlignment = (TailAlignment)rnd.Next(0, 4);
        pin.Callout.TailPosition = rnd.Next(0, 100) / 100;
        pin.Callout.BackgroundColor = KnownColor.White;
        pin.Callout.Color = pin.Color;
        if (rnd.Next(0, 3) < 2)
        {
            pin.Callout.Type = CalloutType.Detail;
            pin.Callout.TitleFontSize = rnd.Next(15, 30);
            pin.Callout.SubtitleFontSize = pin.Callout.TitleFontSize - 5;
            pin.Callout.TitleFontColor = new Color(rnd.Next(0, 256) / 256.0f, rnd.Next(0, 256) / 256.0f, rnd.Next(0, 256) / 256.0f);
            pin.Callout.SubtitleFontColor = pin.Color;
        }
        else
        {
            pin.Callout.Type = CalloutType.Detail;
            pin.Callout.ContentId = "1";
        }

        return pin;
    }
}
