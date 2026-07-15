using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Moq;

namespace Pragmatic.CakeCI.Tests;

public class NugetAliasesTests : CakeContextTestBase
    {

    [Fact]
    public void CiNugetPack_WithSinglePackage_RunsPackCommand()
    {
        var manifest = new BuildManifest
        {
            NugetPackages = new[] { "./src/MyApp/MyApp.csproj" }
        };
        var packagesFolder = "./artifacts/packages";
        var version = "1.0.0";

        Process.Setup(p => p.GetExitCode()).Returns(0);
        Process.Setup(p => p.GetStandardOutput()).Returns([]);
        Process.Setup(p => p.GetStandardError()).Returns([]);

        Context.Object.CiNugetPack(manifest, packagesFolder, version);

        ProcessRunner.ExecutedOnce();
        Log.LogHasMessage($"Packing {manifest.NugetPackages[0]}...");
    }

    [Fact]
    public void CiNugetPush_WithPackages_PushesEachPackage()
    {
        var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"nuget-push-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(System.IO.Path.Combine(tempDir, "MyApp.1.0.0.nupkg"), "");
        File.WriteAllText(System.IO.Path.Combine(tempDir, "Other.2.0.0.nupkg"), "");

        try
        {
            var args = new NugetArgs
            {
                Source = "https://api.nuget.org/v3/index.json",
                ApiKey = "my-api-key",
            };

            Process.Setup(p => p.GetExitCode()).Returns(0);
            Process.Setup(p => p.GetStandardOutput()).Returns([]);
            Process.Setup(p => p.GetStandardError()).Returns([]);

            Context.Object.CiNugetPush(args, tempDir);

            ProcessRunner.Verify(
                pr => pr.Start(It.IsAny<FilePath>(), It.IsAny<ProcessSettings>()),
                Times.Exactly(2));
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}


public class CiArgumentAliasesTests
{
    private readonly Mock<ICakeContext> _context;
    private readonly Mock<ICakeArguments> _cakeArguments;
    private readonly Mock<ICakeEnvironment> _cakeEnvironment;

    public CiArgumentAliasesTests()
    {
        _context = new Mock<ICakeContext>();
        _cakeArguments = new Mock<ICakeArguments>();
        _cakeEnvironment = new Mock<ICakeEnvironment>();

        _context.Setup(c => c.Arguments).Returns(_cakeArguments.Object);
        _context.Setup(c => c.Environment).Returns(_cakeEnvironment.Object);
    }

    [Fact]
    public void Should_ConvertSimpleArg()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments
            .Setup(a => a.GetArguments(key))
            .Returns(new[] { value });

        var result = _context.Object.CiArgument(key);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_ConvertSimpleEnv()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments
            .Setup(a => a.GetArguments(key))
            .Returns(new string[0]);

        _cakeEnvironment
            .Setup(a => a.GetEnvironmentVariable(key))
            .Returns(value);

        var result = _context.Object.CiArgument(key);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_ConvertGithubActionEnv()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments
            .Setup(a => a.GetArguments(key))
            .Returns(new string[0]);

        _cakeEnvironment
            .Setup(a => a.GetEnvironmentVariable($"INPUT_{key}".ToUpperInvariant()))
            .Returns(value);

        var result = _context.Object.CiArgument(key);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_ReturnDefaultWhenSupplied()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments
            .Setup(a => a.GetArguments(key))
            .Returns(new string[0]);

        var result = _context.Object.CiArgument(key, value);
        result.ShouldBe(value);
    }
}