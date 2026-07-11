using Cake.Core;

namespace Pragmatic.CakeCI;

/// <summary>
/// Holds container registry arguments resolved from CI context.
/// </summary>
public class ContainerArgs
{
    /// <summary>
    /// The container registry URL (e.g. ghcr.io/myorg).
    /// </summary>
    public required string Registry { get; set; }

    /// <summary>
    /// The authentication token for the container registry.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// The username for the container registry.
    /// </summary>
    public required string UserName { get; set; }

    /// <summary>
    /// Validates that all required container arguments are present.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown when a required argument is missing.</exception>
    public void Validate()
    {
        if (string.IsNullOrEmpty(Token))
            throw new ArgumentException("ContainerRegistryToken is required.");

        if (string.IsNullOrEmpty(UserName))
            throw new ArgumentException("ContainerRegistryUserName is required.");

        if (string.IsNullOrEmpty(Registry))
            throw new ArgumentException("ContainerRegistry is required.");
    }
}
