using Cake.Core;
using NSubstitute;

namespace Pragmatic.CakeCI.Tests;

public class CiArgumentAliasesTests
{
    private readonly ICakeContext _context;
    private readonly ICakeArguments _cakeArguments;
    private readonly ICakeEnvironment _cakeEnvironment;

    public CiArgumentAliasesTests()
    {
        _context = Substitute.For<ICakeContext>();
        _cakeArguments = Substitute.For<ICakeArguments>();
        _cakeEnvironment = Substitute.For<ICakeEnvironment>();

        _context.Arguments.Returns(_cakeArguments);
        _context.Environment.Returns(_cakeEnvironment);
    }

    [Fact]
    public void Should_ConvertSimpleArg()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments.GetArguments(key).Returns(new[] { value });

        var result = _context.CiArgument(key);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_ConvertSimpleEnv()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments.GetArguments(key).Returns(new string[0]);
        _cakeEnvironment.GetEnvironmentVariable(key).Returns(value);

        var result = _context.CiArgument(key);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_ConvertGithubActionEnv()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments.GetArguments(key).Returns(new string[0]);
        _cakeEnvironment.GetEnvironmentVariable($"INPUT_{key}".ToUpperInvariant()).Returns(value);

        var result = _context.CiArgument(key);
        result.ShouldBe(value);
    }

    [Fact]
    public void Should_ReturnDefaultWhenSupplied()
    {
        var key = "Key";
        var value = "Value";

        _cakeArguments.GetArguments(key).Returns(new string[0]);

        var result = _context.CiArgument(key, value);
        result.ShouldBe(value);
    }
}
