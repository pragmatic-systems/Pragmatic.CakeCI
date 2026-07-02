namespace Pragsys.CakeCI;

/// <summary>
/// Represents the build manifest loaded from (or auto-generated as) <c>build.cakemix</c>.
/// </summary>
public class BuildManifest
{
    /// <summary>
    /// Paths to NuGet package projects (.csproj files to pack).
    /// </summary>
    public string[] NugetPackages { get; set; } = [];

    /// <summary>
    /// Paths to Dockerfiles for container images.
    /// </summary>
    public string[] DockerPackages { get; set; } = [];

    /// <summary>
    /// Paths to benchmark projects (*.Benchmark.csproj).
    /// </summary>
    public string[] Benchmarks { get; set; } = [];

    /// <summary>
    /// API specification mappings (project name to base URL).
    /// </summary>
    public Dictionary<string, string> ApiSpecs { get; set; } = [];
}
