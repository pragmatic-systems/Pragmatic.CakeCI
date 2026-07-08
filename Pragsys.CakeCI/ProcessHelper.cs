using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Pragsys.CakeCI;

/// <summary>
/// Internal helper for executing external processes with consistent logging and error handling.
/// </summary>
internal static class ProcessHelper
{
    private const int MaxLogLines = 256;

    /// <summary>
    /// Runs a process, logs stdout/stderr, and throws on non-zero exit codes.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="exe">The executable to run.</param>
    /// <param name="arguments">The arguments to pass.</param>
    /// <param name="errorMessage">Message to include in the exception on failure.</param>
    /// <returns>Captured stdout.</returns>
    /// <exception cref=CakeException">Thrown when the process exits with a non-zero code.</exception>
    public static string? Run(ICakeContext context, string exe, ProcessArgumentBuilder arguments, string errorMessage)
    {
        var settings = new ProcessSettings
        {
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            Arguments = arguments,
        };

        using var result = context.ProcessRunner.Start(exe, settings);
        result.WaitForExit();

        var exitCode = result.GetExitCode();

        var stdout = string.Join("\n", result.GetStandardOutput().Take(MaxLogLines));
        var stderr = string.Join("\n", result.GetStandardError().Take(MaxLogLines));

        // Log stdout if captured
        if (!string.IsNullOrEmpty(stdout))
        {
            context.Log.Information($"[{exe}] stdout:\n{stdout}");
        }

        // Log stderr always (when available)
        if (!string.IsNullOrEmpty(stderr))
        {
            if (exitCode != 0)
                context.Log.Error($"[{exe}] stderr:\n{stderr}");
            else
                context.Log.Warning($"[{exe}] stderr:\n{stderr}");
        }

        if (exitCode != 0)
        {
            throw new CakeException($"{errorMessage} (exit code {exitCode})");
        }

        return stdout;
    }
}
