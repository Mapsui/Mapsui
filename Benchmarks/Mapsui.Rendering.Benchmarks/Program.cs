using BenchmarkDotNet.Running;
using Mapsui.Rendering.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        var renderPerformance = new RenderPerformance();
        renderPerformance.RenderRasterizingTilingSkp();
        renderPerformance.RenderRasterizingTilingPng();
        renderPerformance.RenderRasterizingTilingWebP();
        renderPerformance.RenderDefault();
#else
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
#endif
    }
}