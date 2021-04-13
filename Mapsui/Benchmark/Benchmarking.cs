using System.Collections.Generic;

namespace Mapsui.Benchmark
{
    public class RenderBenchmark
    {
        public string Name { get; set; }
        public double Time { get; set; }
        public string TimeFixed => Time.ToString("N5");

        public int FeatureCount { get; set; }
        public int StyleCount { get; set; }

        public Dictionary<string, int> StyleTypeCount;
        public Dictionary<string, double> StyleTypeTime;
    }

    public static class Benchmarking
    {
        public static bool Enabled = false;
        public static List<List<RenderBenchmark>> AllBenchmarks { get; } = new();
    }
}
