using FluentAssertions;
using SboxMcp.Tools;
using Xunit;

namespace SboxMcp.Tests.Tools;

public class ToolRegistryTests
{
    [Fact]
    public void Register_AddsTool_RetrievableByName()
    {
        var registry = new ToolRegistry();
        var tool = new StubTool("my_tool");

        registry.Register(tool);

        registry.Get("my_tool").Should().BeSameAs(tool);
    }

    [Fact]
    public void Get_UnknownTool_Throws()
    {
        var registry = new ToolRegistry();

        var act = () => registry.Get("nonexistent");

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void Register_DuplicateName_Throws()
    {
        var registry = new ToolRegistry();
        registry.Register(new StubTool("dup"));

        var act = () => registry.Register(new StubTool("dup"));

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Names_ReturnsAllRegisteredToolNames()
    {
        var registry = new ToolRegistry();
        registry.Register(new StubTool("a"));
        registry.Register(new StubTool("b"));

        registry.Names.Should().BeEquivalentTo(["a", "b"]);
    }

    private sealed class StubTool : ITool
    {
        public StubTool(string name) => Name = name;
        public string Name { get; }
    }
}
