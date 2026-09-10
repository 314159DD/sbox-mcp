using SboxMcp.Lifecycle;
using SboxMcp.Server;
using SboxMcp.Tools;

namespace SboxMcp;

/// <summary>
/// Composition root. Static constructor runs immediately after
/// `Assembly.LoadFile()` in the engine's mount loader (see
/// engine/Sandbox.Engine/Game/Mount/Directory.cs:38–41).
///
/// Wires: ToolRegistry ← ToolConfig (from JSON) → McpServerHost
/// (Kestrel + MCP SDK) → AssemblyObserver (hot-reload events).
/// EditorEventHandlers register automatically via attribute scan
/// (once engine wiring lands at Task 11).
///
/// TODO (Task 11): Re-add [SkipHotload] attribute - Bootstrap holds
/// the long-lived server reference; without [SkipHotload] a game-code
/// hot-reload would tear down the static state and break the listener.
///
/// TODO (Task 11): swap `Console.WriteLine` → `Log.Info` and
/// `Console.Error.WriteLine` → `Log.Error` so bootstrap-time logs land
/// in the s&box editor log window rather than stdout. The catch block
/// in the static ctor goes to `Log.Error`, not `Log.Info` - easy to
/// fat-finger during a uniform sed pass.
///
/// docs/research/sbox-plugin-model.md has the full pattern.
/// </summary>
public static class Bootstrap
{
    private const string Version = "0.1.0";

    private static McpServerHost? _server;
    private static AssemblyObserver? _observer;
    private static ToolRegistry? _registry;

    static Bootstrap()
    {
        try
        {
            Initialize();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[sbox-mcp] bootstrap failed: {ex}");
            throw;
        }
    }

    private static void Initialize()
    {
        Console.WriteLine("[sbox-mcp] bootstrap starting...");

        // 1. Load tool config (descriptions + schemas live here).
        var configPath = Path.Combine(
            Path.GetDirectoryName(typeof(Bootstrap).Assembly.Location)!,
            "config", "tools.json");
        var config = ToolConfig.LoadFromFile(configPath);
        Console.WriteLine($"[sbox-mcp] loaded tool config v{config.Version} with {config.Tools.Count} tools");

        // 2. Build the registry. (MCP SDK does its own attribute-based
        // discovery for actual dispatch; ToolRegistry is for our
        // introspection / future reflective layer.)
        _registry = new ToolRegistry();
        // HealthCheckTool is registered with MCP SDK via WithTools<>;
        // we'll reflect it into ToolRegistry in Sprint 1.3 once we have
        // the discovery pattern locked.

        // 3. Boot the server. StartAsync returns once Kestrel is
        // listening - non-blocking, won't freeze the editor's main thread.
        _server = new McpServerHost();
        _server.StartAsync(Version).GetAwaiter().GetResult();
        Console.WriteLine($"[sbox-mcp] MCP server up at {McpServerHost.BindUrl}");

        // 4. Subscribe to assembly hot-reload events (currently a no-op
        // stub - engine wiring lands at Task 11).
        _observer = new AssemblyObserver();
        Console.WriteLine("[sbox-mcp] assembly observer wired");

        Console.WriteLine("[sbox-mcp] bootstrap complete");
    }

    /// <summary>
    /// Called by the editor's app.exit event handler so we tear down
    /// the server cleanly instead of letting it leak the port.
    /// </summary>
    public static void Shutdown()
    {
        Console.WriteLine("[sbox-mcp] shutting down");
        _observer?.Dispose();
        _server?.Dispose();
        _observer = null;
        _server = null;
    }
}
