namespace SboxMcp.Lifecycle;

/// <summary>
/// Observes hot-reload events on engine + game assemblies via
/// PackageLoader.CreateEnroller. Sprint 1.2 only logs events;
/// Sprint 1.3 will use these to refresh the reflective tool registry.
///
/// Pattern from engine: ToolsDll.cs:109, MenuDll.cs:100,
/// GameInstanceDll.cs:202. Each component creates its own enroller.
///
/// TODO (Task 11 - manual integration): re-enable the engine wiring
/// below once Sandbox engine assemblies are referenced. The body is
/// stubbed to a no-op so Bootstrap can compose it without engine deps.
/// When wired up:
///   - Add `using Sandbox;` and `using Sandbox.GameInstanceDll;`
///   - Restore the _enroller field + ctor body + Dispose body
///   - Restore OnAdded / OnRemoved / OnFastHotload bodies
/// See docs/research/packageloader-enroller.md for the full pattern.
/// </summary>
public sealed class AssemblyObserver : IDisposable
{
    // TODO (Task 11): private readonly Enroller _enroller;

    public AssemblyObserver()
    {
        // TODO (Task 11): wire up engine enroller.
        // _enroller = PackageLoader.CreateEnroller("sbox-mcp-observer");
        // _enroller.OnAssemblyAdded += OnAdded;
        // _enroller.OnAssemblyRemoved += OnRemoved;
        // _enroller.OnAssemblyFastHotload += OnFastHotload;
    }

    // TODO (Task 11): restore engine-aware handlers.
    // private static void OnAdded(LoadedAssembly a) =>
    //     Log.Info($"[sbox-mcp] assembly added: {a.Name}");
    //
    // private static void OnRemoved(LoadedAssembly a) =>
    //     Log.Info($"[sbox-mcp] assembly removed: {a.Name}");
    //
    // private static void OnFastHotload(LoadedAssembly a) =>
    //     Log.Info($"[sbox-mcp] fast-hotload: {a.Name}");

    public void Dispose()
    {
        // TODO (Task 11): unsubscribe handlers + dispose enroller.
        // _enroller.OnAssemblyAdded -= OnAdded;
        // _enroller.OnAssemblyRemoved -= OnRemoved;
        // _enroller.OnAssemblyFastHotload -= OnFastHotload;
        // _enroller.Dispose();
    }
}
