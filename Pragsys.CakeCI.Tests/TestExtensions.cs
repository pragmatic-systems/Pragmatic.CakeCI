using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Moq;

namespace Pragsys.CakeCI.Tests;

public static class TestExtensions
{
    public static void LogHasMessage(this Mock<ICakeLog> logMock, string message)
    {
        logMock.Verify(l => l.Write(It.IsAny<Verbosity>(), LogLevel.Information, It.IsAny<string>(), It.Is<object[]>(o => o.Length == 1 && ((string)o[0]).Contains(message))), Times.Once);
    }

    public static void ExecutedOnce(this Mock<IProcessRunner> processRunner)
    {
        processRunner.Verify(pr => pr.Start(It.IsAny<FilePath>(), It.IsAny<ProcessSettings>()), Times.Once);
    }
}
