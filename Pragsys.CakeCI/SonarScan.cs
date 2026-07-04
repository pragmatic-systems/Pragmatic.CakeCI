using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Pragsys.CakeCI;

/// <summary>
/// Aliases for running SonarQube/SonarCloud code analysis scans.
/// </summary>
[CakeAliasCategory("PragsysCI")]
public static class SonarScanAliases
{
    /// <summary>
    /// Resolves the path to the dotnet-sonarscanner tool, installing it if necessary.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="toolsDir">Optional tools directory. Defaults to <c>./tools</c> relative to the working directory.</param>
    /// <returns>The full path to the sonarscanner executable.</returns>
    private static string ResolveSonarScannerPath(ICakeContext context, DirectoryPath? toolsDir = null)
    {
        if (toolsDir == null)
        {
            var workingDir = context.Environment.WorkingDirectory;
            toolsDir = workingDir.Combine("tools");
        }

        // On Windows the executable is dotnet-sonarscanner.exe
        var scannerExe = System.IO.Path.Combine(toolsDir.FullPath, "dotnet-sonarscanner.exe");
        if (System.IO.File.Exists(scannerExe))
        {
            return scannerExe;
        }

        // On Linux/macOS it's a shell script named dotnet-sonarscanner
        var scannerScript = System.IO.Path.Combine(toolsDir.FullPath, "dotnet-sonarscanner");
        if (System.IO.File.Exists(scannerScript))
        {
            return scannerScript;
        }

        // Not installed yet — install it
        context.Log.Information("Installing dotnet-sonarscanner tool...");
        var installSettings = new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                .Append("tool")
                .Append("install")
                .Append("dotnet-sonarscanner")
                .Append("--version")
                .Append("7.1.1")
                .Append("--tool-path")
                .Append(toolsDir.FullPath)
                .Append("--add-source")
                .Append("https://api.nuget.org/v3/index.json")
                .Append("-v")
                .Append("quiet")
        };

        using var installResult = context.ProcessRunner.Start("dotnet", installSettings);
        installResult.WaitForExit();

        if (installResult.GetExitCode() != 0)
        {
            throw new CakeException("Failed to install dotnet-sonarscanner.");
        }

        // Re-check after install
        scannerExe = System.IO.Path.Combine(toolsDir.FullPath, "dotnet-sonarscanner.exe");
        if (System.IO.File.Exists(scannerExe))
        {
            return scannerExe;
        }

        scannerScript = System.IO.Path.Combine(toolsDir.FullPath, "dotnet-sonarscanner");
        if (System.IO.File.Exists(scannerScript))
        {
            return scannerScript;
        }

        // If we still can't find it, list what's actually there for debugging
        var availableFiles = System.IO.Directory.GetFiles(toolsDir.FullPath)
            .Select(f => System.IO.Path.GetFileName(f))
            .ToArray();
        throw new CakeException(
            $"Sonar scanner executable not found in {toolsDir.FullPath}. Available files: {string.Join(", ", availableFiles)}");
    }

    /// <summary>
    /// Starts a SonarQube/SonarCloud analysis session by running <c>dotnet-sonarscanner begin</c>
    /// and building the solution.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="sonarArgs">The Sonar arguments configuration.</param>
    /// <param name="artifactsFolder">Path to the artifacts folder where coverage reports are stored.</param>
    /// <param name="toolsDir">Optional tools directory for the scanner. Defaults to <c>./tools</c>.</param>
    [CakeMethodAlias]
    [CakeAliasCategory("Sonar")]
    public static void CiSonarScannerBegin(this ICakeContext context, SonarArgs sonarArgs, string artifactsFolder, DirectoryPath? toolsDir = null)
    {
        var scriptDirectory = context.Environment.WorkingDirectory;

        // Discover coverage report paths from test projects
        var testProjects = System.IO.Directory.GetFiles(
            scriptDirectory.FullPath,
            "*Tests.csproj",
            System.IO.SearchOption.AllDirectories);

        var reports = testProjects
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s) + ".coverage.xml")
            .Select(s => System.IO.Path.Combine(artifactsFolder, s));

        var reportPaths = string.Join(",", reports);
        context.Log.Information($"Sonar coverage report paths: {reportPaths}");

        // Resolve scanner path (auto-installs if needed)
        var scannerPath = ResolveSonarScannerPath(context, toolsDir);
        context.Log.Information($"Using Sonar scanner: {scannerPath}");

        // Run dotnet-sonarscanner begin
        var beginSettings = new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                .Append("begin")
                .Append($"/key:{sonarArgs.ProjectKey}")
                .Append($"/name:{sonarArgs.ProjectName}")
                .Append($"/organization:{sonarArgs.Org}")
                .Append($"/d:sonar.token={sonarArgs.Token}")
                .Append($"/d:sonar.branch.name={sonarArgs.Branch}")
                .Append($"/d:sonar.host.url={sonarArgs.HostUrl}")
                .Append($"/d:sonar.cs.vscoveragexml.reportsPaths={reportPaths}")
                .Append("/d:sonar.qualitygate.wait=true")
                .Append("/d:sonar.verbose=true")
        };

        using var beginResult = context.ProcessRunner.Start(scannerPath, beginSettings);
        beginResult.WaitForExit();
        if (beginResult.GetExitCode() != 0)
        {
            throw new CakeException("Sonar scanner begin failed.");
        }

        // Build the solution by finding .sln or .slnx files
        var slnFiles = System.IO.Directory.GetFiles(scriptDirectory.FullPath, "*.sln", System.IO.SearchOption.AllDirectories)
            .Concat(System.IO.Directory.GetFiles(scriptDirectory.FullPath, "*.slnx", System.IO.SearchOption.AllDirectories))
            .ToArray();

        if (slnFiles.Length > 0)
        {
            var solution = slnFiles[0];
            context.Log.Information($"Building solution: {solution}");

            var buildSettings = new ProcessSettings
            {
                Arguments = new ProcessArgumentBuilder()
                    .Append("build")
                    .Append(solution)
            };

            using var buildResult = context.ProcessRunner.Start("dotnet", buildSettings);
            buildResult.WaitForExit();
            if (buildResult.GetExitCode() != 0)
            {
                throw new CakeException("Solution build failed.");
            }
        }
        else
        {
            context.Log.Warning("No solution file (.sln or .slnx) found to build.");
        }
    }

    /// <summary>
    /// Ends a SonarQube/SonarCloud analysis session by running <c>dotnet-sonarscanner end</c>.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="sonarArgs">The Sonar arguments configuration.</param>
    /// <param name="toolsDir">Optional tools directory for the scanner. Defaults to <c>./tools</c>.</param>
    [CakeMethodAlias]
    [CakeAliasCategory("Sonar")]
    public static void CiSonarScannerEnd(this ICakeContext context, SonarArgs sonarArgs, DirectoryPath? toolsDir = null)
    {
        var scannerPath = ResolveSonarScannerPath(context, toolsDir);

        var endSettings = new ProcessSettings
        {
            Arguments = new ProcessArgumentBuilder()
                .Append("end")
                .Append($"/d:sonar.token={sonarArgs.Token}")
        };

        using var endResult = context.ProcessRunner.Start(scannerPath, endSettings);
        endResult.WaitForExit();

        if (endResult.GetExitCode() == 0)
        {
            context.Log.Information("Sonar analysis completed successfully.");
        }
        else
        {
            throw new CakeException("Sonar analysis failed.");
        }
    }
}
