using Cake.Core.Diagnostics;
using Cake.Core.IO;
using NSubstitute;

namespace Pragmatic.CakeCI.Tests;

public static class TestExtensions
{
    public static void LogHasMessage(this ICakeLog logSubstitute, string message)
    {
        logSubstitute.Received(1)
            .Write(Arg.Any<Verbosity>(), LogLevel.Information, Arg.Any<string>(),
                Arg.Is<object[]>(o => o.Length == 1 && ((string)o[0]).Contains(message)));
    }

    public static void ExecutedOnce(this IProcessRunner processRunner)
    {
        processRunner.Received(1).Start(Arg.Any<FilePath>(), Arg.Any<ProcessSettings>());
    }

    public static void ExecutedWithArgs(this IProcessRunner processRunner, params string[] argSubstrings)
    {
        processRunner.Received(1)
            .Start(Arg.Any<FilePath>(),
                Arg.Is<ProcessSettings>(s => s.Contains(argSubstrings)));
    }

    private static bool Contains(this ProcessSettings processSetting, string[] argSubstrings)
    {
        var args = string.Join(" ", processSetting.Arguments);
        return argSubstrings.All(sub => args.Contains(sub));
    }
}
