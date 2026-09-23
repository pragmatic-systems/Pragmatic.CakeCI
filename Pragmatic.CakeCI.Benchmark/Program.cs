using BenchmarkDotNet.Running;

namespace Pragmatic.CQRS.Benchmark;

public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine(string.Join(",", args));

        BenchmarkRunner.Run(typeof(Program).Assembly, args: args);
    }
}
