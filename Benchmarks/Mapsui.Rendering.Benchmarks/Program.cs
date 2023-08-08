using BenchmarkDotNet.Running;
using Mapsui.Rendering.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
#if DEBUG
        var benchmark = new RenderToBitmapPerformance();
        benchmark.RenderDefaultAsync().Wait();
#else
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
#endif
    }
}
