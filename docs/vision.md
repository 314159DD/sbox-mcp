# Product Vision

**Updated:** 2026-04-28

## What This Is

`sbox-mcp` is a Model Context Protocol server that lets you direct Facepunch's [s&box](https://github.com/Facepunch/sbox-public) game engine through natural language. From any MCP-aware LLM client (Claude Code, Cursor, etc.), you describe what you want in the engine and the MCP server makes it happen - generates C# component scripts, manipulates the editor's scene graph, controls a running game.

It also doubles as a learning **interface** for the engine: you do not need to know every menu and setting. You tell the agent what you want and the agent operates the engine.

## Problem

Modern game engines have steep tool-discovery curves. A newcomer to s&box (or Unity, or Unreal) spends weeks learning panels, hotkeys, asset workflows, and component conventions before they can ship a prototype. Existing AI coding tools (Copilot, Cursor) help with code but can't drive the *engine* - they can't place an entity in the scene, configure a material, or hook up an input.

s&box is a perfect target because:
1. It's brand new and open-source (MIT, dropped 2026-04-28).
2. The scripting layer is C# / .NET 10, which gives us very strong programmatic surfaces (Roslyn for codegen, reflection for introspection, AssemblyLoadContext for hot-loading).
3. There's no existing "AI co-driver" for it. First-mover.

The same pattern works for other engines, but s&box is where we're starting.

## Core Features

1. **Code generation** - generate, edit, refactor C# component scripts. The editor's existing hot-reload picks them up.
2. **Scene authoring** - manipulate the editor's scene graph: create entities, set transforms, parent / unparent, attach components, configure properties.
3. **Asset operations** - import models, set materials, configure prefabs.
4. **Live runtime control** - when the game is running, spawn / modify / inspect entities, change globals (gravity, time scale), attach debug overlays.
5. **Project introspection** - list assets, scenes, components; surface compile errors and runtime exceptions back to the LLM.
6. **Schema-typed tools** - every MCP tool is JSON-Schema validated so the LLM gets typed errors instead of silent failures.

## Non-Goals

- **Not a chat UI.** Use existing MCP clients. We don't build our own chat surface.
- **Not a code-completion tool.** This isn't Copilot-at-the-cursor. Operations are project-level.
- **Not a public sbox plugin in v1.** Personal-use first. Possibly open-sourced later, but distribution is a v2 question.
- **Not engine-agnostic.** Other engines are interesting but s&box-specific knowledge will leak into the design and we accept that.
- **No autonomous "AI builds your game" mode.** The user stays in the loop - every operation is a tool call the LLM proposes and the operator can review.

## Current State

Phase 0 - design + scaffolding. No code yet.

| Metric | Value |
|--------|-------|
| Engine source available | Yes (MIT, public 2026-04-28) |
| MCP server prototype | None |
| Editor plugin prototype | None |
| Working tool calls | 0 |

## Revenue Model

None initially. Personal tool. Possible futures (not committed):
- Open-source MIT, build community of sbox developers using AI workflows.
- Premium variant with hosted prompt patterns, undo / replay, multi-session memory.
- Sponsored by the s&box ecosystem if it gains traction.

## Architecture (high-level)

```
[MCP Client]              [MCP Server]              [Editor Plugin]              [s&box]
 Claude Code      ⇄        Python              ⇄        C# / .NET 10        ⇄    Editor + runtime
 Cursor                    sbox_mcp module              loaded as engine plugin    Source 2 + .NET 10
 (any MCP-aware)           speaks MCP                   exposes JSON-RPC port      scripting API
```

See [decisions/](decisions/) for the architecture decision records and [research/](research/) for engine internals notes.

## What Success Looks Like

1. **Prompt → scene edit:** "Place a red bouncing cube at the camera origin and make it spawn 3 children when clicked." Cube appears in editor, behavior works in play mode.
2. **Prompt → component:** "Write me a Component that plays a sound on overlap and attach it to that prop." File written, hot-reloaded, attached, working.
3. **Prompt → runtime control:** While the game is running: "spawn 50 zombies in a circle around the player." Zombies appear; transcript readable from the LLM client.
4. **End-to-end demo:** build a small playable s&box scene from zero entirely through the MCP, without using the editor menus directly. This is the bar - until this works, we're not done.
5. **Personal viability:** the author would rather use the MCP than touch the editor manually for more than 80 percent of s&box work.
