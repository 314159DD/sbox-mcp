# s&box plugin model - research brief

**Date:** 2026-04-28
**Source:** Facepunch/sbox-public (commit c035ff17)
**Researcher:** Explore subagent
**Confidence levels:** high / medium / low / inconclusive (per section)

## 1. Plugin / addon surface

**High confidence.** s&box has a plugin/addon system, but it is **not documented in the README**. It is discoverable only through source code.

**Discovery mechanism:** s&box loads C# assemblies from a convention-based folder structure:
- Location: `mount/` directory (relative to game root) - `engine/Sandbox.Engine/Game/Mount/Directory.cs:23`
- Convention: `mount/{FolderName}/{FolderName}.dll` (line 26)
- Load method: `Assembly.LoadFile()` (line 38) with reflection-based type discovery via `RegisterTypes()` which integrates with the TypeLibrary
- Static constructors are immediately executed after load (line 41)
- Optional asset filesystem mount at `mount/{FolderName}/assets` (lines 116–123)

**No explicit "Plugin" attribute or interface required.** Discovery is purely reflection-based: loaded assemblies are scanned by the TypeLibrary system. Marking is done via existing attributes like `[AssetType]` (`engine/Sandbox.Engine/Resources/GameResourceAttribute.cs`), `[Editor]` attributes for UI elements, and event handlers registered via `EditorEvent.RegisterAssembly()` (`engine/Sandbox.Tools/ToolsDll.cs:23`).

**Editor integration:** The Tools DLL itself (`engine/Sandbox.Tools/ToolsDll.cs`) is the de-facto template. It implements `IToolsDll` interface (a static singleton), initializes a TypeLibrary, registers event handlers, and enrolls assemblies via `PackageLoader.CreateEnroller()` (line 109).

## 2. Live-reload model

**High confidence.** s&box has **sophisticated hot-reload built in**, via the `Sandbox.Hotload` namespace.

- File watcher on editor assembly: `Sandbox.GameInstanceDll.PackageLoader.HotloadWatch( GetType().Assembly )` (`ToolsDll.cs:111`)
- Trigger: automatic on file change
- Mechanism: `AssemblyLoadContext` (`IsolatedAssemblyContext`, `engine/Sandbox.Hotload/FrameworkSpecific.cs:486`) with **Mono.Cecil for IL rewriting**
- Constraints: **only method bodies and field initializers can change.** Signature changes (parameter counts, return types, type layout) will fail. See `SupportsILHotloadAttribute` (line 753) and `[SkipHotload]` attributes for opting out.
- Member replacement: uses `MemberEqualityComparer` (line 633) to match old types to new types across ALC boundaries
- Callback: `OnAfterHotload` event (`ToolsDll.cs:112`) fires when reload completes. Scenes and global context respond via `OnHotload()` hooks.

## 3. Editor entry points

**High confidence.** Entry point is **static initialization at assembly load time**, not attribute-based discovery.

- **When:** Immediately after `Assembly.LoadFile()` in `Directory.cs:38`, followed by `ReflectionUtility.RunAllStaticConstructors( assembly )` (line 41)
- **How:** All static constructors in the loaded assembly run. This is the hook to initialize editor features.
- **Example:** `ToolsDll.Bootstrap()` (line 20) is called explicitly by the engine bootstrap, but addon assemblies rely on static-constructor side effects.
- **Event system:** `EditorEvent.RegisterAssembly()` allows code to register event listeners without a special attribute (`ToolsDll.cs:23`).
- **Implicit type discovery:** The TypeLibrary system (`Sandbox.Internal.TypeLibrary`) automatically discovers and tracks all public types, allowing downstream code to reflect on them.

## 4. In-editor HTTP / network precedents

**Medium confidence - sparse but present.**

- **WebPanel (`engine/Sandbox.Tools/WebPanel.cs`)**: The engine includes a class that hosts web content in the editor UI via `Game.CreateWebSurface()` (line 31). Not a generic HTTP server, but proves embedded web content is viable.
- **localhost constraint**: One reference in `engine/Sandbox.Engine/Utility/Web/Http.cs` whitelists "ports 80/443/8080/8443" for localhost. **Likely outbound-only** (constraint on what the embedded WebPanel browser can fetch), but unconfirmed.
- **No precedent for long-lived socket / HTTP server**: search for `HttpListener`, `Kestrel`, `TcpListener`, `WebApplication` returned only `Networking.cs` (game-side TCP, not editor tooling).

**Implication:** the single-process HTTP server pattern on localhost is architecturally sound, but the plugin will be the first known case. **Recommend binding 8080 to stay inside the whitelist regardless of whether the restriction applies to inbound or outbound.**

## 5. .NET 10 specifics

**High confidence on runtime; inconclusive on Roslyn at runtime.**

- **Target:** `net10.0` (`Sbox.csproj:7`)
- **Runtime:** No bundled runtime; uses system .NET 10 SDK (`README.md:36`)
- **Available in plugin context:**
  - ✓ Reflection: full
  - ✓ `AssemblyLoadContext`: yes, used in hot-reload (`FrameworkSpecific.cs:486`)
  - ✓ Async / Task: extensively used (`SyncContext.cs` shows MainThread/WorkerThread async patterns)
  - ✓ File watching: implicit in hot-reload (`Zio.IFileSystemWatcher` in `BaseFileSystem.cs`)
  - ✓ P/Invoke / native interop: used (`DLLImportResolver.SetupResolvers`, `Bootstrap.cs:37`)
  - ✓ NuGet dependencies in addon: yes (standard .NET ALC resolution; must be vendored or available on system)
  - ✗ **Roslyn (`Microsoft.CodeAnalysis.*`) at runtime: not confirmed.** Found in build-time tools (SboxBuild, code generation) but no evidence of runtime use in the engine itself. We can bundle it as a NuGet dep if needed.
- **Hot-reload implementation:** custom (Sandbox.Hotload + Mono.Cecil), not Microsoft's `BrowserRefresh`-style.

## 6. Constraints / gotchas

**High confidence on threading and hot-reload; low confidence on sandboxing.**

1. **Threading model:** Single-threaded editor with MainThread sync context (`SyncContext.cs:13, 29`). Plugin code must not block main thread; use `Task` / async for long-running work. `ThreadSafe.AssertIsMainThread()` will catch violations.
2. **Hot-reload signature lock:** Changing method/property signatures or type layouts breaks hot-reload. Use `[SkipHotload]` attribute or accept reload failures.
3. **AssemblyLoadContext isolation:** Each hot-reload cycle creates a new ALC. Old instances unload; **static fields / caches do not persist.** Plugin must handle `OnHotload()` callback to re-initialize state.
4. **No native interop constraints found:** P/Invoke is used by the engine, so it is permitted in addons.
5. **No explicit sandboxing:** The engine does not prevent file I/O, network, or process spawning in plugin code. Trust boundary is at install time.
6. **Mounting precedent is game/addon focused, not editor-tool focused:** `mount/` is designed for game content. Plugin architecture is stable, but tooling patterns are undocumented.

## 7. Documentation status

**Inconclusive.** Official docs may exist at sbox.game/dev/doc/ (`README.md:57`), but were not accessed during this pass. The repo itself has:

- No `DEVELOPMENT.md`, `PLUGINS.md`, or `/docs/` folder
- Inline comments are sparse (implementation, not API contracts)
- Game-side `addons/` folders contain sample game projects - not editor plugin examples
- **The plugin/addon contract is discoverable only by reading engine source.**

---

## Implications for sbox-mcp

1. **Single-process all-C# plugin model is viable.** `mount/sbox-mcp/sbox-mcp.dll` + `Assembly.LoadFile` + TypeLibrary + static-constructor entry. No rework to architecture B.
2. **Localhost HTTP endpoint is safe.** Recommend **port 8080** to stay inside the whitelist regardless of which direction it applies to.
3. **Hot-reload becomes a feature, not a problem.** Mark the MCP server host class `[SkipHotload]` so the long-lived HTTP listener survives. Tool implementation classes CAN be hot-reloaded - that's a free dev-loop win, even better than what we expected.
4. **Tool registry state must live in files (not statics).** Static fields die on hot-reload. Either persist via files / file-backed JSON config, or use `[SkipHotload]` selectively.
5. **No s&box `[EditorTool]`-style attribute system to leverage.** We register tools through the MCP SDK's own registry - not a problem, just confirms our design.

---

## Open questions (couldn't determine in this pass)

1. **Is Roslyn (`Microsoft.CodeAnalysis.*`) available at runtime?** Worst case: bundle as NuGet. Verify in Sprint 1.1.
2. **Is the localhost port whitelist inbound or outbound?** If inbound: must use 80/443/8080/8443. Verify in Sprint 1.1.
3. **`PackageLoader.CreateEnroller()` contract** - used in `ToolsDll:109`. May be the proper plugin entry point versus raw static-ctor. Investigate in Sprint 1.1.
4. **`EditorEvent.RegisterAssembly()` event vocabulary** - what events fire, when? May offer cleaner tool / lifecycle integration than reflection.
5. **sbox.game/dev/doc/ official docs** - quick check warranted before Sprint 1.1.
