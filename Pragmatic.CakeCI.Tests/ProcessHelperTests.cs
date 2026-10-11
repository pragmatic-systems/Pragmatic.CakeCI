using Cake.Core.Diagnostics;
using Cake.Core.IO;
using NSubstitute;

namespace Pragmatic.CakeCI.Tests;

/// <summary>
/// Covers the redirection handlers built by <see cref="ProcessHelper"/>'s private <c>CreateProcessSettings</c>
/// by capturing the <see cref="ProcessSettings"/> handed to the runner and invoking the handlers
/// the same way Cake does, per output line.
/// </summary>
public class ProcessHelperTests : CakeContextTestBase
{
    [Fact]
    public void RunCapture_RedirectHandlers_LogLinesAndKeepStdout()
    {
        Process.GetStandardOutput().Returns(["hello"]);
        Process.GetExitCode().Returns(0);

        ProcessSettings? settings = null;
        ProcessRunner
            .When(r => r.Start(Arg.Any<FilePath>(), Arg.Any<ProcessSettings>()))
            .Do(info => settings = info.Arg<ProcessSettings>());

        ProcessHelper.RunCapture(Context, "mytool", new ProcessArgumentBuilder(), "mytool failed");

        settings.ShouldNotBeNull();

        AssertHandlers(settings, captureStdout: true);
    }

    private void AssertHandlers(ProcessSettings settings, bool captureStdout)
    {
        var outputHandler = settings.RedirectedStandardOutputHandler!;
        outputHandler.ShouldNotBeNull();
        outputHandler("hello").ShouldBe(captureStdout ? "hello" : null);
        Log.Received(1).Write(Arg.Any<Verbosity>(), LogLevel.Information, Arg.Any<string>(),
            Arg.Is<object[]>(o => (string)o[0] == "[mytool] hello"));

        var errorHandler = settings.RedirectedStandardErrorHandler!;
        errorHandler.ShouldNotBeNull();
        errorHandler("boom").ShouldBeNull();
        Log.Received(1).Write(Arg.Any<Verbosity>(), LogLevel.Warning, Arg.Any<string>(),
            Arg.Is<object[]>(o => (string)o[0] == "[mytool] boom"));
    }
}
