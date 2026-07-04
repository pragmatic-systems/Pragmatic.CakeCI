using System.Text.Json;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;

namespace Pragsys.CakeCI;

/// <summary>
/// Aliases for loading and managing the build manifest (build.cakemix).
/// Auto-creates a default manifest with discovered projects if the file does not exist.
/// </summary>
[CakeAliasCategory("PragsysCI")]
public static class ManifestAliases
{
    private const string DefaultManifestFile = "build.cakemix";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Loads the build manifest from a cakemix file.
    /// Creates a default manifest with auto-discovered Dockerfiles and benchmark projects if the file does not exist.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="manifestFile">Path to the manifest file. Defaults to <c>build.cakemix</c>.</param>
    /// <returns>The loaded or newly created build manifest.</returns>
    [CakeMethodAlias]
    [CakeAliasCategory("Manifest")]
    public static BuildManifest LoadBuildManifest(this ICakeContext context, string? manifestFile = null)
    {
        var file = manifestFile ?? DefaultManifestFile;

        if (!System.IO.File.Exists(file))
        {
            context.Log.Warning($"No cakemix file found at '{file}', creating default manifest...");
            var manifest = CreateDefaultManifest(context);
            var json = JsonSerializer.Serialize(manifest, JsonOptions);
            File.WriteAllText(file, json);
            return manifest;
        }

        using var fs = File.OpenRead(file); 
        return JsonSerializer.Deserialize<BuildManifest>(fs, JsonOptions)
            ?? throw new InvalidOperationException($"Failed to deserialize build manifest from '{file}'.");
    }

    private static BuildManifest CreateDefaultManifest(ICakeContext context)
    {
        return new BuildManifest
        {
            NugetPackages = [],
            DockerPackages = System.IO.Directory.GetFiles(".", "Dockerfile", SearchOption.AllDirectories),
            Benchmarks = System.IO.Directory.GetFiles(".", "*.Benchmark.csproj", SearchOption.AllDirectories),
        };
    }
}
