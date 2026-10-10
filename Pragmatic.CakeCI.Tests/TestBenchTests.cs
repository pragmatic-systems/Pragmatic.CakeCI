using Pragmatic.CQRS.Benchmark;

namespace Pragmatic.CakeCI.Tests;

public class TestBenchTests
{
    [Fact]
    public async Task DoNothing_ShouldReturnCompletedTask()
    {
        var bench = new TestBench();

        var result = bench.DoNothing();

        result.IsCompleted.ShouldBeTrue();
        await result;
    }

    [Fact]
    public void AllocateMemory_ShouldReturn4KBuffer()
    {
        var bench = new TestBench();

        var result = bench.AllocateMemory();

        result.Length.ShouldBe(4096);
    }
}
