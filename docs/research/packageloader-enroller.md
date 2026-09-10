# PackageLoader.CreateEnroller contract - research brief

**Date:** 2026-04-28
**Source:** Facepunch/sbox-public (local clone)
**Confidence:** High

## What is PackageLoader?

Comprehensive assembly lifecycle manager in `engine/Sandbox.Engine/Services/Packages/PackageManager/PackageLoader.cs:14`. Responsibilities:

1. Load assemblies from packages into a custom `LoadContext` (ALC).
2. Coordinate hotloading - both fast (IL-level patching via `ILHotload`) and full (unload/reload) via `HotloadManager`.
3. Manage access control - verify assemblies against whitelist before loading remote packages.
4. Track loaded state - maintain `Loaded` list of `LoadedAssembly` objects with version + hot-reload metadata.
5. Orchestrate enrollment - dispatch register / unregister / hot-reload events to subscribed `Enroller` listeners.

Public surface:

- `CreateEnroller(string name)` → `Enroller` (line 172)
- `LoadPackage(string ident)` (line 326, internal)
- `HotloadWatch(Assembly)` / `HotloadIgnore(Assembly)` (lines 746–756)
- `OnAfterHotload` callback (line 797)
- `ToolsMode` flag (line 27) - when true, editor/tool assemblies load; when false, editor DLLs are skipped

## What is the Enroller?

Nested public class in `PackageLoader.Enroller.cs:12`. **Registration / notification hub** for a logical scope of loaded assemblies (e.g., "tools", "menu", "gamedll").

Public surface:

- **Constructor:** `Enroller(PackageLoader loader, string name)` (line 27, internal - created via `PackageLoader.CreateEnroller(name)`)
- **Methods:**
  - `LoadPackage(string packageName, bool loadAssemblies = true)` (line 61)
  - `LoadAssemblyFromStream(string name, Stream stream)` (line 79)
  - `GetLoadedAssemblies()` (line 39)
  - `Dispose()` (line 41) - unregisters all assemblies, removes self from `PackageLoader.Enrollers`
- **Events (Action callbacks):**
  - `OnAssemblyAdded(LoadedAssembly)` (line 165)
  - `OnAssemblyRemoved(LoadedAssembly)` (line 166)
  - `OnAssemblyFastHotload(LoadedAssembly)` (line 167)
- **Internal hooks (called by PackageLoader):**
  - `OnRegisterEvent(LoadedAssembly)` (line 117)
  - `OnUnregisterEvent(LoadedAssembly)` (line 128)
  - `OnHotloadEvent(LoadedAssembly)` (line 107)

## What does enrollment provide?

Enrollment is **a subscription pattern**, not a loading mechanism. Compare:

| Aspect | `Assembly.LoadFile()` (raw `mount/`) | `PackageLoader.CreateEnroller()` |
|---|---|---|
| Assembly loads | Yes, but no tracking | Yes, tracked as `LoadedAssembly` |
| Type registration | Manual (`TypeLibrary.AddAssembly()` etc.) | Automatic via `OnAssemblyAdded` |
| Hot-reload awareness | No | Yes (`OnAssemblyFastHotload`, register/unregister) |
| Code archive (reflection metadata) | Not available | `LoadedAssembly.CodeArchiveBytes` |
| Editor assembly filtering | Manual | Automatic - `LoadedAssembly.IsEditorAssembly`; gated by `ToolsMode` |
| Package metadata | None | `LoadedAssembly.Package` reference |
| Hot-reload survival | Unknown / likely fails | Explicit re-registration via `OnRegisterEvent` |
| Dependency management | Manual | Automatic - `LoadPackage()` walks dependencies |

## When is enrollment used in the engine?

| Component | Enroller name | File |
|---|---|---|
| ToolsDll (editor tools) | `"tools"` | `engine/Sandbox.Tools/ToolsDll.cs:109` |
| MenuDll (menu addon) | `"menu"` | `engine/Sandbox.Menu/MenuDll.cs:100` |
| GameInstanceDll (game code, per-session) | `"gamedll{N}"` | `engine/Sandbox.GameInstance/GameInstanceDll.cs:202` |
| **Mount system** | **NONE - uses raw `Assembly.LoadFile`** | `engine/Sandbox.Engine/Game/Mount/Directory.cs:38` |

The mount system (where sbox-mcp lives) does NOT use Enroller - it's the simpler convention path.

## Recommended plugin entry strategy for sbox-mcp

**Hybrid: load via `mount/`, but create our OWN enroller in our static constructor for hot-reload observation.**

We don't enroll OURSELVES - we use the enroller to OBSERVE the engine's other assemblies (tools, game code) so our MCP plugin can keep tool-registry metadata and runtime state in sync with the editor's reality.

```csharp
// inside sbox_mcp's static constructor (runs after Assembly.LoadFile from mount/)
public static class SboxMcpBootstrap
{
    static SboxMcpBootstrap()
    {
        var enroller = PackageLoader.CreateEnroller("sbox-mcp-observer");

        enroller.OnAssemblyAdded += a =>
        {
            // Game / tools assembly loaded - refresh reflective tool schemas
            ToolRegistry.RefreshDynamicTools(a);
        };

        enroller.OnAssemblyFastHotload += a =>
        {
            // IL-patched in place - schema didn't change, no refresh needed
        };

        enroller.OnAssemblyRemoved += a =>
        {
            ToolRegistry.PurgeAssembly(a);
        };
    }
}
```

This gives us:

- Hot-reload awareness via `OnAssemblyAdded` and `OnAssemblyFastHotload` (we see when game code reloads and refresh our reflection-derived tool schemas accordingly).
- Coordinated lifecycle without modifying engine code.
- No dependency on whether ToolsDll's enroller is initialized first.

We do **NOT** modify `engine/Sandbox.Tools/ToolsDll.cs` - the previous agent's "Option C" suggestion to add a special-case there is wrong; it would require forking the engine. The "create our own enroller" approach achieves the same observation goal without any engine changes.

## Concrete next step for Sprint 1.2

1. Plugin loads via `mount/sbox-mcp/sbox-mcp.dll` + static constructor (mount convention).
2. Static constructor immediately:
   - Creates `Enroller("sbox-mcp-observer")` for game/tools assembly observation.
   - Spins up the Streamable HTTP MCP server on `127.0.0.1:8080`.
   - Marks the server class `[SkipHotload]` so the listener survives the plugin's own hot-reload events.
   - Subscribes to relevant `[Event(...)]` attributes (see `editor-event-vocabulary.md`).
3. On hot-reload of game/tools code, the enroller callbacks fire and the tool registry refreshes its reflection-derived schemas.

## Open questions

1. Does `ToolsDll`'s enroller (`"tools"`) get created before our static constructor runs from `mount/`? If yes, our enroller can immediately receive `OnAssemblyAdded` for already-loaded editor assemblies. If no (plugin loads earlier), we miss the initial registrations and only get future ones. Test in Sprint 1.2.
2. Can an enroller "adopt" already-loaded assemblies retroactively, or only see assemblies loaded *after* it's created? `Enroller.Add()` appears internal-only; broadcast on `PackageLoader.Enrollers` happens at register-time, suggesting future-only.
3. Does our enroller need explicit cleanup on plugin unload? `Dispose()` exists; should be called from `OnAssemblyRemoved` for our own assembly.

These are tractable runtime questions for Sprint 1.2 implementation, not blocking design.

## Conclusion

For Sprint 1.2: keep `mount/sbox-mcp/sbox-mcp.dll` + static-ctor entry. Inside the static constructor, **create our own enroller** to observe other assemblies' hot-reload events and refresh our reflective tool schemas accordingly. No engine code modification required.
