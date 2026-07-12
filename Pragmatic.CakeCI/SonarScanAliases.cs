using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Pragmatic.CakeCI;

/// <summary>
/// Aliases for running SonarQube/SonarCloud code analysis scans.
/// </summary>
[CakeAliasCategory("PragmaticCI")]
public static class SonarScanAliases
{
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
    public static void CiSonarScannerBegin(this ICakeContext context, SonarArgs sonarArgs, string artifactsFolder)
    {
        // Discover coverage report paths from test projects
        var testProjects = context.Globber
            .Match("**/*.Tests.csproj")
            .Select(p => p.FullPath)
            .ToArray();

        var reports = testProjects
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s) + ".coverage.xml")
            .Select(s => System.IO.Path.Combine(artifactsFolder, s));

        var reportPaths = string.Join(",", reports);
        context.Log.Information($"Sonar coverage report paths: {reportPaths}");

        // Run dotnet-sonarscanner begin
        var beginArgs = new ProcessArgumentBuilder()
            .Append("dotnet-sonarscanner")
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
    [CakeMethodAlias]
    [CakeAliasCategory("Sonar")]
    public static void CiSonarScannerEnd(this ICakeContext context, SonarArgs sonarArgs)
    {
        var endArgs = new ProcessArgumentBuilder()
            .Append("dotnet-sonarscanner")
            .Append("end")
            .Append($"/d:sonar.token={sonarArgs.Token}");

        ProcessHelper.Run(context, "dotnet", endArgs, "Sonar analysis failed.");
        context.Log.Information("Sonar analysis completed successfully.");
    }
}
