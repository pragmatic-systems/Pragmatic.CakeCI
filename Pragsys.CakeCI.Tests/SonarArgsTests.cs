using System;
using System.Collections.Generic;
using System.Text;
using Xunit.Sdk;

namespace Pragsys.CakeCI.Tests;

public class SonarArgsTests
{
    [Fact]
    public void SonarArgs_ShouldValidate()
    {
        var args = new SonarArgs
        {
            Org = string.Empty,
            Token = string.Empty,
            ProjectKey = string.Empty,
            ProjectName = string.Empty,
            Branch = string.Empty,
            HostUrl = string.Empty,
        };

        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("SonarOrg is required.");

        args.Org = "Org";
        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("SonarToken is required.");

        args.Token = "Token";
        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("SonarBranch is required.");

        args.Branch = "Branch";
        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("SonarProjectKey is required.");

        args.ProjectKey = "ProjectKey";
        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("SonarProjectName is required.");

        args.ProjectName = "ProjectName";
        args.Validate();
    }
}
