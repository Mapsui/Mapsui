using BenchmarkDotNet.Running;

public class Program
{
    public static void Main()
    {
        _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
