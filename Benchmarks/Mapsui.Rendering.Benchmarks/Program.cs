using BenchmarkDotNet.Running;
using Mapsui.Rendering.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
