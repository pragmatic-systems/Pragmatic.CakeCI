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
    public static string CiArgument(this ICakeContext context, string argumentName)
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
}
