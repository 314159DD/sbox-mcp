---
title: "Architecture: Single-Process All-C# Plugin (B over A)"
date: 2026-04-28
status: Accepted
deciders: project owner
supersedes: Initial brainstorm-level assumption of two-process Python + C# plugin
---

# Architecture: Single-Process All-C# Plugin

## Context

The MCP needs to interact with the s&box editor (and runtime) to provide AI-driven control. Three architectures were considered during brainstorming:

- **A.** Two-process: Python MCP server + C# editor plugin, JSON-RPC bridge between them
- **B.** Single-process: one all-C# plugin that speaks MCP directly to the LLM client
- **C.** File-driven: Python MCP server only, manipulating project files on disk (no in-editor plugin)

**C** was eliminated immediately - can't access live editor state or runtime, blocking Phases 2 and 3.

The real fork was **A vs B**. The initial recommendation was A based on assumed maturity gap between Python and C# MCP SDKs. That assumption proved stale: actual state of `modelcontextprotocol/csharp-sdk` as of 2026-03-27 is **v1.2.0 stable, official, maintained in collaboration with Microsoft, three NuGet packages (Core, hosting, ASP.NET Core HTTP), 4.2k stars**. With that fact in place, the SDK-maturity argument against B disappears.

The stated priority is "end-result polish over build-time DX."

## Decision

**Adopt B: single-process all-C# plugin.**

The plugin loads into the s&box editor at startup, hosts the Model Context Protocol server using the official `ModelContextProtocol` .NET SDK, exposes a localhost Streamable HTTP endpoint, and calls engine APIs directly in-process.

## Reasoning

1. **C# MCP SDK is production-grade.** Official, Microsoft-collaborated, v1.2.0 stable as of 2026-03-27 (verified via WebFetch on the day of decision).
2. **Performance difference is imperceptible.** ~3–5ms per tool call vs A, inside an 800–3000ms LLM response loop. Even at 10 tool calls per response, the delta is ≤50ms. The user cannot tell.
3. **Single-binary distribution.** Drop one DLL into the s&box plugins folder. No Python install, no two-process orchestration, no MCP-to-engine bridge port to manage.
4. **Owner priority.** The project prefers end-result polish over build-time DX for a personal-tool-first project that may eventually be shared with the s&box community.
5. **Lower stack complexity.** One language, one build, one process. Simpler mental model and simpler ops story.
6. **In-process == no serialization between MCP and engine.** Direct .NET API calls; no JSON between layers.

## Consequences

### Positive

- Single deployment artifact (one DLL).
- One process - no port management for an internal bridge.
- No serialization overhead between MCP layer and engine.
- Lower operational complexity for the end user.
- Aligns with the s&box plugin / addon convention (when one is documented).

### Negative

- **Recompile required for non-metadata tool changes** (vs ~1s Python restart in A). See mitigations.
- Bug in the MCP layer can crash the editor (vs A, where Python isolation protects the editor). Mitigated by standard exception-handling discipline at every tool boundary.
- Tied to C# / .NET - swapping MCP server implementation language later would require major rework.

### Mitigations for the iteration tax

These bring most of A's iteration speed back for the parts that get tweaked most often:

1. **.NET 10 hot-reload** for small / metadata-only changes - reloads in <2s without a full rebuild.
2. **Tool descriptions + JSON Schemas in a JSON config** the plugin reads at startup. Reloadable at runtime via a `reload_tool_metadata` tool. Edit JSON → call reload → no recompile. The most-iterated artifacts (tool descriptions, schemas, error wording) live here.
3. **s&box plugin live-reload** - verify support and integration path in Sprint 1.1 design.

## Alternatives Considered

- **A. Two-process Python MCP server + C# plugin.** Build-DX wins (~1s Python restart) are real but not worth the deployment + distribution complexity for a personal-tool-first project. C# SDK maturity is no longer a deciding factor.
- **C. File-driven Python-only MCP.** Cannot satisfy Phases 2 (scene authoring) or 3 (live runtime). Hard rejection.
- **Hybrid (thin C# RPC plugin + Python MCP server with arbitrary C# evaluation).** Considered briefly, rejected on security grounds - executing arbitrary C# inside the editor is an enormous attack surface, and we'd be reinventing what the C# SDK already gives us safely.

## Implementation Implications

- Plugin uses `ModelContextProtocol.AspNetCore` for the Streamable HTTP host.
- Plugin binds `127.0.0.1:<port>` only - never bind on a public interface.
- Tool registry has a clear separation between metadata (description, JSON Schema) and handler delegates, so metadata can hot-reload independently.
- Standard exception-handling discipline at every tool boundary (catch, structure into MCP error response, never let exceptions escape into the editor's main loop).

## Revisit If

- Iteration tax in C# proves crippling AND the mitigations above don't help → reconsider A.
- Official C# MCP SDK is abandoned or falls badly behind the spec → reconsider transport / SDK choice.
- We need to swap MCP server implementation language (e.g. for cross-engine support) → reconsider language split.
- s&box's plugin loader turns out to have constraints that block this design → revisit in Sprint 1.1.

## References

- C# MCP SDK: https://github.com/modelcontextprotocol/csharp-sdk
- See also: [`../roadmap.md`](../roadmap.md).
