using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Moq;

namespace Pragsys.CakeCI.Tests;

public class CoreAliasesTests
{
    private readonly Mock<ICakeContext> _context;
    private readonly Mock<ICakeLog> _log;
    private readonly Mock<ICakeEnvironment> _environment;
    private readonly Mock<IProcessRunner> _processRunner;
    private readonly Mock<IProcess> _process;
    private readonly Mock<IGlobber> _globber;

    public CoreAliasesTests()
    {
        _context = new Mock<ICakeContext>();
        _log = new Mock<ICakeLog>();
        _environment = new Mock<ICakeEnvironment>();
        _processRunner = new Mock<IProcessRunner>();
        _process = new Mock<IProcess>();
        _globber = new Mock<IGlobber>();

        _processRunner
            .Setup(pr => pr.Start(It.IsAny<FilePath>(), It.IsAny<ProcessSettings>()))
            .Returns(_process.Object);

        _environment.Setup(e => e.WorkingDirectory).Returns(new DirectoryPath("."));

        _context.Setup(c => c.Log).Returns(_log.Object);
        _context.Setup(c => c.ProcessRunner).Returns(_processRunner.Object);
        _context.Setup(c => c.Environment).Returns(_environment.Object);
        _context.Setup(c => c.Globber).Returns(_globber.Object);
    }

    [Fact]
    public void CiVersion_WithOverride_ReturnsOverride()
    {
        var result = _context.Object.CiVersion("1.2.3");

        result.ShouldBe("1.2.3");
        _log.Verify(l => l.Write(It.IsAny<Verbosity>(), LogLevel.Information, It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public void CiVersion_FromGitVersion_ReturnsSemVer()
    {
        _process.Setup(p => p.GetExitCode()).Returns(0);
        _process.Setup(p => p.GetStandardOutput()).Returns(["{\"SemVer\":\"1.0.0-beta.1+3\"}"]);
        _process.Setup(p => p.GetStandardError()).Returns([]);

        var result = _context.Object.CiVersion();

        result.ShouldBe("1.0.0-beta.1+3");
        _processRunner.ExecutedOnce();
    }

    [Fact]
    public void CiVersion_WhenProcessFails_ThrowsCakeException()
    {
        _process.Setup(p => p.GetExitCode()).Returns(1);
        _process.Setup(p => p.GetStandardOutput()).Returns([]);
        _process.Setup(p => p.GetStandardError()).Returns(["error"]);

        Should.Throw<Cake.Core.CakeException>(() => _context.Object.CiVersion())
            .Message.ShouldContain("GitVersion failed");
    }

    [Fact]
    public void CiVersion_WhenJsonIsInvalid_ThrowsCakeException()
    {
        _process.Setup(p => p.GetExitCode()).Returns(0);
        _process.Setup(p => p.GetStandardOutput()).Returns(["not valid json"]);
        _process.Setup(p => p.GetStandardError()).Returns([]);

        Should.Throw<Cake.Core.CakeException>(() => _context.Object.CiVersion())
            .Message.ShouldContain("Failed to parse GitVersion output");
    }

    [Fact]
    public void CiVersion_WhenSemVerIsNull_ThrowsCakeException()
    {
        _process.Setup(p => p.GetExitCode()).Returns(0);
        _process.Setup(p => p.GetStandardOutput()).Returns(["{\"SemVer\": null}"]);
        _process.Setup(p => p.GetStandardError()).Returns([]);

        Should.Throw<Cake.Core.CakeException>(() => _context.Object.CiVersion())
            .Message.ShouldContain("null SemVer");
    }

    [Fact]
    public void CiLint_WhenProcessSucceeds_LogsSuccess()
    {
        _process.Setup(p => p.GetExitCode()).Returns(0);
        _process.Setup(p => p.GetStandardOutput()).Returns([]);
        _process.Setup(p => p.GetStandardError()).Returns([]);

        _context.Object.CiLint();

        var message = "Lint check passed";
        _processRunner.ExecutedOnce();
        _log.LogHasMessage(message);
    }

    [Fact]
    public void CiTest_WhenProcessSucceeds_LogsSuccess()
    {
        _process.Setup(p => p.GetExitCode()).Returns(0);
        _process.Setup(p => p.GetStandardOutput()).Returns([]);
        _process.Setup(p => p.GetStandardError()).Returns([]);

        _globber.Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new Cake.Core.IO.Path[] { new FilePath("./src/MyApp.Tests/MyApp.Tests.csproj") });

        _context.Object.CiTest();

        var message = "tests passed";
        _processRunner.ExecutedOnce();
        _log.LogHasMessage(message);
    }

    [Fact]
    public void CiBenchmark_WhenProcessSucceeds_LogsSuccess()
    {
        _process.Setup(p => p.GetExitCode()).Returns(0);
        _process.Setup(p => p.GetStandardOutput()).Returns([]);
        _process.Setup(p => p.GetStandardError()).Returns([]);

        _globber.Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new Cake.Core.IO.Path[] { new FilePath("./src/MyApp.Benchmark/MyApp.Benchmark.csproj") });

        _context.Object.CiBenchmark();

        var message = "Benchmarking";
        _processRunner.ExecutedOnce();
        _log.LogHasMessage(message);
    }
}