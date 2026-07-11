using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Pragmatic.CakeCI;

/// <summary>
/// Aliases for NuGet package packing and pushing.
/// </summary>
[CakeAliasCategory("PragsysCI")]
public static class NugetAliases
{
    [CakeMethodAlias]
    [CakeAliasCategory("NugetPack")]
    public static void CiNugetPack(this ICakeContext context, BuildManifest manifest, string packagesFolder, string versionNumber)
    {
        foreach (var package in manifest.NugetPackages)
        {
            context.Log.Information($"Packing {package}...");

            var args = new ProcessArgumentBuilder()
                .Append("pack")
                .Append(package)
                .Append("-c")
                .Append("Release")
                .Append("-o")
                .Append(packagesFolder)
                .Append("--version")
                .Append(versionNumber);

            ProcessHelper.Run(context, "dotnet", args, $"Pack failed: {package}");
        }
    }

    [CakeMethodAlias]
    [CakeAliasCategory("NugetPush")]
    public static void CiNugetPush(this ICakeContext context, NugetArgs args, string packagesFolder)
    {
        if (!System.IO.Directory.Exists(packagesFolder))
        {
            context.Log.Information("No packages to push in the packages folder");
            return;
        }

        foreach (var package in System.IO.Directory.EnumerateFiles(packagesFolder, "*.nupkg"))
        {
            var pushArgs = new ProcessArgumentBuilder()
                .Append("nuget")
                .Append("push")
                .Append(package)
                .Append("--api-key")
                .Append(args.ApiKey)
                .Append("--source")
                .Append(args.Source);

            ProcessHelper.Run(context, "dotnet", pushArgs, $"Push failed: {package}");
        }
    }
}
