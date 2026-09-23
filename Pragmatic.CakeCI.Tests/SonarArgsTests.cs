using Pragmatic.CakeCI;

namespace Pragmatic.CakeCI.Tests;

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
            AdditionalProperties = null
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
        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("SonarAdditionalProperties cannot be null.");

        args.AdditionalProperties = new Dictionary<string, string>();
        args.Validate();
    }

    [Fact]
    public void SonarArgs_WithEmptyAdditionalPropertyKey_ShouldFail()
    {
        var args = new SonarArgs
        {
            Org = "Org",
            Token = "Token",
            ProjectKey = "ProjectKey",
            ProjectName = "ProjectName",
            Branch = "Branch",
            HostUrl = "HostUrl",
            AdditionalProperties = new Dictionary<string, string>
            {
                [""] = "value"
            },
        };

        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("Sonar additional property key is empty.");
    }

    [Fact]
    public void SonarArgs_WithPrefixedAdditionalPropertyKey_ShouldFail()
    {
        var args = new SonarArgs
        {
            Org = "Org",
            Token = "Token",
            ProjectKey = "ProjectKey",
            ProjectName = "ProjectName",
            Branch = "Branch",
            HostUrl = "HostUrl",
            AdditionalProperties = new Dictionary<string, string>
            {
                ["/d:sonar.exclusions"] = "**/Scripts/*.sql"
            },
        };

        Should
            .Throw<ArgumentException>(() => args.Validate())
            .Message.ShouldBe("Sonar additional property key '/d:sonar.exclusions' must not include the '/d:' prefix.");
    }
}
