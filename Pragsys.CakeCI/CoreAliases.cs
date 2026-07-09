using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Pragsys.CakeCI;

/// <summary>
/// Aliases for core CI operations: test, lint, version resolution, and benchmarking.
/// </summary>
[CakeAliasCategory("PragsysCI")]
public static class CoreAliases
{
    [CakeMethodAlias]
    [CakeAliasCategory("Test")]
    public static void CiTest(this ICakeContext context)
    {
        var scriptDirectory = context.Environment.WorkingDirectory;
        var artifactsPath = System.IO.Path.Combine(scriptDirectory.FullPath, "artifacts");

        var testProjects = context.Globber
            .Match("**/*.Tests.csproj")
            .Select(p => p.FullPath)
            .ToArray();

        foreach (var testProject in testProjects)
        {
            var projectName = System.IO.Path.GetFileNameWithoutExtension(testProject.ToString());
            context.Log.Information($"Testing - {projectName}");

            var args = new ProcessArgumentBuilder()
                .Append("test")
                .Append(testProject)
                .Append("--")
                .AppendQuoted($"--results-directory {artifactsPath} --report-ctrf --coverage --coverage-output '{projectName}.coverage.xml' --coverage-output-format xml");

            ProcessHelper.Run(context, "dotnet", args, $"Tests failed for {projectName}");
            context.Log.Information($"{projectName} tests passed");
        }
    }

    [CakeMethodAlias]
    [CakeAliasCategory("Lint")]
    public static void CiLint(this ICakeContext context)
    {
        context.Log.Information("Running lint check with dotnet format...");

        var args = new ProcessArgumentBuilder()
            .Append("format")
            .Append("--verify-no-changes");

        ProcessHelper.Run(context, "dotnet", args, "Lint check failed: code formatting violations detected. Run `dotnet format`");
        context.Log.Information("Lint check passed – no formatting changes required.");
    }

    [CakeMethodAlias]
    [CakeAliasCategory("Version")]
    public static string CiVersion(this ICakeContext context, string? versionOverride = null)
    {
        if (!string.IsNullOrEmpty(versionOverride))
        {
            context.Log.Information($"Version Number: {versionOverride}");
            return versionOverride;
        }

        context.Log.Information("Resolving version from GitVersion...");

        var args = new ProcessArgumentBuilder().Append("gitversion");
        var output = ProcessHelper.Run(context, "dotnet", args, "GitVersion failed")
            ?? throw new CakeException("GitVersion returned no output.");

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(output);
            var semVer = doc.RootElement.GetProperty("SemVer").GetString();

            var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
            var json = System.Text.Json.JsonSerializer.Serialize(doc.RootElement, options);

            context.Log.Information($"GitVersion Info: {json}");
            context.Log.Information($"Version Number: {semVer}");

            return semVer ?? throw new CakeException("GitVersion returned a null SemVer.");
        }
        catch (System.Text.Json.JsonException)
        {
            throw new CakeException($"Failed to parse GitVersion output: {output}");
        }
    }

    [CakeMethodAlias]
    [CakeAliasCategory("Benchmark")]
    public static void CiBenchmark(this ICakeContext context)
    {
        var scriptDirectory = context.Environment.WorkingDirectory;
        var artifactsFolder = System.IO.Path.Combine(scriptDirectory.FullPath, "artifacts");

        var benchmarkProjects = context.Globber
            .Match("**/*.Benchmark.csproj")
            .Select(p => p.FullPath)
            .ToArray();


        foreach (var benchmarkProject in benchmarkProjects)
        {
            var benchName = System.IO.Path.GetFileNameWithoutExtension(benchmarkProject);
            context.Log.Information($"Benchmarking {benchName}...");

            var args = new ProcessArgumentBuilder()
                .Append("run")
                .Append("--project")
                .Append(benchmarkProject)
                .Append("--configuration")
                .Append("Release")
                .Append("--artifacts")
                .AppendQuoted(System.IO.Path.Combine(artifactsFolder, benchName));

            ProcessHelper.Run(context, "dotnet", args, $"Benchmark failed: {benchName}");
        }
    }
}
