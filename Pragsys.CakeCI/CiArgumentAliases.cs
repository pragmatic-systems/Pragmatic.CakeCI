using Cake.Core;
using Cake.Core.Annotations;

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
