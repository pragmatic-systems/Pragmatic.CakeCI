using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;

namespace Pragmatic.CakeCI;

/// <summary>
/// Internal helper for executing external processes with consistent logging and error handling.
/// </summary>
internal static class ProcessHelper
{
    /// <summary>
    /// Runs a process, streaming stdout/stderr to the log line-by-line as it is produced,
    /// and throws on non-zero exit codes.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="exe">The executable to run.</param>
    /// <param name="arguments">The arguments to pass.</param>
    /// <param name="errorMessage">Message to include in the exception on failure.</param>
    /// <exception cref=CakeException>Thrown when the process exits with a non-zero code.</exception>
    public static void Run(ICakeContext context, string exe, ProcessArgumentBuilder arguments, string errorMessage)
    {
        var settings = CreateProcessSettings(context, exe, arguments);

        using var result = context.ProcessRunner.Start(exe, settings);
        result.WaitForExit();

        var exitCode = result.GetExitCode();
        if (exitCode != 0)
        {
            throw new CakeException($"{errorMessage} (exit code {exitCode})");
        }
    }

    /// <summary>
    /// Runs a process, streaming stdout/stderr to the log line-by-line as it is produced,
    /// and throws on non-zero exit codes.
    /// </summary>
    /// <param name="context">The Cake context.</param>
    /// <param name="exe">The executable to run.</param>
    /// <param name="arguments">The arguments to pass.</param>
    /// <param name="errorMessage">Message to include in the exception on failure.</param>
    /// <returns>Captured stdout.</returns>
    /// <exception cref=CakeException>Thrown when the process exits with a non-zero code.</exception>
    public static string RunCapture(ICakeContext context, string exe, ProcessArgumentBuilder arguments, string errorMessage)
    {
        var settings = CreateProcessSettings(context, exe, arguments);

        using var result = context.ProcessRunner.Start(exe, settings);
        result.WaitForExit();

        var exitCode = result.GetExitCode();
        if (exitCode != 0)
        {
            throw new CakeException($"{errorMessage} (exit code {exitCode})");
        }

        return string.Join("\n", result.GetStandardOutput());
    }

    private static ProcessSettings CreateProcessSettings(ICakeContext context, string exe, ProcessArgumentBuilder arguments) => new ProcessSettings
    {
        RedirectStandardError = true,
        RedirectStandardOutput = true,
        Arguments = arguments,
        RedirectedStandardOutputHandler = line =>
        {
            context.Log.Information($"[{exe}] {line}");
            return line;
        },
        RedirectedStandardErrorHandler = line =>
        {
            context.Log.Warning($"[{exe}] {line}");
            return line;
        },
    };
}
