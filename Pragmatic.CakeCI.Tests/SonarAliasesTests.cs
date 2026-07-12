using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Moq;

namespace Pragmatic.CakeCI.Tests;

public class SonarAliasesTests
{
    private readonly Mock<ICakeContext> _context;
    private readonly Mock<ICakeLog> _log;
    private readonly Mock<ICakeEnvironment> _environment;
    private readonly Mock<IProcessRunner> _processRunner;
    private readonly Mock<IProcess> _process;
    private readonly Mock<IGlobber> _globber;

    public SonarAliasesTests()
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
    public void SonarScanBegin_ShouldRunSonarScanExecutable()
    {
        var sonarArgs = new SonarArgs
        {
            Org = "Org",
            Branch = "Branch",
            Token = "Token",
            ProjectName = "Name",
            ProjectKey = "Key",
            HostUrl = "localhost",
        };
        var artifactsFolder = "./artifacts/packages";
        var sonarDll = "dotnet-sonarscanner";

        _globber
            .Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new[] { new FilePath(sonarDll) });

        _context.Object.CiSonarScannerBegin(sonarArgs, artifactsFolder);

        _processRunner.ExecutedOnce();
    }

    [Fact]
    public void SonarScanEnd_WhenSuccesfull_ShouldRunSonarScanExecutable()
    {
        var sonarArgs = new SonarArgs
        {
            Org = "Org",
            Branch = "Branch",
            Token = "Token",
            ProjectName = "Name",
            ProjectKey = "Key",
            HostUrl = "localhost",
        };
        var artifactsFolder = "./artifacts/packages";
        var sonarDll = "SonarScanner.MSBuild.dll";

        _globber
            .Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new[] { new FilePath(sonarDll) });

        _context.Object.CiSonarScannerEnd(sonarArgs);

        _processRunner.ExecutedOnce();
        _log.LogHasMessage("Sonar analysis completed successfully.");
    }

    [Fact]
    public void SonarScanEnd_WhenError_ShouldFailSonarScan()
    {
        var sonarArgs = new SonarArgs
        {
            Org = "Org",
            Branch = "Branch",
            Token = "Token",
            ProjectName = "Name",
            ProjectKey = "Key",
            HostUrl = "localhost",
        };
        var artifactsFolder = "./artifacts/packages";
        var sonarDll = "SonarScanner.MSBuild.dll";

        _globber
            .Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new[] { new FilePath(sonarDll) });


        _process
            .Setup(pr => pr.GetExitCode())
            .Returns(1);

        Should
            .Throw<CakeException>(() => _context.Object.CiSonarScannerEnd(sonarArgs))
            .Message.ShouldBe("Sonar analysis failed. (exit code 1)");

        _processRunner.ExecutedOnce();
    }
}
