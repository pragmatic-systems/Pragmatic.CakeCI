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
    /// Resolves the path to the SonarScanner.MSBuild.dll from the dotnet-sonarscanner package
    /// installed via <c>#tool</c> in the tools directory.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="toolsDir">Optional tools directory. Defaults to <c>./tools</c> relative to the working directory.</param>
    /// <returns>The full path to SonarScanner.MSBuild.dll.</returns>
    private static string ResolveSonarScannerPath(ICakeContext context, DirectoryPath? toolsDir = null)
    {
        if (toolsDir == null)
        {
            var workingDir = context.Environment.WorkingDirectory;
            toolsDir = workingDir.Combine("tools");
        }

        var packageDir = System.IO.Directory.GetDirectories(toolsDir.FullPath, "dotnet-sonarscanner.*")
            .OrderByDescending(d => d)
            .FirstOrDefault();

        if (packageDir == null)
        {
            throw new CakeException($"dotnet-sonarscanner package directory not found in {toolsDir.FullPath}.");
        }

        var filePath = System.IO.Directory
            .GetFiles(packageDir, "SonarScanner.MSBuild.dll", SearchOption.AllDirectories)
            .SingleOrDefault();

        if (filePath == null)
        {
            throw new CakeException($"SonarScanner.MSBuild.dll not found in {packageDir}.");
        }

        context.Log.Information($"Using Sonar scanner from {filePath}");
        return filePath;
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

        // Resolve scanner path
        var scannerDll = ResolveSonarScannerPath(context, toolsDir);
        context.Log.Information($"Using Sonar scanner DLL: {scannerDll}");

        // Run dotnet-sonarscanner begin
        var beginArgs = new ProcessArgumentBuilder()
            .Append(scannerDll)
            .Append("begin")
            .Append($"/key:{sonarArgs.ProjectKey}")
            .Append($"/name:{sonarArgs.ProjectName}")
            .Append($"/organization:{sonarArgs.Org}")
            .Append($"/d:sonar.token={sonarArgs.Token}")
            .Append($"/d:sonar.branch.name={sonarArgs.Branch}")
            .Append($"/d:sonar.host.url={sonarArgs.HostUrl}")
            .Append($"/d:sonar.cs.vscoveragexml.reportsPaths={reportPaths}")
            .Append("/d:sonar.qualitygate.wait=true")
            .Append("/d:sonar.verbose=true");

        ProcessHelper.Run(context, "dotnet", beginArgs, "Sonar scanner begin failed.");
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
        var scannerDll = ResolveSonarScannerPath(context, toolsDir);

        var endArgs = new ProcessArgumentBuilder()
            .Append(scannerDll)
            .Append("end")
            .Append($"/d:sonar.token={sonarArgs.Token}");

        ProcessHelper.Run(context, "dotnet", endArgs, "Sonar analysis failed.");
        context.Log.Information("Sonar analysis completed successfully.");
    }
}
