using Mapsui.Layers;
using Mapsui.Styles;
using Mapsui.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Mapsui.Extensions;

/// <summary>
/// Extensions for MemoryLayer
/// </summary>
public static class MarkerExtensions
{
    public const string MarkerKey = "Marker";

    private static readonly string markerImage;
    private static readonly double markerImageHeight;
    private static readonly Regex extractHeight = new Regex("height=\\\"(\\d+)\\\"", RegexOptions.Compiled);

    static MarkerExtensions()
    {
        // Load SVG for Marker
        using (var s = new StreamReader(EmbeddedResourceLoader.Load($"Resources.Images.Pin.svg", typeof(MarkerExtensions))))
        {
            markerImage = s.ReadToEnd();

            var result = extractHeight.Matches(markerImage);

            if (result.Count < 1)
                return;

            markerImageHeight = result[0].Success ? double.Parse(result[0].Groups[1].Value ?? "") : 0;
        }
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="y">Y position</param>
    /// <param name="color">Color of marker</param>
    /// <param name="scale">Scale of marker</param>
    /// <param name="touched">Action called when marker is touched</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, double x, double y, Styles.Color? color = null, double scale = 1.0, string? title = null, string? subtitle = null, Action<ILayer, IFeature, MapInfoEventArgs>? touched = null)
    {
        var marker = new PointFeature(x, y);

        marker[MarkerKey] = true;

        var symbol = new SymbolStyle()
        {
            Enabled = true,
            SymbolType = SymbolType.Image,
            BitmapId = GetPinWithColor(color ?? Color.Red),
            SymbolOffset = new RelativeOffset(0.0, 0.5),
            SymbolScale = scale,
        };

        var callout = new CalloutStyle()
        {
            Enabled = false,
            Type = CalloutType.Single,
            ArrowPosition = 0.5f,
            ArrowAlignment = ArrowAlignment.Bottom,
            SymbolOffset = new Offset(0.0, markerImageHeight),
            Padding = new MRect(10, 5, 10, 5),
            Color = Color.Black,
            BackgroundColor = Color.White,
            MaxWidth = 200,
            TitleFontColor = Color.Black,
            TitleTextAlignment = Widgets.Alignment.Center,
            SubtitleFontColor = Color.Black,
            SubtitleTextAlignment = Widgets.Alignment.Center,
        };

        callout.Title = title;
        callout.TitleFont.Size = 16;
        callout.Subtitle = subtitle;
        callout.SubtitleFont.Size = 12;
        callout.Type = String.IsNullOrEmpty(callout.Subtitle) ? CalloutType.Single : CalloutType.Detail;

        marker.Styles.Clear();
        marker.Styles.Add(symbol);
        marker.Styles.Add(callout);

        if (touched != null) marker[MarkerKey+".Touched"] = touched;

        ((ConcurrentBag<IFeature>)layer.Features).Add(marker);

        return layer;
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="x">X position</param>
    /// <param name="color">Color of marker</param>
    /// <param name="scale">Scale of marker</param>
    /// <param name="touched">Action called when marker is touched</param>
    /// <param name="y">Y position</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, (double x, double y) position, Color? color = null, double scale = 1.0, string? title = null, string? subtitle = null, Action<ILayer, IFeature, MapInfoEventArgs>? touched = null)
    {
        return AddMarker(layer, position.x, position.y, color, scale, title, subtitle, touched);
    }

    /// <summary>
    /// Add a <see cref="Marker"/> to the layer
    /// </summary>
    /// <param name="layer">Layer to use</param>
    /// <param name="point">Point for position</param>
    /// <param name="color">Color of marker</param>
    /// <param name="scale">Scale of marker</param>
    /// <param name="touched">Action called when marker is touched</param>
    public static MemoryLayer AddMarker(this MemoryLayer layer, MPoint position, Color? color = null, double scale = 1.0, string? title = null, string? subtitle = null, Action<ILayer, IFeature, MapInfoEventArgs>? touched = null)
    {
        return AddMarker(layer, position.X, position.Y, color, scale, title, subtitle, touched);
    }

    /// <summary>
    /// Hide all callouts of <see cref="Marker"/> on this layer
    /// </summary>
    /// <param name="layer"></param>
    public static void HideAllCallouts(this MemoryLayer layer)
    {
        foreach (var m in layer.Features.Where(f => f.Fields.Contains(MarkerKey) && f.Styles.First(s => s is CalloutStyle) != null && f.Styles.First(s => s is CalloutStyle).Enabled))
            m.Styles.First(s => s is CalloutStyle).Enabled = false;
    }

    public static int GetPinWithColor(Color color)
    {
        var colorInHex = $"{color.R:X2}{color.G:X2}{color.B:X2}";

        if (BitmapRegistry.Instance.TryGetBitmapId($"{MarkerKey}_{colorInHex}", out int bitmapId))
            return bitmapId;

        var svg = markerImage.Replace("#000000", $"#{colorInHex}");

        return BitmapRegistry.Instance.Register(svg, $"{MarkerKey}_{colorInHex}");
    }

}
