using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using NSubstitute;

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

        Globber.Match(Arg.Any<GlobPattern>(), Arg.Any<GlobberSettings>())
            .Returns(new[] { new FilePath(proj) });

        Context.CiSonarScannerBegin(sonarArgs, artifactsFolder);

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

        Globber.Match(Arg.Any<GlobPattern>(), Arg.Any<GlobberSettings>())
            .Returns(new[] { new FilePath("SonarScanner.MSBuild.dll") });

        Context.CiSonarScannerEnd(sonarArgs);

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

        Globber.Match(Arg.Any<GlobPattern>(), Arg.Any<GlobberSettings>())
            .Returns(new[] { new FilePath("SonarScanner.MSBuild.dll") });

        Process.GetExitCode().Returns(1);

        Should
            .Throw<CakeException>(() => Context.CiSonarScannerEnd(sonarArgs))
            .Message.ShouldBe("Sonar analysis failed. (exit code 1)");

        ProcessRunner.ExecutedOnce();
    }
}
