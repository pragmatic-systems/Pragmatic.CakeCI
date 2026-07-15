using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using NSubstitute;

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

        Process.GetExitCode().Returns(0);
        Process.GetStandardOutput().Returns([]);
        Process.GetStandardError().Returns([]);

        Context.CiNugetPack(manifest, packagesFolder, version);

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

            Process.GetExitCode().Returns(0);
            Process.GetStandardOutput().Returns([]);
            Process.GetStandardError().Returns([]);

            Context.CiNugetPush(args, tempDir);

            ProcessRunner.Received(2).Start(Arg.Any<FilePath>(), Arg.Any<ProcessSettings>());
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
    private readonly ICakeContext _context;
    private readonly ICakeArguments _cakeArguments;
    private readonly ICakeEnvironment _cakeEnvironment;

    public CiArgumentAliasesTests()
    {
        _context = Substitute.For<ICakeContext>();
        _cakeArguments = Substitute.For<ICakeArguments>();
        _cakeEnvironment = Substitute.For<ICakeEnvironment>();

        _context.Arguments.Returns(_cakeArguments);
        _context.Environment.Returns(_cakeEnvironment);
    }

    [Fact]
    public void Should_ConvertSimpleArg()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments.GetArguments(key).Returns(new[] { value });

        var result = _context.CiArgument(key);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_ConvertSimpleEnv()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments.GetArguments(key).Returns(new string[0]);
        _cakeEnvironment.GetEnvironmentVariable(key).Returns(value);

        var result = _context.CiArgument(key);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_ConvertGithubActionEnv()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments.GetArguments(key).Returns(new string[0]);
        _cakeEnvironment.GetEnvironmentVariable($"INPUT_{key}".ToUpperInvariant()).Returns(value);

        var result = _context.CiArgument(key);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_ReturnDefaultWhenSupplied()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments.GetArguments(key).Returns(new string[0]);

        var result = _context.CiArgument(key, value);
        result.ShouldBe(value);
    }
}
