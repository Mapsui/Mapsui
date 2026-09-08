using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Mapsui.Layers;
using Mapsui.Rendering.Skia;
using Mapsui.Widgets;
using Mapsui.Widgets.ScaleBar;
using SkiaSharp;

namespace Mapsui.Rendering.Benchmarks;

[SimpleJob(RunStrategy.Throughput)]
[MemoryDiagnoser]
[MinColumn, MaxColumn, MeanColumn, MedianColumn]
public sealed class ScaleBarFontPerformance : IDisposable
{
    private readonly Map _map = new() { CRS = "EPSG:3857" };
    private readonly MapRenderer _renderer = new();
    private readonly RenderService _renderServiceWithoutCache = new();
    private readonly SKSurface _surface = SKSurface.Create(new SKImageInfo(800, 600));
    private readonly IWidget[] _widgets;

    public ScaleBarFontPerformance()
    {
        _map.Navigator.SetSize(800, 600);
        _map.Navigator.CenterOnAndZoomTo(new MPoint(0, 0), 1000);
        _widgets = [new ScaleBarWidget(_map)];
        _renderServiceWithoutCache.VectorCache.Enabled = false;
    }

    [Benchmark(Baseline = true)]
    public void RenderWithoutFontCache()
    {
        _renderer.Render(_surface.Canvas, _map.Navigator.Viewport, Array.Empty<ILayer>(), _widgets,
            _renderServiceWithoutCache);
    }

    [Benchmark]
    public void RenderWithFontCache()
    {
        _renderer.Render(_surface.Canvas, _map.Navigator.Viewport, Array.Empty<ILayer>(), _widgets,
            _map.RenderService);
    }

    public void Dispose()
    {
        _surface.Dispose();
        _renderServiceWithoutCache.Dispose();
        _map.Dispose();
    }
}
