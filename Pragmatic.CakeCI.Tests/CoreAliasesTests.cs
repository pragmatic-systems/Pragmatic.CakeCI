using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using NSubstitute;

namespace Pragmatic.CakeCI.Tests;

public class CoreAliasesTests : CakeContextTestBase
{
    [Fact]
    public void CiVersion_WithOverride_ReturnsOverride()
    {
        var result = Context.CiVersion("1.2.3");

        result.ShouldBe("1.2.3");
        Log.Received(1).Write(Arg.Any<Verbosity>(), LogLevel.Information, Arg.Any<string>(), Arg.Any<object[]>());
    }

    [Fact]
    public void CiVersion_FromGitVersion_ReturnsSemVer()
    {
        Process.GetExitCode().Returns(0);
        Process.GetStandardOutput().Returns(["{\"SemVer\":\"1.0.0-beta.1+3\"}"]);
        Process.GetStandardError().Returns([]);

        var result = Context.CiVersion();

        result.ShouldBe("1.0.0-beta.1+3");
        ProcessRunner.ExecutedOnce();
    }

    [Fact]
    public void CiVersion_WhenProcessFails_ThrowsCakeException()
    {
        Process.GetExitCode().Returns(1);
        Process.GetStandardOutput().Returns([]);
        Process.GetStandardError().Returns(["error"]);

        Should.Throw<Cake.Core.CakeException>(() => Context.CiVersion())
            .Message.ShouldContain("GitVersion failed");
    }

    [Fact]
    public void CiVersion_WhenJsonIsInvalid_ThrowsCakeException()
    {
        Process.GetExitCode().Returns(0);
        Process.GetStandardOutput().Returns(["not valid json"]);
        Process.GetStandardError().Returns([]);

        Should.Throw<Cake.Core.CakeException>(() => Context.CiVersion())
            .Message.ShouldContain("Failed to parse GitVersion output");
    }

    [Fact]
    public void CiVersion_WhenSemVerIsNull_ThrowsCakeException()
    {
        Process.GetExitCode().Returns(0);
        Process.GetStandardOutput().Returns(["{\"SemVer\": null}"]);
        Process.GetStandardError().Returns([]);

        Should.Throw<Cake.Core.CakeException>(() => Context.CiVersion())
            .Message.ShouldContain("null SemVer");
    }

    [Fact]
    public void CiLint_WhenProcessSucceeds_LogsSuccess()
    {
        Process.GetExitCode().Returns(0);
        Process.GetStandardOutput().Returns([]);
        Process.GetStandardError().Returns([]);

        Context.CiLint();

        var message = "Lint check passed";
        ProcessRunner.ExecutedOnce();
        Log.LogHasMessage(message);
    }

    [Fact]
    public void CiTest_WhenProcessSucceeds_LogsSuccess()
    {
        Process.GetExitCode().Returns(0);
        Process.GetStandardOutput().Returns([]);
        Process.GetStandardError().Returns([]);

        Globber.Match(Arg.Any<GlobPattern>(), Arg.Any<GlobberSettings>())
            .Returns(new Cake.Core.IO.Path[] { new FilePath("./src/MyApp.Tests/MyApp.Tests.csproj") });

        Context.CiTest();

        var message = "tests passed";
        ProcessRunner.ExecutedOnce();
        Log.LogHasMessage(message);
    }

    [Fact]
    public void CiBenchmark_WhenProcessSucceeds_LogsSuccess()
    {
        Process.GetExitCode().Returns(0);
        Process.GetStandardOutput().Returns([]);
        Process.GetStandardError().Returns([]);

        Globber.Match(Arg.Any<GlobPattern>(), Arg.Any<GlobberSettings>())
            .Returns(new Cake.Core.IO.Path[] { new FilePath("./src/MyApp.Benchmark/MyApp.Benchmark.csproj") });

        Context.CiBenchmark();

        var message = "Benchmarking";
        ProcessRunner.ExecutedOnce();
        Log.LogHasMessage(message);
    }
}
