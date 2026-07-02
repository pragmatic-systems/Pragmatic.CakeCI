using Cake.Core;
using Moq;

namespace Pragsys.CakeCI.Tests;

public class CiArgumentAliasesTests
{
    private readonly Mock<ICakeContext> _context;
    private readonly Mock<ICakeArguments> _arguments;
    private readonly Mock<ICakeEnvironment> _environment;

    public CiArgumentAliasesTests()
    {
        _context = new Mock<ICakeContext>();
        _arguments = new Mock<ICakeArguments>();
        _environment = new Mock<ICakeEnvironment>();

        _context.Setup(c => c.Arguments).Returns(_arguments.Object);
        _context.Setup(c => c.Environment).Returns(_environment.Object);
    }

    [Fact]
    public void CiArgument_WithArgument_ReturnsArgumentValue()
    {
        _arguments.Setup(a => a.GetArgument("MyKey")).Returns("arg-value");

        var result = _context.Object.CiArgument("MyKey");

        result.Should().Be("arg-value");
    }

    [Fact]
    public void CiArgument_WithoutArgument_WithEnvVar_ReturnsEnvVarValue()
    {
        _arguments.Setup(a => a.GetArgument("MyKey")).Returns((string?)null);
        _environment.Setup(e => e.GetEnvironmentVariable("MyKey")).Returns("env-value");

        var result = _context.Object.CiArgument("MyKey");

        result.Should().Be("env-value");
    }

    [Fact]
    public void CiArgument_WithoutArgumentOrEnvVar_WithInputEnvVar_ReturnsInputEnvVarValue()
    {
        _arguments.Setup(a => a.GetArgument("MyKey")).Returns((string?)null);
        _environment.Setup(e => e.GetEnvironmentVariable("MyKey")).Returns((string?)null);
        _environment.Setup(e => e.GetEnvironmentVariable("INPUT_MYKEY")).Returns("input-value");

        var result = _context.Object.CiArgument("MyKey");

        result.Should().Be("input-value");
    }

    [Fact]
    public void CiArgument_WithNothing_ReturnsNull()
    {
        _arguments.Setup(a => a.GetArgument("MyKey")).Returns((string?)null);
        _environment.Setup(e => e.GetEnvironmentVariable("MyKey")).Returns((string?)null);
        _environment.Setup(e => e.GetEnvironmentVariable("INPUT_MYKEY")).Returns((string?)null);

        var result = _context.Object.CiArgument("MyKey");

        result.Should().BeNull();
    }

    [Fact]
    public void CiArgument_WithDefaultValue_WhenNothing_ReturnsDefaultValue()
    {
        _arguments.Setup(a => a.GetArgument("MyKey")).Returns((string?)null);
        _environment.Setup(e => e.GetEnvironmentVariable("MyKey")).Returns((string?)null);
        _environment.Setup(e => e.GetEnvironmentVariable("INPUT_MYKEY")).Returns((string?)null);

        var result = _context.Object.CiArgument("MyKey", "default-value");

        result.Should().Be("default-value");
    }

    [Fact]
    public void CiArgument_WithDefaultValue_WhenArgumentExists_ReturnsArgumentValue()
    {
        _arguments.Setup(a => a.GetArgument("MyKey")).Returns("arg-value");

        var result = _context.Object.CiArgument("MyKey", "default-value");

        result.Should().Be("arg-value");
    }

    [Fact]
    public void CiArgument_WithDefaultValue_WhenEnvVarExists_ReturnsEnvVarValue()
    {
        _arguments.Setup(a => a.GetArgument("MyKey")).Returns((string?)null);
        _environment.Setup(e => e.GetEnvironmentVariable("MyKey")).Returns("env-value");

        var result = _context.Object.CiArgument("MyKey", "default-value");

        result.Should().Be("env-value");
    }

    [Fact]
    public void CiArgument_Priority_Is_Argument_Over_EnvVar_Over_InputEnvVar_Over_Default()
    {
        _arguments.Setup(a => a.GetArgument("MyKey")).Returns("arg-value");
        _environment.Setup(e => e.GetEnvironmentVariable("MyKey")).Returns("env-value");
        _environment.Setup(e => e.GetEnvironmentVariable("INPUT_MYKEY")).Returns("input-value");

        var result = _context.Object.CiArgument("MyKey", "default-value");

        result.Should().Be("arg-value");
    }
}
