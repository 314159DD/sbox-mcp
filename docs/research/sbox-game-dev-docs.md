# sbox.game/dev/doc - official documentation reachability brief

**Date:** 2026-04-28
**Source:** WebFetch + repo README cross-check
**Confidence:** High (on existence and rendering); not-applicable for content (couldn't read the docs)

## URL inventory

| URL | Status | Notes |
|---|---|---|
| https://sbox.game/ | 200 | Main website |
| https://sbox.game/dev/ | 200 | Developer hub - Blazor Server-rendered, returns skeleton HTML on WebFetch |
| https://sbox.game/dev/doc/ | 200 | Documentation root - Blazor Server-rendered |
| https://sbox.game/dev/doc/about/getting-started/first-steps/ | likely 200 | Getting started, not directly fetched |
| https://sbox.game/dev/doc/addons/ | likely 200 | Addon docs (referenced by README) |
| https://docs.facepunch.com/s/sbox/ | 404 | Not found |
| https://wiki.facepunch.com/sbox/ | 404 | Not found |

## What this means

Official documentation **exists** and is the canonical reference (`README.md:57` directs all developer questions there). It is **Blazor Server-rendered** - the initial HTML response is a skeleton; the actual content is rendered client-side via JavaScript. WebFetch / curl / static HTML scrapers receive only the skeleton, so we cannot read the content programmatically.

## Why this isn't blocking

The four other research briefs from 2026-04-28 (`roslyn-at-runtime`, `port-whitelist`, `packageloader-enroller`, `editor-event-vocabulary`) answer every Sprint-1.2-blocking question directly from the engine source code. The official docs would be **confirmatory and supplementary**, not foundational, for our work. Specifically:

- Plugin entry point: nailed down via `mount/{name}/{name}.dll` + static-ctor (Directory.cs).
- Hot-reload: nailed down via Sandbox.Hotload + Mono.Cecil + `[SkipHotload]`.
- Threading: single main thread + `ThreadSafe.AssertIsMainThread()`.
- Port restrictions: outbound-only whitelist.
- Roslyn: baked in at runtime.
- EditorEvent: full vocabulary captured.
- Enroller: full contract captured.

## Recommended action

**Manual browser visit only when** Sprint 1.2 / 1.3 implementation hits a specific gap that the source-code audit didn't cover. Likely candidates:

- Asset import pipeline conventions (relevant in Phase 2 asset tools)
- Cloud / package distribution conventions (relevant if we ever distribute sbox-mcp publicly)
- Versioning / SemVer conventions for addons
- Official guidance on plugin packaging best practices
- Confirmation of the `EditorEvent` keys fired in newer engine builds (event names can drift)

When Sprint 1.2 starts, briefly browse https://sbox.game/dev/doc/ for any addon / plugin section; if found, save the relevant pages as PDF / markdown to `plan/research/` for offline reference.

## Open questions

- Whether the docs site is gated for unauthenticated visitors (some Facepunch properties require a Steam-linked account). Check during the manual visit.

## Conclusion

Official docs exist but are not WebFetch-readable. This is **not a blocker** - the source-code research closed every Sprint 1.2 question. Defer the docs visit to Sprint 1.2 implementation if specific gaps emerge.
