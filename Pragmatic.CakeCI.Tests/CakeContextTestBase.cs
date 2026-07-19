using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using NSubstitute;

namespace Pragmatic.CakeCI.Tests;

/// <summary>
/// Base class providing a pre-configured substitute <see cref="ICakeContext"/> and related substitutes
/// for tests that exercise Cake aliases involving process execution.
/// </summary>
public abstract class CakeContextTestBase
{
    protected readonly ICakeContext Context;
    protected readonly ICakeLog Log;
    protected readonly ICakeEnvironment Environment;
    protected readonly IProcessRunner ProcessRunner;
    protected readonly IProcess Process;
    protected readonly IGlobber Globber;

    protected CakeContextTestBase()
    {
        Context = Substitute.For<ICakeContext>();
        Log = Substitute.For<ICakeLog>();
        Environment = Substitute.For<ICakeEnvironment>();
        ProcessRunner = Substitute.For<IProcessRunner>();
        Process = Substitute.For<IProcess>();
        Globber = Substitute.For<IGlobber>();

        ProcessRunner.Start(Arg.Any<FilePath>(), Arg.Any<ProcessSettings>()).Returns(Process);

        Environment.WorkingDirectory.Returns(new DirectoryPath("."));

        Context.Log.Returns(Log);
        Context.ProcessRunner.Returns(ProcessRunner);
        Context.Environment.Returns(Environment);
        Context.Globber.Returns(Globber);
    }
}
