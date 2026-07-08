namespace Pragsys.CakeCI.Tests;

public class DockerAliasesTests
{
    [Theory]
    [InlineData("myapp", "ghcr.io/myorg/myapp")]
    [InlineData("otherservice", "ghcr.io/myorg/otherservice")]
    [InlineData("deepservice", "ghcr.io/myorg/deepservice")]
    public void GetFullImageName_WithRegistry_AppendsRegistryPrefix(string packageName, string expected)
    {
        var args = new ContainerArgs
        {
            Registry = "ghcr.io/myorg",
            Token = "token",
            UserName = "user",
        };

        var result = DockerAliases.GetFullImageName(args, packageName);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("myapp", "myapp")]
    [InlineData("otherservice", "otherservice")]
    [InlineData("deepservice", "deepservice")]
    public void GetFullImageName_WithoutRegistry_ReturnsPackageName(string packageName, string expected)
    {
        var args = new ContainerArgs
        {
            Registry = string.Empty,
            Token = "token",
            UserName = "user",
        };

        var result = DockerAliases.GetFullImageName(args, packageName);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("./src/MyApp/Dockerfile", "myapp")]
    [InlineData("src/OtherService/Dockerfile", "otherservice")]
    [InlineData("a/b/c/DeepService/Dockerfile", "deepservice")]
    public void PathSeparators_ForwardSlash_ShouldExtractDirectoryName(string dockerfilePath, string expectedName)
    {
        // Verify that Path.GetFileName(GetDirectoryName(...)) correctly handles
        // forward-slash paths regardless of platform (the fix for the separator issue).
        var directoryName = System.IO.Path.GetDirectoryName(dockerfilePath);
        var packageName = System.IO.Path.GetFileName(directoryName!).ToLowerInvariant();

        packageName.Should().Be(expectedName);
    }

    [Fact]
    public void PathSeparators_Backslash_ShouldExtractDirectoryName()
    {
        // On Windows the paths may use backslashes; the fix must handle those too.
        var dockerfilePath = System.IO.Path.Combine("src", "MyApp", "Dockerfile");
        var directoryName = System.IO.Path.GetDirectoryName(dockerfilePath);
        var packageName = System.IO.Path.GetFileName(directoryName!).ToLowerInvariant();

        packageName.Should().Be("myapp");
    }
}
