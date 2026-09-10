using FluentAssertions;
using SboxMcp.Tools;
using Xunit;

namespace SboxMcp.Tests.Tools;

public class HealthCheckToolTests
{
    [Fact]
    public void Name_IsHealthCheck()
    {
        var tool = new HealthCheckTool(startedAtUtc: DateTime.UtcNow, version: "0.1.0");

        tool.Name.Should().Be("health_check");
    }

    [Fact]
    public void Invoke_ReturnsExpectedShape()
    {
        var startedAt = DateTime.UtcNow.AddSeconds(-10);
        var tool = new HealthCheckTool(startedAt, version: "0.1.0");

        var result = tool.Invoke();

        result.Status.Should().Be("ok");
        result.Version.Should().Be("0.1.0");
        result.UptimeSeconds.Should().BeGreaterOrEqualTo(10).And.BeLessThan(15);
    }

    [Fact]
    public void Invoke_FreshStart_UptimeIsZeroOrSmall()
    {
        var tool = new HealthCheckTool(startedAtUtc: DateTime.UtcNow, version: "0.1.0");

        var result = tool.Invoke();

        result.UptimeSeconds.Should().BeInRange(0, 1);
    }
}
