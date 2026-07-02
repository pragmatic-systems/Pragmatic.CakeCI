using Cake.Core;
using Moq;

namespace Pragsys.CakeCI.Tests;

public class ManifestAliasesTests
{
    private readonly Mock<ICakeContext> _context;
    private readonly Mock<ICakeEnvironment> _environment;

    public ManifestAliasesTests()
    {
        _context = new Mock<ICakeContext>();
        _environment = new Mock<ICakeEnvironment>();

        _context.Setup(c => c.Environment).Returns(_environment.Object);
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
            var result = _context.Object.LoadBuildManifest(tempFile);

            result.Should().NotBeNull();
            result.NugetPackages.Should().ContainEquivalentOf("./src/A.csproj");
            result.DockerPackages.Should().ContainEquivalentOf("./src/B/Dockerfile");
            result.Benchmarks.Should().BeEmpty();
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
            var result = _context.Object.LoadBuildManifest(tempFile);

            result.Should().NotBeNull();
            File.Exists(tempFile).Should().BeTrue("manifest file should be auto-created");
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
            var result = _context.Object.LoadBuildManifest();

            result.Should().NotBeNull();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }
}
