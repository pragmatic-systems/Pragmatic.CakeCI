using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Moq;

namespace Pragmatic.CakeCI.Tests;

public class SonarAliasesTests : CakeContextTestBase
{
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
        var proj = "C:\\temp\\sample.csproj";

        Globber
            .Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new[] { new FilePath(proj) });

        Context.Object.CiSonarScannerBegin(sonarArgs, artifactsFolder);

        ProcessRunner.ExecutedOnce();
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

        Globber
            .Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new[] { new FilePath(sonarDll) });

        Context.Object.CiSonarScannerEnd(sonarArgs);

        ProcessRunner.ExecutedOnce();
        Log.LogHasMessage("Sonar analysis completed successfully.");
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

        Globber
            .Setup(g => g.Match(It.IsAny<GlobPattern>(), It.IsAny<GlobberSettings>()))
            .Returns(new[] { new FilePath(sonarDll) });


        Process
            .Setup(pr => pr.GetExitCode())
            .Returns(1);

        Should
            .Throw<CakeException>(() => Context.Object.CiSonarScannerEnd(sonarArgs))
            .Message.ShouldBe("Sonar analysis failed. (exit code 1)");

        ProcessRunner.ExecutedOnce();
    }
}
