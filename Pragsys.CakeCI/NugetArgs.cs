using Cake.Core;

namespace Pragmatic.CakeCI;

/// <summary>
/// Holds NuGet publishing arguments resolved from CI context.
/// </summary>
public class NugetArgs
{
    /// <summary>
    /// The NuGet package source URL to push packages to.
    /// </summary>
    public required string Source { get; set; }

    /// <summary>
    /// The API key for authenticating with the NuGet source.
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// Validates that all required NuGet arguments are present.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when a required argument is missing.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Source))
            throw new ArgumentException("NugetPackageSource (Source) is required.");

        if (string.IsNullOrEmpty(ApiKey))
            throw new ArgumentException("NugetApiKey (ApiKey) is required.");
    }
}
