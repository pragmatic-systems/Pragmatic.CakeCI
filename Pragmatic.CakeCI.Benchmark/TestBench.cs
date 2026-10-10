using BenchmarkDotNet.Attributes;

namespace Pragmatic.CQRS.Benchmark;

public class TestBench
{
    [Benchmark]
    public Task DoNothing()
    {
        return Task.CompletedTask;
    }

    [Benchmark]
    public byte[] AllocateMemory()
    {
        return new byte[4096];
    }
}