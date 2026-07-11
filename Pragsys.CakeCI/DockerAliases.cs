using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Pragmatic.CakeCI;

/// <summary>
/// Aliases for building and pushing Docker container images.
/// </summary>
[CakeAliasCategory("PragsysCI")]
public static class DockerAliases
{
    /// <summary>
    /// Logs into a container registry using the provided credentials.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="args">The container registry arguments.</param>
    [CakeMethodAlias]
    [CakeAliasCategory("Docker")]
    public static void CiDockerLogin(this ICakeContext context, ContainerArgs args)
    {
        context.Log.Information($"Logging into container registry: {args.Registry}...");

        var dockerArgs = new ProcessArgumentBuilder()
            .Append("login")
            .Append(args.Registry)
            .Append("-u")
            .Append(args.UserName)
            .Append("-p")
            .Append(args.Token);

        ProcessHelper.Run(context, "docker", dockerArgs, $"Docker login failed for registry '{args.Registry}'");
        context.Log.Information("Docker login successful.");
    }

    /// <summary>
    /// Builds Docker images for all Dockerfiles listed in the build manifest.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="manifest">The build manifest containing Docker package paths.</param>
    /// <param name="args">The container registry arguments (for registry prefix).</param>
    /// <param name="versionNumber">The version tag to apply to the images.</param>
    [CakeMethodAlias]
    [CakeAliasCategory("Docker")]
    public static void CiDockerBuild(this ICakeContext context, BuildManifest manifest, ContainerArgs args, string versionNumber)
    {
        foreach (var dockerfilePath in manifest.DockerPackages)
        {
            var packageName = GetDockerPackageName(context, dockerfilePath);
            var fullImageName = GetFullImageName(args, packageName);
            var tag = $"{fullImageName}:{versionNumber}";

            context.Log.Information($"Building Docker image: {tag}");

            var dockerArgs = new ProcessArgumentBuilder()
                .Append("build")
                .Append("-t")
                .Append(tag)
                .Append("-f")
                .Append(dockerfilePath)
                .Append(".");

            ProcessHelper.Run(context, "docker", dockerArgs, $"Docker build failed for '{dockerfilePath}'");
            context.Log.Information($"Docker image built successfully: {tag}");
        }
    }

    /// <summary>
    /// Pushes Docker images for all Dockerfiles listed in the build manifest.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="manifest">The build manifest containing Docker package paths.</param>
    /// <param name="args">The container registry arguments (for registry prefix).</param>
    /// <param name="versionNumber">The version tag that was used during build.</param>
    [CakeMethodAlias]
    [CakeAliasCategory("Docker")]
    public static void CiDockerPush(this ICakeContext context, BuildManifest manifest, ContainerArgs args, string versionNumber)
    {
        foreach (var dockerfilePath in manifest.DockerPackages)
        {
            var packageName = GetDockerPackageName(context, dockerfilePath);
            var fullImageName = GetFullImageName(args, packageName);
            var tag = $"{fullImageName}:{versionNumber}";

            context.Log.Information($"Pushing Docker image: {tag}");

            var dockerArgs = new ProcessArgumentBuilder()
                .Append("push")
                .Append(tag);

            ProcessHelper.Run(context, "docker", dockerArgs, $"Docker push failed for '{tag}'");
            context.Log.Information($"Docker image pushed successfully: {tag}");
        }
    }

    /// <summary>
    /// Extracts a package name from a Dockerfile path by taking the parent directory name.
    /// </summary>
    private static string GetDockerPackageName(ICakeContext context, string dockerfilePath)
    {
        var directoryName = System.IO.Path.GetDirectoryName(dockerfilePath);
        if (string.IsNullOrEmpty(directoryName))
        {
            context.Log.Warning($"Could not determine package name from Dockerfile path: {dockerfilePath}. Using 'default-image'.");
            return "default-image";
        }

        // Path.GetFileName handles both '/' and '\' separators regardless of platform,
        // so this works even when cakemix paths use forward slashes on Windows.
        return System.IO.Path.GetFileName(directoryName).ToLowerInvariant();
    }

    /// <summary>
    /// Builds the full image name with optional registry prefix.
    /// Falls back to local image name when registry is not configured.
    /// </summary>
    internal static string GetFullImageName(ContainerArgs args, string packageName)
    {
        var name = packageName.ToLowerInvariant();
        if (!string.IsNullOrEmpty(args.Registry))
        {
            name = $"{args.Registry}/{name}";
        }
        return name;
    }
}
