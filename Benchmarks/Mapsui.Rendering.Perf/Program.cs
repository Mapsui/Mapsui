using Mapsui.Extensions;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Rendering.Skia;
using Mapsui.Styles;
using SkiaSharp;
using System.Diagnostics;

namespace Mapsui.Rendering.Perf;

/// <summary>
/// Performance test comparing full-canvas rendering against partial (dirty-rect) rendering.
///
/// Run with both scenarios in the same process — no stored baseline file is needed.
/// The FullRender time IS the reference; the PartialRender row below shows the speedup.
///
/// PartialRender is currently a placeholder (marked TODO) until Stage 3 is implemented.
/// When Stage 3 ships, add the PartialRender measurement and re-run to see the ratio.
/// </summary>
internal class Program
{
    // Spread 5000 random features across the full SphericalMercator world extent.
    // At city-region zoom only a small fraction of them fall inside the viewport,
    // and an even smaller fraction inside the GPS dirty rect.
    private const int BaseFeatureCount = 5_000;
    private const int WarmupIterations = 5;
    private const int MeasuredIterations = 30;

    // Viewport centred on European city at ~300 m/px (city-region overview)
    private const double CenterX = 1_000_000d;
    private const double CenterY = 6_000_000d;
    private const double Resolution = 300d;    // metres per pixel
    private const int ViewW = 800;
    private const int ViewH = 600;

    // Base layer: 5000 points spread around the viewport centre. Layer caches the
    // features fetched for the full viewport and returns the full cache from GetFeatures
    // regardless of the query extent. This is the real behaviour of the Layer class:
    // spatial filtering happens at fetch time (inside the provider), not at render time.
    // WritableLayer / tile layers filter spatially at GetFeatures call time, so they
    // benefit additionally from a dirty-rect query extent in Stage 3.
    private const double SpreadHalfM = 600_000d; // metres from centre

    // GPS position: somewhere inside the viewport
    private const double GpsX = 950_000d;
    private const double GpsY = 5_950_000d;

    // Symbol half-size in pixels — used to compute the dirty world rect
    private const double SymbolHalfPx = 8d;

    static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Mapsui Partial Rendering Performance Test");
        Console.WriteLine("=========================================");

        var viewport = new Viewport(CenterX, CenterY, Resolution, rotation: 0, width: ViewW, height: ViewH);
        var fullExtent = viewport.ToExtent()!;

        // Dirty rect: world-space bounding box that covers the GPS symbol
        // (old position ∪ new position — both the same here since we're just measuring)
        var halfM = SymbolHalfPx * Resolution;
        var dirtyRect = new MRect(GpsX - halfM, GpsY - halfM, GpsX + halfM, GpsY + halfM);

        using var baseLayer = CreateBaseLayer(BaseFeatureCount);
        using var gpsLayer = CreateGpsLayer(GpsX, GpsY);
        ILayer[] allLayers = [baseLayer, gpsLayer];

        // Layer fetches features asynchronously from its data source into an internal cache.
        // Prime that cache before measuring so the benchmark reflects steady-state rendering.
        var fetchInfo = new FetchInfo(new MSection(fullExtent, Resolution));
        await baseLayer.FetchAsync(fetchInfo, 1).ConfigureAwait(false);
        await gpsLayer.FetchAsync(fetchInfo, 1).ConfigureAwait(false);

        var renderer = new MapRenderer();
        using var renderService = new RenderService();

        using var bmp = new SKBitmap(ViewW, ViewH);
        using var canvas = new SKCanvas(bmp);

        // Warmup: ensures any lazy initialisation (style caches etc.) is paid before measuring
        for (var i = 0; i < WarmupIterations; i++)
            renderer.Render(canvas, viewport, allLayers, [], renderService, Color.White);

        // ── Information ──────────────────────────────────────────────────────────
        var countFull = allLayers.Sum(l => l.GetFeatures(fullExtent, Resolution).Count());
        var countPartial = allLayers.Sum(l => l.GetFeatures(dirtyRect, Resolution).Count());

        Console.WriteLine();
        Console.WriteLine($"  Viewport  : {ViewW}×{ViewH} px  @  {Resolution} m/px  " +
                          $"({ViewW * Resolution / 1_000:F0} km × {ViewH * Resolution / 1_000:F0} km)");
        Console.WriteLine($"  Base layer: {BaseFeatureCount} point features, spread ±{SpreadHalfM / 1_000:F0} km from viewport centre");
        Console.WriteLine($"  Layer type : Layer (fetches into cache; GetFeatures returns full cache, ignores query bbox)");
        Console.WriteLine($"  GPS layer : 1 point feature");
        Console.WriteLine($"  Dirty rect: {dirtyRect.Width / 1_000:F1} km × {dirtyRect.Height / 1_000:F1} km  " +
                          $"({SymbolHalfPx * 2:F0}×{SymbolHalfPx * 2:F0} px symbol)");
        Console.WriteLine();
        Console.WriteLine($"  Features in full viewport : {countFull,6}");
        Console.WriteLine($"  Features in dirty rect    : {countPartial,6}");
        if (countPartial > 0)
            Console.WriteLine($"  Ratio                     : {(double)countFull / countPartial,6:F0}×  fewer features in the partial path");
        Console.WriteLine();

        // ── Measurements ─────────────────────────────────────────────────────────
        var tGetFull = Measure(MeasuredIterations, () => { foreach (var l in allLayers) _ = l.GetFeatures(fullExtent, Resolution).Count(); });
        var tGetPartial = Measure(MeasuredIterations, () => { foreach (var l in allLayers) _ = l.GetFeatures(dirtyRect, Resolution).Count(); });
        var tFullRender = Measure(MeasuredIterations, () =>
            renderer.Render(canvas, viewport, allLayers, [], renderService, Color.White));

        // ── Results ───────────────────────────────────────────────────────────────
        Console.WriteLine("+----------------------------------------------------------------+");
        Console.WriteLine($"  {"Scenario",-40} {"Avg ms",8} {"Min ms",8} {"Max ms",8}");
        Console.WriteLine($"  {"--------",-40} {"------",8} {"------",8} {"------",8}");
        PrintRow("GetFeatures — full viewport", tGetFull);
        PrintRow("GetFeatures — dirty rect", tGetPartial);
        PrintRow("FullRender (all layers)", tFullRender);
        PrintRow("PartialRender (TODO: Stage 3)", null); // fill in after Stage 3
        Console.WriteLine("+----------------------------------------------------------------+");

        Console.WriteLine();
        Console.WriteLine("  Once Stage 3 is implemented:");
        Console.WriteLine($"    Expected PartialRender ≈ GetFeatures_dirty + render {countPartial} feature(s)");
        Console.WriteLine($"    Predicted speedup        ≈ {tFullRender.Avg / Math.Max(tGetPartial.Avg, 0.001):F0}×  " +
                          "(if render overhead ≈ GetFeatures cost)");
        Console.WriteLine();
        Console.WriteLine("Done.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────────

    private static Layer CreateBaseLayer(int count)
    {
        var rng = new Random(42);
        var features = Enumerable.Range(0, count)
            .Select(_ => (IFeature)new PointFeature(new MPoint(
                CenterX + (rng.NextDouble() * 2 - 1) * SpreadHalfM,
                CenterY + (rng.NextDouble() * 2 - 1) * SpreadHalfM)))
            .ToList();
        return new Layer("BaseLayer")
        {
            DataSource = new MemoryProvider(features),
            Style = new SymbolStyle(),
        };
    }

    private static Layer CreateGpsLayer(double x, double y)
    {
        return new Layer("GpsLayer")
        {
            DataSource = new MemoryProvider(new PointFeature(new MPoint(x, y))),
            Style = new SymbolStyle(),
        };
    }

    private record struct Timing(double Avg, double Min, double Max);

    private static Timing Measure(int n, Action action)
    {
        var sw = new Stopwatch();
        var times = new double[n];
        for (var i = 0; i < n; i++)
        {
            sw.Restart();
            action();
            sw.Stop();
            times[i] = sw.Elapsed.TotalMilliseconds;
        }
        return new Timing(times.Average(), times.Min(), times.Max());
    }

    private static void PrintRow(string label, Timing? t)
    {
        if (t is null)
            Console.WriteLine($"  {label,-40} {"—",8} {"—",8} {"—",8}");
        else
            Console.WriteLine($"  {label,-40} {t.Value.Avg,8:F3} {t.Value.Min,8:F3} {t.Value.Max,8:F3}");
    }
}
