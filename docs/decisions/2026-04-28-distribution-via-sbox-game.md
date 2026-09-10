---
title: "Distribution: sbox.game addon marketplace as primary channel"
date: 2026-04-28
status: Accepted
deciders: project owner
---

# Distribution: sbox.game addon marketplace

## Context

Where does sbox-mcp live for end users?

Options considered:

1. **GitHub-only** - clone, `dotnet build`, copy DLL into `mount/`. The standard OSS dev-tool experience.
2. **sbox.game addon marketplace** - Facepunch's official addon publishing platform. One-click install through the s&box client. Supports paid addons.
3. **Both** - open-source on GitHub, packaged on sbox.game.

GitHub-only is what we'd do if we treated sbox-mcp as a generic OSS tool. sbox.game distribution makes the install one-click for s&box developers and unlocks the marketplace's monetization rails for whatever revenue path we eventually pick.

Decision: **route 2 is primary. GitHub source-of-truth stays open MIT, marketplace is the install channel.**

## Decision

**Primary distribution: sbox.game addon marketplace.**

The plugin is published as a proper s&box addon. Users install from inside the s&box client. GitHub remains the open-source repository (MIT) but is not the install path most users follow.

## Reasoning

1. **Audience alignment.** Every s&box developer already knows how to install sbox.game addons. Asking them to `git clone + dotnet build + drop DLL in mount/` is unnecessary friction for a population that has a one-click install habit.
2. **Discovery.** sbox.game's marketplace surfaces addons to users who'd never find a GitHub repo. That's where the s&box-curious live.
3. **Monetization optionality** *(corrected 2026-04-28)*. Initially I claimed sbox.game supports paid addons. Verified via [`research/sbox-game-paid-addons.md`](../research/sbox-game-paid-addons.md): **paid addons are NOT currently available on sbox.game** - only cosmetic items (avatar clothing via Steam Inventory) have a working paid path. Plugin / editor-tool monetization is not supported. Distribution via sbox.game stays correct (free addon channel; one-click install for users); revenue model decouples to GitHub Sponsors / Patreon / own-website-Stripe rather than the marketplace itself.
4. **Native fit.** sbox-mcp IS already an s&box plugin via the `mount/` convention (locked in `2026-04-28-architecture-single-process.md` ADR). The distribution channel just matches the architecture. No re-engineering.
5. **Trust.** Users trust addons from sbox.game more than random GitHub C# DLLs. That matters for adoption.

## Implications

- Plugin must conform to sbox.game addon conventions: manifest format, content layout, version semantics, asset bundling. Research in Sprint 1.2 / 1.3.
- Roadmap gets a new Phase 4 expansion: **Distribution.** Covers addon manifest, sbox.game submission, version bumping process, release notes pipeline.
- Source stays MIT on GitHub. The marketplace addon is a packaged build of the same source. CI may eventually publish releases automatically.
- Versioning: SemVer; tags on GitHub map to addon versions on sbox.game.
- The "Built in public" narrative is now: open MIT source on GitHub + addon you can install from sbox.game. We're not hiding the code, we're hiding the working notes.

## Open questions (research before Phase 4)

- sbox.game's actual addon publishing flow - submission, review, versioning. Likely documented at sbox.game/dev/doc/.
- Whether sbox.game allows MIT-licensed addons. Should be yes; verify terms.
- Future Facepunch policy on paid plugins / editor tools. Currently free-only (verified 2026-04-28 via source-code audit). Monitor sbox.game/dev/blog/ for announcements.
- Asset bundling rules - what we can ship alongside the DLL.

## Revisit If

- sbox.game terms make our use case non-viable.
- sbox.game lacks paid addon support and we want to charge.
- A better distribution channel emerges (unlikely for s&box-specific tooling).

## References

- `docs/decisions/2026-04-28-architecture-single-process.md` - the architecture choice this distribution channel implies.
