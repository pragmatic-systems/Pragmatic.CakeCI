using Cake.Core;

namespace Pragmatic.CakeCI;

/// <summary>
/// Holds SonarQube/SonarCloud arguments resolved from CI context.
/// </summary>
public class SonarArgs
{
    /// <summary>
    /// The Sonar organization identifier.
    /// </summary>
    public required string Org { get; set; }

    /// <summary>
    /// The Sonar authentication token.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// The Sonar project key.
    /// </summary>
    public required string ProjectKey { get; set; }

    /// <summary>
    /// The Sonar project display name.
    /// </summary>
    public required string ProjectName { get; set; }

    /// <summary>
    /// The branch to register as being applied
    /// </summary>
    public required string Branch { get; set; }

    /// <summary>
    /// The Sonar host URL. Defaults to <c>http://localhost:9000</c>.
    /// </summary>
    public required string HostUrl { get; set; }

    /// <summary>
    /// Validates that all required Sonar arguments are present.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when a required argument is missing.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Org))
            throw new ArgumentException("SonarOrg is required.");

        if (string.IsNullOrEmpty(Token))
            throw new ArgumentException("SonarToken is required.");

        if (string.IsNullOrEmpty(Branch))
            throw new ArgumentException("SonarBranch is required.");

        if (string.IsNullOrEmpty(ProjectKey))
            throw new ArgumentException("SonarProjectKey is required.");

        if (string.IsNullOrEmpty(ProjectName))
            throw new ArgumentException("SonarProjectName is required.");
    }
}
