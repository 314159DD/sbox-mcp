using FluentAssertions;
using SboxMcp.Tools;
using Xunit;

namespace SboxMcp.Tests.Tools;

public class ToolConfigTests
{
    [Fact]
    public void LoadFromJson_ParsesValidConfig()
    {
        const string json = """
            {
              "version": 1,
              "tools": [
                {
                  "name": "health_check",
                  "description": "Returns plugin health.",
                  "schema": { "type": "object" }
                }
              ]
            }
            """;

        var config = ToolConfig.LoadFromJson(json);

        config.Version.Should().Be(1);
        config.Tools.Should().HaveCount(1);
        config.Tools[0].Name.Should().Be("health_check");
        config.Tools[0].Description.Should().Be("Returns plugin health.");
    }

    [Fact]
    public void LoadFromJson_MalformedJson_Throws()
    {
        var act = () => ToolConfig.LoadFromJson("{ not json");

        act.Should().Throw<System.Text.Json.JsonException>();
    }

    [Fact]
    public void LoadFromJson_MissingTools_ReturnsEmptyList()
    {
        const string json = """{ "version": 1 }""";

        var config = ToolConfig.LoadFromJson(json);

        config.Tools.Should().BeEmpty();
    }
}
