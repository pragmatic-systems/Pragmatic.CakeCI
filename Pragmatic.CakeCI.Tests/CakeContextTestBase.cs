using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Moq;

namespace Pragmatic.CakeCI.Tests;

/// <summary>
/// Base class providing a pre-configured mock <see cref="ICakeContext"/> and related mocks
/// for tests that exercise Cake aliases involving process execution.
/// </summary>
public abstract class CakeContextTestBase
{
    protected readonly Mock<ICakeContext> Context;
    protected readonly Mock<ICakeLog> Log;
    protected readonly Mock<ICakeEnvironment> Environment;
    protected readonly Mock<IProcessRunner> ProcessRunner;
    protected readonly Mock<IProcess> Process;
    protected readonly Mock<IGlobber> Globber;

    protected CakeContextTestBase()
    {
        Context = new Mock<ICakeContext>();
        Log = new Mock<ICakeLog>();
        Environment = new Mock<ICakeEnvironment>();
        ProcessRunner = new Mock<IProcessRunner>();
        Process = new Mock<IProcess>();
        Globber = new Mock<IGlobber>();

        ProcessRunner
            .Setup(pr => pr.Start(It.IsAny<FilePath>(), It.IsAny<ProcessSettings>()))
            .Returns(Process.Object);

        Environment.Setup(e => e.WorkingDirectory).Returns(new DirectoryPath("."));

        Context.Setup(c => c.Log).Returns(Log.Object);
        Context.Setup(c => c.ProcessRunner).Returns(ProcessRunner.Object);
        Context.Setup(c => c.Environment).Returns(Environment.Object);
        Context.Setup(c => c.Globber).Returns(Globber.Object);
    }
}
