using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Moq;

namespace Pragsys.CakeCI.Tests;

public class NugetAliasesTests
{
    private readonly Mock<ICakeContext> _context;
    private readonly Mock<ICakeLog> _log;
    private readonly Mock<ICakeEnvironment> _environment;
    private readonly Mock<IProcessRunner> _processRunner;
    private readonly Mock<IProcess> _process;
    private readonly Mock<IGlobber> _globber;

    public NugetAliasesTests()
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
    public void CiNugetPack_WithSinglePackage_RunsPackCommand()
    {
        var manifest = new BuildManifest
        {
            NugetPackages = new[] { "./src/MyApp/MyApp.csproj" }
        };
        var packagesFolder = "./artifacts/packages";
        var version = "1.0.0";

        _process.Setup(p => p.GetExitCode()).Returns(0);
        _process.Setup(p => p.GetStandardOutput()).Returns([]);
        _process.Setup(p => p.GetStandardError()).Returns([]);

        _context.Object.CiNugetPack(manifest, packagesFolder, version);

        _processRunner.ExecutedOnce();
        _log.LogHasMessage($"Packing {manifest.NugetPackages[0]}...");
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

            _process.Setup(p => p.GetExitCode()).Returns(0);
            _process.Setup(p => p.GetStandardOutput()).Returns([]);
            _process.Setup(p => p.GetStandardError()).Returns([]);

            _context.Object.CiNugetPush(args, tempDir);

            _processRunner.Verify(
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