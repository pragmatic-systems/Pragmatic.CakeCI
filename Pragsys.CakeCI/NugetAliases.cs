using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Pragsys.CakeCI;

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

            //  dotnet pack Pragsys.CakeCI/Pragsys.CakeCI.csproj -c Release -o local-packages --version 0.1.0-dogfood
            var settings = new ProcessSettings();
            settings.WithArguments(a =>
            {
                a.Append("pack");
                a.Append(package);
                a.Append("-c");
                a.Append("Release");
                a.Append("-o");
                a.Append(packagesFolder);
                a.Append("--version");
                a.Append(versionNumber);
            });

            using var result = context.ProcessRunner.Start("dotnet", settings);

            result.WaitForExit();
            if (result.GetExitCode() != 0)
            {
                throw new CakeException($"Pack failed: {package}");
            }
        }
    }

    [CakeMethodAlias]
    [CakeAliasCategory("NugetPush")]
    public static void CiNugetPush(this ICakeContext context, NugetArgs manifest, string packagesFolder)
    {
        if (!System.IO.Directory.Exists(packagesFolder))
        {
            context.Log.Information("No packages to push in the packages folder");
            return;
        }

        // dotnet nuget push <package-file> --api-key <API-key> --source <source-url>
        var packedArtifacts = System.IO.Directory.EnumerateFiles(packagesFolder);
        foreach (var package in packedArtifacts)
        {
            var settings = new ProcessSettings();
            settings.WithArguments(a =>
            {
                a.Append("nuget");
                a.Append("push");
                a.Append(package);
                a.Append("--api-key");
                a.Append(manifest.ApiKey);
                a.Append("--source");
                a.Append(manifest.Source);
            });

            using var result = context.ProcessRunner.Start("dotnet", settings);

            result.WaitForExit();
            if (result.GetExitCode() != 0)
            {
                throw new CakeException($"Push failed: {package}");
            }
        }
    }
}