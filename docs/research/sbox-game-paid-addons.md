# sbox.game paid-addon support - research brief

**Date:** 2026-04-28 (updated with context7 sbox docs lookup)
**Source:** Facepunch/sbox-public (local clone) + context7 `/websites/sbox_game_dev_doc` (official sbox.game/dev/doc/, 1518 snippets) + WebFetch
**Confidence:** **High** - confirmed against official Facepunch docs.

## Direct answer

**Two monetization paths exist on sbox.game, neither covers our case.** Confirmed via the official FAQ at `https://sbox.game/dev/doc/getting-started/faq` (queried via context7):

> "Creators can monetize their work in s&box through two primary methods: selling cosmetic items for a cut of the revenue via Steam Workshop, or by creating games and receiving a portion of the playfund."

The two paths:
1. **Cosmetic items via Steam Workshop** - avatar clothing only, revenue cut from Steam.
2. **Play Fund (games)** - game creators get a share. Per `https://sbox.game/dev/doc/getting-started/monetization`: "Currently, the Play Fund is only applicable to maps and games. Future updates may allow for more granular distribution, such as sharing revenue with creators of specific assets used in a package."

**For sbox-mcp specifically (a plugin / editor tool / addon):** **no paid path on sbox.game.** Addons are publishable (asset → publish from asset browser → addon page on sbox.game) but pricing isn't a feature. Free distribution only.

My earlier confident claim that sbox.game supports paid addons was wrong. Corrected.

## Evidence

### What DOES exist (cosmetics monetization)

- `engine/Sandbox.Engine/Game/Services/Inventory/` - full Steam Inventory integration:
  - `ItemDefinition.cs` - items have `Price` and `BasePrice` properties (`CurrencyValue`), with `SellStart` / `SellEnd` dates
  - `Inventory.cs` - full checkout flow via Steam (`NativeEngine.SteamInventory.CheckOut()`)
  - `CurrencyValue.cs` - multi-currency support (USD, EUR, GBP, JPY, etc.)
- `game/addons/menu/Code/MenuUI/ItemStore/` - active Item Store UI with shopping cart
- `game/addons/menu/Code/AvatarEditor/UI/Workshop/` - Workshop UI, but **exclusively for avatar / cosmetic packages**, not addons.

### What does NOT exist (addon / plugin monetization)

- No code for paid game projects, paid plugins, or paid editor tools.
- The Workshop Package system shows votes / ratings but **no pricing UI**.
- GitHub search for "sbox paid addon monetize" returns 0 results in the public repo.
- No public discussions, issues, or roadmap items mentioning paid addons.

### Limits of the research

- sbox.game site is Blazor Server-rendered → WebFetch returns skeleton HTML, can't read marketplace UI definitively.
- This is "absence of evidence" rather than "evidence of absence" for the policy side. There may be private agreements or upcoming features not in the public source.

## Pricing model details

For cosmetics (the only working paid path):
- One-time purchase via Steam Inventory.
- Steam takes ~30% per its standard publishing terms (industry standard, not s&box-specific).
- Multi-currency.

For addons / plugins: not applicable - no pricing model exists.

## Content type restrictions

Currently: avatar / cosmetic items can be paid. Everything else (games, addons, plugins, gameplay assets) is free-distribution only.

For sbox-mcp specifically (a C# editor plugin distributed via `mount/{name}/{name}.dll`): falls in the **no-paid-distribution** bucket as of 2026-04-28.

## Implications for sbox-mcp monetization

The monetization landscape differs from the earlier assumption:

| Path | Status |
|---|---|
| sbox.game paid addons | **Not currently available.** Wait-and-see if Facepunch ever opens this. |
| sbox.game free distribution + GitHub Sponsors | **Best near-term path.** Free for users, sponsor button on the public landing repo. |
| sbox.game free distribution + Patreon | Solid alternative, especially with bundled tutorials / dev-log content. |
| Own website + Stripe | Possible but adds infrastructure burden for a small project. |
| Bundled with paid content (Patreon-style "I teach s&box with this tool") | Often the highest-EV route for first-project creators. |

**Recommended near-term posture:** treat sbox-mcp as **free, open-distribution** for v1. Add GitHub Sponsors button when the plugin ships. If sbox.game ever opens paid addon support, revisit.

## Open questions

1. **Future Facepunch policy on paid addons** - no public roadmap. Monitor sbox.game/dev/blog/ and the s&box Discord.
2. **Can we sell sbox-mcp through our own website / Stripe** while distributing the package on sbox.game for free? Likely yes (free addon + paid Pro features unlocked via license key) but needs validation.
3. **Whether the Workshop system is the actual addon distribution channel for plugins** or if there's a separate "tools / extensions" track being built. Source doesn't show one yet.

## What would conclusively answer this

- Direct contact with Facepunch via sbox.game/f/ forums or their Discord.
- Watch sbox.game/dev/blog/ for monetization announcements.
- A test submission of a free addon to confirm the publishing flow exists for non-cosmetic content.

## Context7 library IDs for s&box (use these for future sbox queries)

To prevent stale-claim mistakes like the original "paid addons supported" assertion, future sbox-related questions should query context7 first:

| Library ID | What it is | Snippets |
|---|---|---|
| `/websites/sbox_game_dev_doc` | Official sbox.game/dev/doc/ - getting started, FAQ, project types, monetization, addon publishing | 1518 |
| `/websites/sbox_game_api` | Full s&box API reference - types, methods, attributes | 10075 |
| `/facepunch/sbox-public` | The public engine source repo (subset indexed) | 222 |
| `/websites/facepunch_s_sbox-dev` | Alternate dev docs source | 1142 |
| `/websites/sbox_game` | Main sbox.game site content | 21 |

All marked High source reputation. **Default behavior going forward: query context7 before asserting any sbox fact.**
