using BenchmarkDotNet.Running;

namespace Mapsui.Rendering.Benchmarks;
public class Program
{
    public static void Main()
    {
        _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
