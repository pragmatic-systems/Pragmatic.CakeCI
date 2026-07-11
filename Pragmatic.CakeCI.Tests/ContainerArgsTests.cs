using Pragmatic.CakeCI;
using System;

namespace Pragmatic.CakeCI.Tests;

public class ContainerArgsTests
{
    [Fact]
    public void ContainerArgs_ShouldValidate()
    {
        var args = new ContainerArgs
        {
            Registry = string.Empty,
            Token = string.Empty,
            UserName = string.Empty,
        };

        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("ContainerRegistryToken is required.");

        args.Token = "my-token";
        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("ContainerRegistryUserName is required.");

        args.UserName = "my-user";
        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("ContainerRegistry is required.");

        args.Registry = "ghcr.io/myorg";
        args.Validate();
    }
}
