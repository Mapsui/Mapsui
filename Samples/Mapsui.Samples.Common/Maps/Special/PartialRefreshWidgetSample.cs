using Mapsui.Experimental.Rendering.Skia;
using Mapsui.Extensions;
using Mapsui.Rendering;
using Mapsui.Styles;
using Mapsui.Tiling;
using Mapsui.Widgets;
using Mapsui.Widgets.BoxWidgets;
using Mapsui.Widgets.ButtonWidgets;
using SkiaSharp;
using System;
using System.Threading.Tasks;

namespace Mapsui.Samples.Common.Maps.Special;

// This sample demonstrates partial viewport refresh: instead of redrawing the entire map,
// only a specified rectangle is redrawn. The random-colour full-screen overlay makes it easy
// to see exactly which area was repainted on each button tap.
//
// Two coordinate spaces are shown:
//   - World (EPSG:3857 metres): the dirty rect is expressed in geographic coordinates.
//     Use this when refreshing spatial features whose position changes in the real world,
//     such as a moving GPS marker or a live vehicle track. Panning the map away from the
//     feature's location means the refresh has no visible effect — which is correct.
//   - Screen (device-independent pixels): the dirty rect is expressed relative to the
//     top-left corner of the rendered surface. Use this for fixed-position UI elements
//     such as widgets, overlays, or attribution labels that always occupy the same spot
//     regardless of where the map is panned or zoomed.
public sealed class PartialRefreshWidgetSample : ISample
{
    public string Name => "PartialRefreshWidget";
    public string Category => "Special";

    public Task<Map> CreateMapAsync() => Task.FromResult(CreateMap());

    public static Map CreateMap()
    {
        Mapsui.Rendering.Skia.MapRenderer.RegisterWidgetRenderer(typeof(FullScreenColorWidget), new FullScreenColorWidgetRenderer());
        MapRenderer.RegisterWidgetRenderer(typeof(FullScreenColorWidget), new FullScreenColorWidgetExperimentalRenderer());

        var map = new Map { CRS = "EPSG:3857" };
        map.Layers.Add(OpenStreetMap.CreateTileLayer());
        map.Widgets.Add(new FullScreenColorWidget());
        map.Widgets.Add(CreateInstructionTextBox());
        map.Widgets.Add(CreateWorldSpaceRefreshButton());
        map.Widgets.Add(CreateScreenSpaceRefreshButton());
        return map;
    }

    // World-space refresh: the dirty rect is a fixed geographic extent in EPSG:3857 metres.
    // Only tiles and features that intersect this rectangle are redrawn. If you pan the
    // viewport outside this area the button has no visible effect, illustrating that the
    // refresh is tied to geography, not to the screen. This is the right choice for
    // features that move through the world, e.g. a live GPS position update.
    private static ButtonWidget CreateWorldSpaceRefreshButton() => new()
    {
        Text = "Refresh World Area (World coords)",
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Left,
        CornerRadius = 3,
        BackColor = new Color(0, 123, 255),
        TextColor = Color.White,
        Margin = new MRect(10, 50, 10, 50),
        Padding = new MRect(8),
        TextSize = 16,
        WithTappedEvent = (s, e) =>
        {
            // Fixed world-space extent — pan away from this area and the button has no effect.
            var dirtyRect = new MRect(-1_000_000, -1_000_000, 1_000_000, 1_000_000);
            e.Map.RefreshGraphics(dirtyRect); // CoordinateSpace.World is the default
            e.Handled = true;
        }
    };

    // Screen-space refresh: the dirty rect is expressed in device-independent pixels
    // relative to the top-left of the rendered surface. The refreshed region stays at
    // exactly the same spot on screen regardless of how the map is panned or zoomed.
    // This is the right choice for widgets and overlays — e.g. a GPS accuracy badge or
    // an attribution label — whose screen position never changes.
    private static ButtonWidget CreateScreenSpaceRefreshButton() => new()
    {
        Text = "Refresh Bottom-Center (Screen)",
        VerticalAlignment = VerticalAlignment.Bottom,
        HorizontalAlignment = HorizontalAlignment.Left,
        CornerRadius = 3,
        BackColor = new Color(40, 167, 69),
        TextColor = Color.White,
        Margin = new MRect(10),
        Padding = new MRect(8),
        TextSize = 16,
        WithTappedEvent = (s, e) =>
        {
            // A fixed pixel rectangle covering the bottom-centre of the screen — exactly
            // where a typical overlay widget (e.g. an attribution bar) would live.
            var w = e.Map.Navigator.Viewport.Width;
            var h = e.Map.Navigator.Viewport.Height;
            var dirtyRect = new MRect(w / 2 - 100, h - 60, w / 2 + 100, h);
            e.Map.RefreshGraphics(dirtyRect, CoordinateSpace.Screen);
            e.Handled = true;
        }
    };

    private static TextBoxWidget CreateInstructionTextBox() => new()
    {
        Text = "Partial viewport refresh in world and screen coordinate space using a random color overlay.",
        TextSize = 16,
        VerticalAlignment = VerticalAlignment.Top,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new MRect(10),
        Padding = new MRect(8),
        CornerRadius = 3,
        BackColor = new Color(108, 117, 125, 128),
        TextColor = Color.White,
    };


}

internal sealed class FullScreenColorWidget : BaseWidget { }

internal sealed class FullScreenColorWidgetRenderer : Mapsui.Rendering.Skia.SkiaWidgets.ISkiaWidgetRenderer
{
    private static readonly Random _random = new();

    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService, float layerOpacity)
    {
        widget.Envelope = new MRect(0, 0, viewport.Width, viewport.Height);
        // A new random colour on every draw call — this is intentional: it makes each
        // render cycle visually distinct, which clearly demonstrates when a partial vs
        // full repaint occurs.
        var alpha = (byte)Math.Clamp((int)Math.Round(80 * layerOpacity), 0, 255);
        var color = new SKColor((byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256), alpha);
        using var paint = new SKPaint { Color = color };
        canvas.DrawRect(new SKRect(0, 0, (float)viewport.Width, (float)viewport.Height), paint);
    }
}

internal sealed class FullScreenColorWidgetExperimentalRenderer : Mapsui.Experimental.Rendering.Skia.SkiaWidgets.ISkiaWidgetRenderer
{
    private static readonly Random _random = new();

    public void Draw(SKCanvas canvas, Viewport viewport, IWidget widget, RenderService renderService, float layerOpacity, SKRect? dirtyScreenRect)
    {
        widget.Envelope = new MRect(0, 0, viewport.Width, viewport.Height);
        // A new random colour on every draw call — this is intentional: it makes each
        // render cycle visually distinct, which clearly demonstrates when a partial vs
        // full repaint occurs.
        var alpha = (byte)Math.Clamp((int)Math.Round(80 * layerOpacity), 0, 255);
        var color = new SKColor((byte)_random.Next(256), (byte)_random.Next(256), (byte)_random.Next(256), alpha);
        using var paint = new SKPaint { Color = color };
        canvas.DrawRect(new SKRect(0, 0, (float)viewport.Width, (float)viewport.Height), paint);
    }
}
