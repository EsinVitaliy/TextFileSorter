using BenchmarkDotNet.Running;

namespace TextFileSorter.DecisionBenchmarks
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
        }
    }
}
