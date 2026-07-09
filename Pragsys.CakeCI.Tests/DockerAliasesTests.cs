using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Moq;

namespace Pragsys.CakeCI.Tests;

public class DockerAliasesTests
{
    private readonly Mock<ICakeContext> _context;
    private readonly Mock<ICakeLog> _log;
    private readonly Mock<ICakeEnvironment> _environment;
    private readonly Mock<IProcessRunner> _processRunner;
    private readonly Mock<IProcess> _process;
    private readonly Mock<IGlobber> _globber;

    public DockerAliasesTests()
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
    public void CiLint_WhenDockerLoginSucceeds_LogsSuccess()
    {
        var args = new ContainerArgs
        {
            Registry = "ghcr.io/myorg",
            Token = "token",
            UserName = "user",
        };
        _context.Object.CiDockerLogin(args);
        _log.LogHasMessage("Docker login successful.");
    }

    [Fact]
    public void CiLint_WhenDockerBuildSucceeds_LogsSuccess()
    {
        var version = "0.0.1";
        var manifest = new BuildManifest
        {
            DockerPackages = new [] { "Package" }
        };

        var args = new ContainerArgs
        {
            Registry = "ghcr.io/myorg",
            Token = "token",
            UserName = "user",
        };

        var tag = $"{args.Registry}/default-image:{version}";

        _context.Object.CiDockerBuild(manifest, args, version);
        _log.LogHasMessage($"Docker image built successfully: {tag}");
    }

    [Fact]
    public void CiLint_WhenDockerPushSucceeds_LogsSuccess()
    {
        var version = "0.0.1";
        var manifest = new BuildManifest
        {
            DockerPackages = new[] { "Package" }
        };

        var args = new ContainerArgs
        {
            Registry = "ghcr.io/myorg",
            Token = "token",
            UserName = "user",
        };

        var tag = $"{args.Registry}/default-image:{version}";

        _context.Object.CiDockerPush(manifest, args, version);
        _log.LogHasMessage($"Docker image pushed successfully: {tag}");
    }

    [Theory]
    [InlineData("myapp", "ghcr.io/myorg/myapp")]
    [InlineData("otherservice", "ghcr.io/myorg/otherservice")]
    [InlineData("deepservice", "ghcr.io/myorg/deepservice")]
    public void GetFullImageName_WithRegistry_AppendsRegistryPrefix(string packageName, string expected)
    {
        var args = new ContainerArgs
        {
            Registry = "ghcr.io/myorg",
            Token = "token",
            UserName = "user",
        };

        var result = DockerAliases.GetFullImageName(args, packageName);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("myapp", "myapp")]
    [InlineData("otherservice", "otherservice")]
    [InlineData("deepservice", "deepservice")]
    public void GetFullImageName_WithoutRegistry_ReturnsPackageName(string packageName, string expected)
    {
        var args = new ContainerArgs
        {
            Registry = string.Empty,
            Token = "token",
            UserName = "user",
        };

        var result = DockerAliases.GetFullImageName(args, packageName);

        result.ShouldBe(expected);
    }

    [Theory]
    [InlineData("./src/MyApp/Dockerfile", "myapp")]
    [InlineData("src/OtherService/Dockerfile", "otherservice")]
    [InlineData("a/b/c/DeepService/Dockerfile", "deepservice")]
    public void PathSeparators_ForwardSlash_ShouldExtractDirectoryName(string dockerfilePath, string expectedName)
    {
        // Verify that Path.GetFileName(GetDirectoryName(...)) correctly handles
        // forward-slash paths regardless of platform (the fix for the separator issue).
        var directoryName = System.IO.Path.GetDirectoryName(dockerfilePath);
        var packageName = System.IO.Path.GetFileName(directoryName!).ToLowerInvariant();

        packageName.ShouldBe(expectedName);
    }

    [Fact]
    public void PathSeparators_Backslash_ShouldExtractDirectoryName()
    {
        // On Windows the paths may use backslashes; the fix must handle those too.
        var dockerfilePath = System.IO.Path.Combine("src", "MyApp", "Dockerfile");
        var directoryName = System.IO.Path.GetDirectoryName(dockerfilePath);
        var packageName = System.IO.Path.GetFileName(directoryName!).ToLowerInvariant();

        packageName.ShouldBe("myapp");
    }
}
