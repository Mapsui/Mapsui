using BenchmarkDotNet.Running;
using Mapsui.Rendering.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var performance = new RenderToBitmapPerformance();
        performance.RenderRasterizingTilingSkpAsync().Wait();

        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}