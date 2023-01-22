using Mapsui.Samples.Common;
using Mapsui.Samples.Common.Maps;
using Mapsui.UI;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Mapsui.Extensions;
using Mapsui.Samples.Common.Maps.Demo;
using Mapsui.Styles;
using Mapsui.Widgets.PerformanceWidget;
#if __MAUI__
using Mapsui.UI.Maui;
using Microsoft.Maui.Graphics;
using Color = Microsoft.Maui.Graphics.Color;
using KnownColor = Mapsui.UI.Maui.KnownColor;
#else
using Mapsui.UI.Forms;
using Xamarin.Forms;
using Color = Xamarin.Forms.Color;
using KnownColor = Xamarin.Forms.Color;
#endif

#if __MAUI__
namespace Mapsui.Samples.Maui;
#else
namespace Mapsui.Samples.Forms;
#endif

public class ManyPinsSample : IFormsSample
{
    static int markerNum = 1;
    static Random rnd = new Random(1);

    public string Name => "Add many Pins Sample";

    public string Category => "Forms";

    public bool OnClick(object? sender, EventArgs args)
    {
        var mapView = sender as MapView;
        var e = args as MapClickedEventArgs;

        if (mapView == null)
            return false;

        var assembly = typeof(AllSamples).GetTypeInfo().Assembly;
        foreach (var str in assembly.GetManifestResourceNames())
            System.Diagnostics.Debug.WriteLine(str);

        switch (e?.NumOfTaps)
        {
            case 1:
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
                pin.Callout.ArrowHeight = rnd.Next(0, 20);
                pin.Callout.ArrowWidth = rnd.Next(0, 20);
                pin.Callout.ArrowAlignment = (ArrowAlignment)rnd.Next(0, 4);
                pin.Callout.ArrowPosition = rnd.Next(0, 100) / 100;
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
                    pin.Callout.Content = 1;
                }
                mapView.Pins.Add(pin);
                pin.ShowCallout();
                break;
            case 2:
                foreach (var r in assembly.GetManifestResourceNames())
                    System.Diagnostics.Debug.WriteLine(r);

                using (var stream = assembly.GetManifestResourceStream("Mapsui.Samples.Common.Images.Ghostscript_Tiger.svg"))
                {
                    using StreamReader reader = new StreamReader(stream!);
                    var svgString = reader.ReadToEnd();
                    mapView?.Pins.Add(new Pin(mapView)
                    {
                        Label = $"PinType.Svg {markerNum++}",
                        Position = e.Point,
                        Type = PinType.Svg,
                        Scale = 0.1f,
                        Svg = svgString
                    });
                }

                break;
            case 3:
                var icon = assembly.GetManifestResourceStream("Mapsui.Samples.Common.Images.loc.png")!.ToBytes();
                mapView?.Pins.Add(new Pin(mapView)
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

        if (mapControl.Performance == null)
            mapControl.Performance = new Utilities.Performance();

        var widget = new PerformanceWidget(mapControl.Performance);

        widget.WidgetTouched += (sender, args) =>
        {
            mapControl?.Performance.Clear();
            mapControl?.RefreshGraphics();

            args.Handled = true;
        };

        mapControl.Map.Widgets.Add(widget);
        mapControl.Renderer.WidgetRenders[typeof(PerformanceWidget)] = new Rendering.Skia.SkiaWidgets.PerformanceWidgetRenderer(10, 10, 12, SkiaSharp.SKColors.Black, SkiaSharp.SKColors.White);

        ((MapView)mapControl).UseDoubleTap = true;
        ((MapView)mapControl).UniqueCallout = true;

        var sw = new Stopwatch();
        sw.Start();

        // Add 1000 pins
        var list = new System.Collections.Generic.List<Pin>();
        for (var i = 0; i < 1000; i++)
        {
            list.Add(CreatePin(i));
        }

        var timePart1 = sw.Elapsed;

        ((ObservableRangeCollection<Pin>)((MapView)mapControl).Pins).AddRange(list);

        var timePart2 = sw.Elapsed;

        sw.Stop();
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
        pin.Callout.ArrowHeight = rnd.Next(0, 20);
        pin.Callout.ArrowWidth = rnd.Next(0, 20);
        pin.Callout.ArrowAlignment = (ArrowAlignment)rnd.Next(0, 4);
        pin.Callout.ArrowPosition = rnd.Next(0, 100) / 100;
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
            pin.Callout.Content = 1;
        }

        return pin;
    }
}
