using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using SboxMcp.Tools;

namespace SboxMcp.Server;

/// <summary>
/// Owns the in-process ASP.NET Core MCP server.
///
/// TODO (Task 11): Re-add [SkipHotload] attribute when engine assemblies
/// are referenced (manual integration in s&box editor). This attribute is
/// required so the long-lived HTTP listener and any active MCP-client
/// connections survive when game/tools assemblies hot-reload around it.
/// (See docs/research/sbox-plugin-model.md.)
///
/// Bound to 127.0.0.1:8080 to stay inside s&box's localhost outbound
/// whitelist (see docs/research/port-whitelist.md).
/// </summary>
public sealed class McpServerHost : IDisposable
{
    public const string BindUrl = "http://127.0.0.1:8080";
    public DateTime StartedAtUtc { get; private set; }

    private WebApplication? _app;

    public async Task StartAsync(string version)
    {
        StartedAtUtc = DateTime.UtcNow;
        var startedAt = StartedAtUtc;

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(BindUrl);

        // HealthCheckTool's instance methods are dispatched via the SDK's
        // WithTools<T>(), which constructs a target per invocation through DI.
        // Registering the singleton instance here is load-bearing: it gives every
        // call the same captured startedAt + version. Without this line the SDK
        // would fall back to ActivatorUtilities.CreateInstance and fail to inject
        // the (DateTime, string) constructor parameters.
        builder.Services
            .AddSingleton(new HealthCheckTool(startedAt, version))
            .AddMcpServer()
            .WithHttpTransport(o => o.Stateless = true)
            .WithTools<HealthCheckTool>();

        var app = builder.Build();
        app.MapMcp();

        try
        {
            await app.StartAsync();
            _app = app;
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }
    }

    public void Dispose()
    {
        _app?.StopAsync().GetAwaiter().GetResult();
        _app = null;
    }
}
