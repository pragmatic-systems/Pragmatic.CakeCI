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
}
