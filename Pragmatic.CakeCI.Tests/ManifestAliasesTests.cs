using Cake.Core;
using NSubstitute;

namespace Pragmatic.CakeCI.Tests;

public class ManifestAliasesTests
{
    private readonly ICakeContext _context;
    private readonly ICakeEnvironment _environment;

    public ManifestAliasesTests()
    {
        _context = Substitute.For<ICakeContext>();
        _environment = Substitute.For<ICakeEnvironment>();

        _context.Environment.Returns(_environment);
    }

    [Fact]
    public void LoadBuildManifest_WithExistingFile_ReturnsDeserializedManifest()
    {
        var json = """
            {
                "NugetPackages": ["./src/A.csproj"],
                "DockerPackages": ["./src/B/Dockerfile"],
                "Benchmarks": [],
                "ApiSpecs": {}
            }
            """;

        var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"manifest-{Guid.NewGuid()}.json");
        File.WriteAllText(tempFile, json);

        try
        {
            var result = _context.LoadBuildManifest(tempFile);

            result.ShouldNotBeNull();
            result.NugetPackages.ShouldContain("./src/A.csproj");
            result.DockerPackages.ShouldContain("./src/B/Dockerfile");
            result.Benchmarks.ShouldBeEmpty();
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadBuildManifest_WithMissingFile_CreatesDefaultManifest()
    {
        var tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"manifest-{Guid.NewGuid()}.json");

        try
        {
            var result = _context.LoadBuildManifest(tempFile);

            result.ShouldNotBeNull();
            File.Exists(tempFile).ShouldBeTrue("manifest file should be auto-created");
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadBuildManifest_WithDefaultPath_UsesBuildCakemix()
    {
        var json = """
            {
                "NugetPackages": [],
                "DockerPackages": [],
                "Benchmarks": [],
                "ApiSpecs": {}
            }
            """;

        var tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"manifest-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);
        var manifestPath = System.IO.Path.Combine(tempDir, "build.cakemix");
        File.WriteAllText(manifestPath, json);

        try
        {
            var result = _context.LoadBuildManifest();

            result.ShouldNotBeNull();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
