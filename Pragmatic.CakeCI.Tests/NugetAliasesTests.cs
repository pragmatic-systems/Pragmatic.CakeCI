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