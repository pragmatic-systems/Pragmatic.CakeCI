using System.Diagnostics;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Pragsys.CakeCI;

/// <summary>
/// Aliases for resolving CI arguments from command-line args with environment variable fallback.
/// </summary>
[CakeAliasCategory("PragsysCI")]
public static class CiArgumentAliases
{
    [CakeMethodAlias]
    [CakeAliasCategory("Arguments")]
    public static string? CiArgument(this ICakeContext context, string argumentName)
    {
        // Normal CMD Argument First
        var result = context.Arguments.GetArgument(argumentName);
        if (!string.IsNullOrEmpty(result))
            return result;

        // Then Check Environment Variable
        result = context.Environment.GetEnvironmentVariable(argumentName);
        if (!string.IsNullOrEmpty(result))
            return result;

        // Then Check Capitalized Github environment variable. INPUT_{MYVAR}
        var formattedArg = $"INPUT_{argumentName}".ToUpperInvariant();
        result = context.Environment.GetEnvironmentVariable(formattedArg);
        if (!string.IsNullOrEmpty(result))
            return result;

        return result;
    }

    [CakeMethodAlias]
    [CakeAliasCategory("Arguments")]
    public static string CiArgument(this ICakeContext context, string argumentName, string defaultValue)
    {
        string? result = null;

        // Normal CMD Argument First
        result = context.Arguments.GetArgument(argumentName);
        if (!string.IsNullOrEmpty(result))
            return result;

        // Then Check Environment Variable
        result = context.Environment.GetEnvironmentVariable(argumentName);
        if (!string.IsNullOrEmpty(result))
            return result;

        // Then Check Capitalized Github environment variable. INPUT_{MYVAR}
        var formattedArg = $"INPUT_{argumentName}".ToUpperInvariant();
        result = context.Environment.GetEnvironmentVariable(formattedArg);
        if (!string.IsNullOrEmpty(result))
            return result;

        return defaultValue;
    }

    [CakeMethodAlias]
    [CakeAliasCategory("Test")]
    public static void CiTest(this ICakeContext context)
    {
        // Assumes the .cake script resides at the repository root.
        var scriptDirectory = context.Environment.WorkingDirectory;

        var testProjects = System.IO.Directory.GetFiles(
                    "./",
                    "*.Tests.csproj",
                    System.IO.SearchOption.AllDirectories);

        foreach (var testProject in testProjects)
        {
            var projectName = System.IO.Path.GetFileNameWithoutExtension(testProject.ToString());

            // NOTE: New dotnet test model moves the relative path to inside the local app.
            context.Log.Information($"Testing - {projectName}");

            var settings = new ProcessSettings();
            settings.WithArguments(a =>
            {
                a.Append("test");
                a.Append(testProject);
                a.Append("--");
                a.AppendQuoted($"--results-directory {scriptDirectory}\\artifacts --report-ctrf --coverage --coverage-output '{projectName}.coverage.xml' --coverage-output-format xml");
            });

            using var result = context.ProcessRunner.Start("dotnet", settings);
            result.WaitForExit();
            if (result.GetExitCode() != 0)
            {
                throw new CakeException("Tests failed");
            }

            context.Log.Information("Tests pass");
        }
    }

    [CakeMethodAlias]
    [CakeAliasCategory("Lint")]
    public static void CiLint(this ICakeContext context)
    {
        context.Log.Information("Running lint check with dotnet format...");

        // Run `dotnet format --verify-no-changes`
        var settings = new ProcessSettings();
        settings.WithArguments(a =>
        {
            a.Append("format");
            a.Append("--verify-no-changes");
        });

        using var result = context.ProcessRunner.Start("dotnet", settings);
        result.WaitForExit();
        if (result.GetExitCode() != 0)
        {
            throw new CakeException("Lint check failed: code formatting violations detected. Run `dotnet format`");
        }
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

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "gitversion",
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            throw new CakeException("Failed to start GitVersion process.");
        }

        var output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new CakeException($"GitVersion exited with code {process.ExitCode}: {output}");
        }

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

        var benchmarkProjects = System.IO.Directory.GetFiles(
                    "./",
                    "*.Benchmark.csproj",
                    System.IO.SearchOption.AllDirectories);

        foreach (var benchmarkProject in benchmarkProjects)
        {
            var benchName = System.IO.Path.GetFileNameWithoutExtension(benchmarkProject);
            context.Log.Information($"Benchmarking {benchName}...");

            var settings = new ProcessSettings();
            settings.WithArguments(a =>
            {
                a.Append("run");
                a.Append("--project");
                a.Append(benchmarkProject);
                a.Append("--configuration");
                a.Append("Release");
                a.Append("--artifacts");
                a.AppendQuoted(System.IO.Path.Combine(artifactsFolder, benchName));
            });

            using var result = context.ProcessRunner.Start("dotnet", settings);
            result.WaitForExit();
            if (result.GetExitCode() != 0)
            {
                throw new CakeException($"Benchmark failed: {benchName}");
            }
        }
    }
}
