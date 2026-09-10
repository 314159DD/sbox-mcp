namespace SboxMcp.Lifecycle;

/// <summary>
/// Editor lifecycle handlers. Methods will be invoked by the engine's
/// EditorEvent system once wired up at Task 11.
///
/// See docs/research/editor-event-vocabulary.md for the full event
/// vocabulary. Sprint 1.2 wires the lifecycle subset we need for boot
/// + shutdown sanity. Sprint 1.3 will add scene-edit hooks (the
/// ISceneEdited interface pattern).
///
/// TODO (Task 11 - manual integration): re-enable engine wiring.
///   - Add `using Editor;` (for [Event] attribute) and `using Sandbox;`
///     (for Log.Info).
///   - Restore the [Event("...")] attributes on each method.
///   - Replace Console.WriteLine calls with Log.Info for s&box logger
///     integration.
/// EditorEvent attribute discovery is automatic via assembly scan
/// (engine/Sandbox.Tools/Events/EventSystem.cs:164) - survives
/// hot-reload via the orphaned-instance re-attach pattern.
/// </summary>
public static class EditorEventHandlers
{
    // TODO (Task 11): [Event("editor.created")]
    public static void OnEditorCreated()
    {
        Console.WriteLine("[sbox-mcp] editor.created - server should already be up.");
    }

    // TODO (Task 11): [Event("hotloaded")]
    public static void OnHotloaded()
    {
        Console.WriteLine("[sbox-mcp] hotloaded - tool registry refresh deferred to Sprint 1.3.");
    }

    // TODO (Task 11): [Event("scene.play")]
    public static void OnScenePlay()
    {
        Console.WriteLine("[sbox-mcp] scene.play - runtime tools become available.");
    }

    // TODO (Task 11): [Event("scene.stop")]
    public static void OnSceneStop()
    {
        Console.WriteLine("[sbox-mcp] scene.stop - runtime tools no longer applicable.");
    }

    // TODO (Task 11): [Event("app.exit")]
    public static void OnAppExit()
    {
        Console.WriteLine("[sbox-mcp] app.exit - tearing down server.");
        SboxMcp.Bootstrap.Shutdown();
    }
}
