# Roadmap

**Updated:** 2026-04-28

## Core Loop

The fundamental flow that everything else serves:

```
User prompt
   ↓
MCP client (Claude Code / Cursor)
   ↓ MCP tool call (Streamable HTTP, 127.0.0.1:8080)
sbox-mcp plugin (C# / .NET 10, in-editor)
   ↓ in-process .NET API call
s&box editor / runtime
   ↓ result
[reverses back up the stack]
```

## Build sequencing - tool-quality axis (locked 2026-04-28)

We build by **tool quality tier**, not by capability axis. This gets end-to-end working faster and lets curated tools be informed by observed LLM behavior. See [`docs/decisions/2026-04-28-tool-design-hybrid.md`](docs/decisions/2026-04-28-tool-design-hybrid.md).

- **Tier R - Reflective generics** (~17 tools). Generic tools that cover the entire engine API surface via reflection. Foundation tier. End-to-end demoable across all three capability layers (codegen / scene / runtime) once landed.
- **Tier C - Curated primitives** (~25 tools). Hand-authored, well-described per-domain operations. Built on top of Tier R, informed by observed LLM patterns. Polish tier.
- **Tier M - Macros** (~5–8 tools). Atomic create-and-configure flows for high-frequency patterns. Top-level magic.

## Feature Map

| # | Feature | Tier | Priority | Phase | Sprint | Status |
|---|---------|------|----------|-------|--------|--------|
| 1 | Sprint 1.1: technical design spec → planning | - | Core | 0 | 1.1 | done (2026-04-28) |
| 2 | Plugin skeleton in `mount/sbox-mcp/sbox-mcp.dll` | - | Core | 1 | 1.2 | done (2026-04-28) |
| 3 | MCP server boot on `127.0.0.1:8080`, Streamable HTTP | - | Core | 1 | 1.2 | done (2026-04-28) |
| 4 | First end-to-end tool: `health_check` round-trip | - | Core | 1 | 1.2 | done (2026-04-28, integration test deferred) |
| 5 | Reflective: `describe_type`, `list_types`, `find_components` | R | Core | 1 | 1.3 | pending |
| 6 | Reflective: `set_property`, `get_property`, `call_method` | R | Core | 1 | 1.3 | pending |
| 7 | Reflective scene: `list_entities`, `get_entity`, `create_entity`, `delete_entity`, `attach_component` | R | Core | 1 | 1.4 | pending |
| 8 | Reflective IO: `read_file`, `write_file`, `apply_patch`, `compile_status` | R | Core | 1 | 1.5 | pending |
| 9 | Reflective runtime: `runtime_status`, `save_scene`, `load_scene` | R | Core | 1 | 1.6 | pending |
| 10 | **Tier R end-to-end demo** (M1 bar) | - | Core | 1 | 1.7 | pending |
| 11 | Curated codegen: `create_component`, `edit_component`, structured compile-error model | C | Core | 2 | 2.1 | pending |
| 12 | Curated scene: `set_transform`, `parent_entity`, `attach_component_by_name`, `place_object` | C | Core | 2 | 2.2 | pending |
| 13 | Curated assets: `import_model`, `set_material`, `list_assets` | C | Enhancement | 2 | 2.3 | pending |
| 14 | Curated runtime: `spawn_at_runtime`, `mutate_global`, `dump_runtime_state`, debug overlays | C | Core | 2 | 2.4 | pending |
| 15 | **Tier C end-to-end demo** (M2 bar) | - | Core | 2 | 2.5 | pending |
| 16 | Macros: `create_configured_entity`, `create_component_with_template` | M | Enhancement | 3 | 3.1 | pending |
| 17 | Macros: `duplicate_entity`, `snap_to_grid`, `place_with_behavior` | M | Enhancement | 3 | 3.2 | pending |
| 18 | Undo / dry-run mode, structured exception model | - | Enhancement | 3 | 3.3 | pending |
| 19 | **M4: build a small playable scene end-to-end through the MCP only** | - | Core | 3 | 3.4 | pending |

Priority: Core | Enhancement
Status: pending | in_progress | done

## Phase 0 - Kickoff

**Start:** 2026-04-28
**Status:** in progress.
**Goal:** Project scaffolded, plan complete, design decisions captured, ready for implementation.

### Sprint 1.1 - Technical design spec → implementation plan

**Goal:** Resolve open Sprint 1.1 follow-ups (Roslyn-at-runtime, port whitelist direction, `PackageLoader.CreateEnroller` contract, EditorEvent vocabulary, sbox.game/dev/doc check), produce the implementation plan for Phase 1 via the writing-plans skill.

## Phase 1 - Reflective foundation

**Goal:** Ship Tier R coverage across all three capability layers (codegen, scene, runtime) so the system is end-to-end demoable. This is the foundation; subsequent phases polish.

### Sprint 1.2 - Plugin skeleton + MCP transport ✅ DONE (2026-04-28)

Implementation complete. 10/10 unit tests pass, release build clean (0 errors / 0 warnings, 18MB output). Plugin DLL produced at `plugin/bin/Release/net10.0/sbox-mcp.dll`. **Manual integration test in s&box editor deferred** - the smoke test (Phase 11A) is still to be run. Engine wiring (Phase 11B - `[SkipHotload]`, `[Event]`, `Sandbox.Log`, `PackageLoader.CreateEnroller`) is also deferred behind clear `TODO (Task 11)` markers in the source. Sprint file archived.

Original sprint scope:
- Plugin DLL at `mount/sbox-mcp/sbox-mcp.dll`
- MCP server (Microsoft .NET SDK) boot on `127.0.0.1:8080`, Streamable HTTP
- `health_check` first-tool round-trip
- `[SkipHotload]` on the server host class
- Tool registry in JSON config, reloadable at runtime

### Sprint 1.3 - Reflective introspection
- `describe_type`, `list_types`, `find_components`
- `set_property`, `get_property`, `call_method`
- Dynamic schemas for property paths

### Sprint 1.4 - Reflective scene
- `list_entities`, `get_entity`, `create_entity`, `delete_entity`
- `attach_component` (generic, by type name)

### Sprint 1.5 - Reflective IO + codegen
- `read_file`, `write_file`, `apply_patch` (project-scoped)
- `compile_status` - surface compile errors

### Sprint 1.6 - Reflective runtime
- `runtime_status`
- `save_scene`, `load_scene`

### Sprint 1.7 - Tier R end-to-end demo
- Run through codegen → scene → runtime demo via reflective tools only
- Capture LLM friction patterns to inform Tier C scope

## Phase 2 - Curated primitives

**Goal:** Layer ergonomic per-domain tools on top of Tier R, informed by observed LLM patterns from Sprint 1.7.

### Sprint 2.1 - Codegen primitives
### Sprint 2.2 - Scene primitives
### Sprint 2.3 - Asset primitives (Enhancement)
### Sprint 2.4 - Runtime primitives
### Sprint 2.5 - Tier C end-to-end demo

## Phase 3 - Macros, hardening, demo

**Goal:** Atomic compose flows + DX polish + the M4 demo.

### Sprint 3.1 - Compose macros
### Sprint 3.2 - Place / duplicate / arrange macros
### Sprint 3.3 - Undo, dry-run, structured exceptions
### Sprint 3.4 - **M4 demo**: build a small playable s&box scene end-to-end through MCP only

## Future Phases (post-M4, sketch only)

### Phase 4 - Distribution

If interest emerges from the s&box community: package as a public addon, documentation, multi-project support.

### Phase 5 - Other engines

Generalize the plugin pattern to Unity / Godot / Unreal - out of scope until s&box is solid.
