using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Moq;

namespace Pragmatic.CakeCI.Tests;

public class CoreAliasesTests : CakeContextTestBase
    {

    [Fact]
    public void CiVersion_WithOverride_ReturnsOverride()
    {
        var result = Context.Object.CiVersion("1.2.3");

        result.ShouldBe("1.2.3");
        Log.Verify(l => l.Write(It.IsAny<Verbosity>(), LogLevel.Information, It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void CiVersion_FromGitVersion_ReturnsSemVer()
    {
        Process.Setup(p => p.GetExitCode()).Returns(0);
        Process.Setup(p => p.GetStandardOutput()).Returns(["{\"SemVer\":\"1.0.0-beta.1+3\"}"]);
        Process.Setup(p => p.GetStandardError()).Returns([]);

        var result = Context.Object.CiVersion();

        result.ShouldBe("1.0.0-beta.1+3");
        ProcessRunner.ExecutedOnce();
    }

    [Fact]
    public void CiVersion_WhenProcessFails_ThrowsCakeException()
    {
        Process.Setup(p => p.GetExitCode()).Returns(1);
        Process.Setup(p => p.GetStandardOutput()).Returns([]);
        Process.Setup(p => p.GetStandardError()).Returns(["error"]);

        Should.Throw<Cake.Core.CakeException>(() => Context.Object.CiVersion())
            .Message.ShouldContain("GitVersion failed");
    }

    [Fact]
    public void CiVersion_WhenJsonIsInvalid_ThrowsCakeException()
    {
        Process.Setup(p => p.GetExitCode()).Returns(0);
        Process.Setup(p => p.GetStandardOutput()).Returns(["not valid json"]);
        Process.Setup(p => p.GetStandardError()).Returns([]);

        Should.Throw<Cake.Core.CakeException>(() => Context.Object.CiVersion())
            .Message.ShouldContain("Failed to parse GitVersion output");
    }

    [Fact]
    public void CiVersion_WhenSemVerIsNull_ThrowsCakeException()
    {
        Process.Setup(p => p.GetExitCode()).Returns(0);
        Process.Setup(p => p.GetStandardOutput()).Returns(["{\"SemVer\": null}"]);
        Process.Setup(p => p.GetStandardError()).Returns([]);

        Should.Throw<Cake.Core.CakeException>(() => Context.Object.CiVersion())
            .Message.ShouldContain("null SemVer");
    }

    [Fact]
    public void CiLint_WhenProcessSucceeds_LogsSuccess()
    {
        Process.Setup(p => p.GetExitCode()).Returns(0);
        Process.Setup(p => p.GetStandardOutput()).Returns([]);
        Process.Setup(p => p.GetStandardError()).Returns([]);

        Context.Object.CiLint();

        var message = "Lint check passed";
        ProcessRunner.ExecutedOnce();
        Log.LogHasMessage(message);
    }

    [Fact]
    public void CiTest_WhenProcessSucceeds_LogsSuccess()
    {
        Process.Setup(p => p.GetExitCode()).Returns(0);
        Process.Setup(p => p.GetStandardOutput()).Returns([]);
        Process.Setup(p => p.GetStandardError()).Returns([]);

        Globber.Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new Cake.Core.IO.Path[] { new FilePath("./src/MyApp.Tests/MyApp.Tests.csproj") });

        Context.Object.CiTest();

        var message = "tests passed";
        ProcessRunner.ExecutedOnce();
        Log.LogHasMessage(message);
    }

    [Fact]
    public void CiBenchmark_WhenProcessSucceeds_LogsSuccess()
    {
        Process.Setup(p => p.GetExitCode()).Returns(0);
        Process.Setup(p => p.GetStandardOutput()).Returns([]);
        Process.Setup(p => p.GetStandardError()).Returns([]);

        Globber.Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new Cake.Core.IO.Path[] { new FilePath("./src/MyApp.Benchmark/MyApp.Benchmark.csproj") });

        Context.Object.CiBenchmark();

        var message = "Benchmarking";
        ProcessRunner.ExecutedOnce();
        Log.LogHasMessage(message);
    }
}