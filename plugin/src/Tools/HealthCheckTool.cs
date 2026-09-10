using ModelContextProtocol.Server;
using System.ComponentModel;

namespace SboxMcp.Tools;

public sealed record HealthCheckResult(
    string Status,
    string Version,
    long UptimeSeconds);

[McpServerToolType]
public sealed class HealthCheckTool : ITool
{
    private readonly DateTime _startedAtUtc;
    private readonly string _version;

    public HealthCheckTool(DateTime startedAtUtc, string version)
    {
        _startedAtUtc = startedAtUtc;
        _version = version;
    }

    public string Name => "health_check";

    [McpServerTool, Description("Returns plugin health, version, and uptime. Use to verify the MCP server is alive and reachable.")]
    public HealthCheckResult Invoke()
    {
        var uptime = (long)(DateTime.UtcNow - _startedAtUtc).TotalSeconds;
        return new HealthCheckResult("ok", _version, uptime);
    }
}
