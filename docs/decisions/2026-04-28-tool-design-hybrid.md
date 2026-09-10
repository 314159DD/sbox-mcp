---
title: "Tool Design Philosophy: Hybrid, Reflective-First"
date: 2026-04-28
status: Accepted
deciders: project owner
supersedes: Initial brainstorm-level "primitives-first" assumption in roadmap
---

# Tool Design Philosophy: Hybrid, Reflective-First

## Context

Three philosophies for designing the MCP tool surface were considered:

1. **Curated primitives only** - hand-author every operation. ~30–40 well-tested, well-described tools. LLM composes operations.
2. **Reflective generic only** - a handful of generic tools that cover everything via reflection (`set_property`, `attach_component`, `call_method`, etc.). ~10 tools total. LLM uses introspection to find paths.
3. **Hybrid** - primitives + reflective fallback + a few macros for atomic compose flows.

## Decision

**Hybrid (Option 3), built reflective-first.**

The full surface at maturity will be ~50 tools across three quality tiers:

- **Tier R - Reflective generics** (~17 tools). Cover *everything* the engine exposes via reflection. Examples: `describe_type`, `list_types`, `find_components`, `set_property`, `get_property`, `call_method`, `attach_component`, `list_entities`, `get_entity`, `create_entity`, `delete_entity`, `save_scene`, `load_scene`, `compile_status`, `runtime_status`, `read_file`, `write_file`, `apply_patch`. These reach 100% engine API coverage with minimal API surface.
- **Tier C - Curated primitives** (~25 tools). Hand-authored, well-described per-domain operations: `set_transform`, `parent_entity`, `attach_component_by_name`, `spawn_at_runtime`, `create_component`, `set_global`, etc. One-call ergonomics for the common cases observed in Tier R usage.
- **Tier M - Macros** (~5–8 tools). Atomic create-and-configure flows: `create_configured_entity(spec)`, `create_component_with_template(name, pattern)`, `duplicate_entity(id, offset)`. One-call magic for high-frequency creation patterns.

## Build sequencing

**Reflective FIRST. Curated SECOND. Macros THIRD.**

This inverts the original capability-axis roadmap (codegen → scene → runtime) into a tool-quality-axis roadmap. With the reflective tier alone, the LLM can already do anything the engine permits - codegen via `write_file`/`apply_patch`/`compile_status`, scene authoring via `set_property`/`attach_component`/`list_entities`, runtime control via `runtime_status`/`call_method` on live entities. Tier R is the foundation that proves the whole system works end-to-end.

Tier C then polishes: we build it on top of observed friction in Tier R usage. We see which calls the LLM gets wrong, which patterns it repeats, which combinations it composes, and curate accordingly. **No curation guesses** - empirical, data-informed.

Tier M is the final polish layer: atomic operations for the most common compose patterns observed during Tier C use.

## Reasoning

1. **Reflective coverage gets to "feels useful" fastest.** Once Tier R works, you can demo end-to-end across all three capability layers. M4 (the bar: build a small playable scene start-to-finish) becomes reachable without curating anything.
2. **Curated tools are easier when informed by real usage.** We avoid building primitives the LLM doesn't actually need. Saves a meaningful percentage of work that would otherwise go to dead code.
3. **The reflective fallback is permanent value.** Even at maturity, when curated primitives exist for the common cases, the reflective tier lets the LLM reach engine APIs we never anticipated curating. That's the difference between "operate the engine within our designed flows" and "operate the engine, period."
4. **Token cost is acceptable in early phase.** Reflective tools require more reasoning per task (introspect → set), which means more tokens per turn. Token spend is not a concern during the build phase (only at production-use phase). By the time the tool is dogfooded, Tier C + M will be live, dropping turn counts.
5. **Owner ambition signal.** "Really all we can do and get" / "Really wanna go all out" → no compromise on coverage. Hybrid is the only philosophy that delivers comprehensive coverage AND ergonomic common-case calls.

## Consequences

### Positive

- End-to-end working state in Phase 1 (after Tier R lands), not Phase 3.
- Curated tools are designed against real LLM behavior, not guesses.
- Reflective tier is permanent infrastructure - value beyond any single curated tool.
- Larger total surface (~50) but well-tagged by category in the MCP tool registry.

### Negative

- Higher per-task token usage during Tier R-only period. Acceptable per owner priority.
- LLM has to choose between reflective and curated tools once both exist - risk of confusion. Mitigation: tool descriptions explicitly call out "use the curated `set_transform` instead of reflective `set_property` when setting a transform" / similar guidance.
- Reflection-based tools have weaker static schemas (component-typed properties handled at runtime). Mitigation: dynamic schemas served via the MCP `tools/list` response, refreshed as types change.
- Some reflective tools are **dangerous-by-default** - `evaluate_expression(csharp_code)` is essentially "run arbitrary C# in the editor." Either gate it behind an explicit opt-in, or omit from v1. Decided in Sprint 1.1.

## Implementation Implications

- Tool registry stores both static (curated) and dynamic (reflection-derived) entries. Curated tool descriptions in JSON config; reflective tool schemas generated at runtime from loaded type information.
- `tools/list` MCP response combines both sources.
- Tool tags / categories: `tier:reflective | tier:curated | tier:macro`, `domain:codegen | domain:scene | domain:runtime | domain:introspection`.
- Tier C and M can hot-reload (their classes live in the plugin, .NET 10 hot-reload covers method bodies). Tier R schemas refresh when the engine type universe changes - likely on hot-reload events.

## Alternatives Considered

- **Curated primitives only.** Rejected: artificial ceiling on what the LLM can do. Would force maintaining a wrapping library that mirrors the entire engine API, growing forever.
- **Reflective generic only.** Rejected: poor ergonomics for common operations, every task needs introspection round-trips, schema validation gets hand-rolled per type.
- **Build curated-first, add reflection later.** Rejected: misses the "informed by real usage" benefit, dead-code risk on primitives that don't earn use.

## Revisit If

- Reflective tier proves too LLM-confusing in practice (LLM consistently misuses generic tools where a curated one would have been clearer) → reorder so a minimal Tier C lands alongside Tier R rather than after.
- Tool count budget (~50) feels too noisy - LLM picks wrong tools - → consolidate or hide low-frequency ones via tags.
- A general-purpose engine adapter pattern emerges in the MCP / Anthropic ecosystem we should adopt → reconsider then.

## References

- Related: [`plan/decisions/2026-04-28-architecture-single-process.md`](2026-04-28-architecture-single-process.md), [`plan/research/sbox-plugin-model.md`](../research/sbox-plugin-model.md).
- See also: [`plan/roadmap.md`](../roadmap.md) - phases reorganized to reflect this sequencing.
