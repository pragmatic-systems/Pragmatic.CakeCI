using Pragmatic.CakeCI;
using System;

namespace Pragmatic.CakeCI.Tests;

public class NugetArgsTests
{
    [Fact]
    public void NugetArgs_ShouldValidate()
    {
        var args = new NugetArgs
        {
            Source = string.Empty,
            ApiKey = string.Empty,
        };

        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("NugetPackageSource (Source) is required.");

        args.Source = "https://api.nuget.org/v3/index.json";
        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("NugetApiKey (ApiKey) is required.");

        args.ApiKey = "my-api-key";
        args.Validate();
    }
}
