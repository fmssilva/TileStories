# The Theme Map: the Lab's look on Unity's tokens

**Now** (the last column of the tables): what the Lab has today. P1b.1 built the table, its test, the looks and knobs, the one
mode and the card's table on `var(--ts-…)`; P1b.2 the profile model, the one bar and the Editor kit (§3, §4); P3 the rest of B's
colours (the theme's ratchet, `tools/theme_ratchet.py`) on W's components; P4 W's screens.

Version 0. **Audited at commit `78ea86f`** ("Let the worker guide drive Lab rounds and brief merge P1a.2 in it"). The P1a.2 builder's
uncommitted work in `src/` (the router, `src/screen/`, the devices, the preview bar's changes) is **not** included: B was read from
`git archive 78ea86f`. W is the frozen World Editor Lab (`proj_guides/_4.1.2_Prototype/lab_src`).

This is the same kind of map as `STATE_MAP.md`, for the look: every `--ts-*` token of Unity's theme, what in the two Labs means it,
what has no token yet, the looks and modes, one profile model and one Editor kit. Merge P1b (`_4.2_Lab_Merge_Plan.md` §3) builds
from it. Names: **B** = this Lab's code (the Block Components Lab), **W** = the World Editor Lab. "(guess)" marks a judgement.

## 0 · The source of truth
- **`TileStories/Assets/Framework/Runtime/Resources/TileStories/Themes/Theme.default.asset`** (a `ThemeAsset`,
  `Framework/Runtime/UI/Theme/ThemeAsset.cs`) is the one source. It holds `shared` values (sizes, radii, space, motion), one token
  set per mode (`light`, `dark`), the `knobs` (a wall's closed choices: today the **accent** knob with 4 options, heritage /
  azulejo / terracotta / garden, light and dark) and the type (`fontLibrary`, display = `fraunces_semibold`, body =
  `atkinson_next_regular` / `_medium` / `_bold`, the display selectors).
- **`Framework/Editor/Theme/ThemeUssGenerator.cs`** writes it to **`Framework/Runtime/UI/Theme/Generated/Theme.default.uss`**
  (pressed from the asset's Inspector, `Editor/Theme/ThemeAssetEditor.cs`): one rule per mode on `.ts-app.ts-theme-default.ts-mode-<mode>`,
  a mode's value winning over a shared one, then the four font tokens on `.ts-app.ts-theme-default`. Every mode must define the same
  colours (the generator refuses otherwise). `TileStoriesTheme.tss` imports it for every panel.
- **`Framework/Tests/Editor/UI/Theme/ThemeUssStaleTests.cs`** fails while the `.uss` differs from what the asset generates now, so
  the `.uss` is a faithful, test-guarded copy of the asset. `ThemeContrastTests` measures the colours **in linear light**.
- C# reads the asset through `ThemeResolver` (never a stylesheet); `ThemeModeRule.Shown(modes, visitorChoice, wallDefault,
  themeDefault)` picks the mode: the visitor's choice, then the wall's default, then the theme's; `ThemeController.SetMode` swaps one
  class on every marked root.
- The knobs are **not generated yet** (the asset's tooltip: `_3.3 P4.5`): they are in the asset only.

**P1b's test reads** the generated `Theme.default.uss` for the 167 tokens (plain `rgb()` / `rgba()` / `px` text, the two modes and
the shared block) and the asset's `knobs:` block for the 4 accent options (YAML floats × 255, rounded as the generator does).

**Token count:** 167 = 163 in both modes (130 colours, 7 shadows, 2 sizes, 10 type sizes, 5 radii, 6 spaces, 1 number, 1 duration,
1 easing; the sizes, radii, spaces and motion are written in both mode rules from `shared`, the same value) + 4 font tokens in the
mode-free rule. Unity renders in **linear** colour; the Lab's CSS blends in sRGB, so a see-through token (glass, scrims) looks lighter
in the Lab than on the phone (guess: by eye, not measured). Contrast checks compose in linear light, as `ThemeContrastTests` does.

## 1 · Tokens

How to read the table:
- Values as Unity writes them (`#RRGGBB @alpha`); "same" = mode-free.
- **B** and **W**: the variables or literals that mean the token, with their uses and the files that use them most. "exact literals"
  are colour literals equal to the token's value (counted by a script over B's `src/` and W's `js/` + `css/`); white, black and
  transparent are never attributed (they match too many tokens). Sizes are counted in B's phone code (the Editor left out).
- `--` = nothing in that Lab means it.

**Summary.** Of the 167 tokens, **127 are matched** by at least one Lab (B 119, W 51), **40 are used by neither** (§1.3).
**29 new tokens are PROPOSED** (§1.2), with 4 new knobs (`look`, `glass`, `roundness`, `density`), 1 new accent option (`cobalt`)
and 1 font (Fredoka) for the ThemeAsset. B writes 1,507 colour literals (560
distinct; 447 distinct, 888 uses, match no token even nearly); W writes 1,119 (450 distinct; 342 distinct, 673 uses, unmatched,
most of them the Playful kit's frame colours and the Editor's own skin). Not one `var(--ts-…)` exists in either Lab today.

| Token | Kind | Light | Dark | B: what means it (uses, files) | W: what means it | Now |
|---|---|---|---|---|---|---|
| `bg` | colour | #F4F0E8 | #100F0D | -- (B has no app page; base.css `--bg` is the Lab page, LAB) | `--page` #F4F0E8 / #100F0D exact (ds.css:574; 4 uses); `--ob-bg` Premium (prem_flows.css, 6) | table done; used 2× |
| `surface` | colour | #FFFFFF | #1C1A17 | `--paper` where it grounds a framed block (`.fr-featured`, blocks.css; 83 uses in all, split with sheet-open); dock `--surfsolid` (9, dock.css) | `--navbg` light #FFFFFF (ds.css:574; 11 uses; dark #26231F = surface-2: drift) | table done; used 2× |
| `surface-2` | colour | #F4F0E8 | #26231F | `--box` #F3EEE6 / #1F1D1A (40: blocks.css 11, controls.css 5, tour-end.css 5); game-ask chip #26231F (dark, exact) | `--navbg` dark #26231F (ds.css:574) | table done; used 40× |
| `scrim` | colour | #000000 @0.7 | #000000 @0.7 | `--scrim` (focus.css, Dim the rest); hero radial shade rgba(0,0,0,.6) (base.css) | `--sdim` sheet backdrop 0.35 (D.ds.sheet.backdrop; 3 uses, ds.css) | table done; used from P3 |
| `scrim-hero` | colour | #0A0806 @0.94 | #0A0806 @0.94 | hero shade (base.css `.hero::after`, rgba(0,0,0,.6) → .25) (near) | `--scrim` 0.85 (D.ds.scrim; 6, ds.css): "the shade under words on a photo" | table done; used from P3 |
| `on-scrim` | colour | #FFFFFF | #FFFFFF | hero title white (literal) | white words on the photo (ds.css) | table done; used from P3 |
| `on-scrim-2` | colour | #F1E9DA | #F1E9DA | -- | eyebrow on the photo (ds.css, exact 3) | table done; used from P3 |
| `hero-glow` | colour | #0A0806 @0.72 | #0A0806 @0.72 | -- | -- | table done; used from P3 |
| `text-halo` | shadow | #000000 @0.85 | #000000 @0.85 | hero words text-shadow (blocks.css halo) | -- | table done; used from P3 |
| `bar` | colour | #FFFFFF | #1C1A17 | `--bar` #FFFFFF / #24221F (3: controls.css, base.css, dock.css) | -- | table done; used 2× |
| `button-glass` | colour | #14110D @0.07 | #FFFFFF @0.12 | white 0.12 discs in dark (dock.css, tour-end.css) | glass button grounds (ds.css) | table done; used from P3 |
| `top-shade` | shadow | #000000 @0.5 | #000000 @0.5 | rgba(0,0,0,.5) (states.css, base.css) | the soft top shade over the camera (tap.css; _4.1.7 §5) | table done; used from P3 |
| `chrome-glass` | colour | #FAF8F4 @0.72 | #1A1B1E @0.72 | -- | -- | table done; used from P3 |
| `chrome-band` | colour | #FAF8F4 @0.94 | #1A1B1E @0.99 | card top band (chrome.js, phone/sheet.css; literals) | -- | table done; used from P3 |
| `top-fade` | colour | #F4F0E8 | #100F0D | card top band Fade (sheet.css; `--paper`) | -- | table done; used from P3 |
| `navbar-glass` | colour | #FAF8F4 @0.72 | #1A1B1E @0.98 | `#navbar.nb-glass` color-mix(--bar 72%) + blur 14 px (controls.css:30) | `.ds-nav.fill-glass` `--navbg: var(--tg)` (ds.css:370) | table done; used from P3 |
| `chrome-glass-hero` | colour | #0A0806 @0.92 | #0A0806 @0.92 | always-dark discs #16181C (camera-ask.css, guide-button.css) (near) | photo-card shade (ds.css, near) | table done; used from P3 |
| `chrome-line-hero` | colour | #FFFFFF @0.32 | #FFFFFF @0.32 | white 0.3 rims (dock.css, demo-hand.css) | white 0.3 rims (tap.css) | table done; used from P3 |
| `glass-ground` | colour | #F4F0E8 @0.62 | #100F0D @0.8 | section glass `.gsec.glasslit` (controls.css:23) | -- | table done; used from P3 |
| `ink` | colour | #14110D | #F6F1E8 | `--ink1` #1A1814 / #F1EEE8 (101: present.css 19, blocks.css 12, controls.css 11); dock `--ink` (30, dock.css 22); `--bt-ink` (18, present.css); `--tlink` (3) | `--tx` = D.themes.text.main #14110D / #F6F1E8 exact (87: ds.css 44, tap.css 12); `--sheet-fg` (89); `--btn-fg` (8) | table done; used 105× |
| `ink-2` | colour | #3D362D | #D9D1C3 | `--ink2` #47423B / #C9C3B8 (115: blocks.css 21, dock.css 19, base.css 14); dock `--ink2`; `--bt-ink2` (9) | `--td` = D.themes.text.soft #38312A / #D9D1C3 (63; light drifts from #3D362D); `--sheet-fg2` (56) | table done; used 88× |
| `ink-3` | colour | #4F473C | #C2B9A8 | dock `--faded` #A39C90 dark (dock.css, teleprompter.css) | -- | table done; used from P3 |
| `on-accent` | colour | #FFFFFF | #1A1206 | `--onacc` (5, dock.css 4); `--bt-onacc` (3) | `--acc-fg` = D.themes.accent_text (36: ds.css 11); `--cta-fg` (5) | table done; used from P3 |
| `accent` | colour | #1E3A8A | #F2B544 | `--accent2` (61: states.css 11, blocks.css 9, base.css 7), set from `ACCENTS[D.card.cardCfg.accent]` (controls.js:9, :114); dock `--acc` (15); `--bt-acc` (10); `--tlacc` (4); exact literals: dark 23× (present.css 9, dock.css 7, highlight.css 6) | `--acc` = D.themes.accent #1E3A8A / #F2B544 exact (138: ds.css 77, tap.css 17); `--cta` (6); ACCENTS (30_tap.js:62) | table done; used 62× |
| `accent-text` | colour | #1E3A8A | #F2B544 | dock `--acctx` (3); `--bt-acctx` (3); gold #FFD98A (38: tour.css, present.css, tour-end.css); exact literals: dark 23× (present.css 9, dock.css 7, highlight.css 6) | `--acc` as text on solid surfaces only (_4.1.7 §3.2) | table done; used from P3 |
| `accent-shine` | colour | #2246AA | #FFF1C8 | `.shine` highlight #9FD1FF (base.css:175) | -- | table done; used from P3 |
| `accent-2` | colour | #C9A227 | #B47828 | -- | -- | table done; used from P3 |
| `line` | colour | #6F665A | #9A9181 | -- | -- | table done; used from P3 |
| `hair` | colour | #D9D2C6 | #3A362F | `--hair` rgba(26,24,20,.14) / rgba(241,238,232,.16) (72: tour-end.css 13, blocks.css 12, base.css 9); dock `--line` (10); `--bt-line` (12) | hairline dividers in lists (ds.css literals) | table done; used 73× |
| `metal-edge` | colour | #433C32 | #CFC6B5 | `.bprev.rimlit` rim (controls.css:24, effects.js) | -- | table done; used from P3 |
| `metal-hi` | colour | #FFFFFF | #4A443B | -- | -- | table done; used from P3 |
| `metal-lo` | colour | #8A8276 | #24211D | -- | -- | table done; used from P3 |
| `metal-groove` | colour | #B0A89B | #000000 @0 | -- | -- | table done; used from P3 |
| `elevation-1` | shadow | #14110D @0.2 | #000000 @0 | box-shadows with black 0.12-0.2 (literals: card-base.css, states.css, blocks.css) | `--depth` 0.35 × D.ds.depth (18: ds.css 17), "floating 0.20" | table done; used 2× |
| `elevation-2` | shadow | #14110D @0.32 | #000000 @0 | `.fr-featured` 0 10px 26px rgba(0,0,0,.18) (blocks.css) | `--depth` "raised 0.32" | table done; used from P3 |
| `status-0` | colour | #2E7D32 | #6FBF73 | -- | markers outline levels (33_markers.js, exact for 0, 2, 3) | table done; used from P3 |
| `status-1` | colour | #7A6200 | #C9B458 | -- | markers outline levels (33_markers.js) | table done; used from P3 |
| `status-2` | colour | #A35400 | #E0913A | -- | markers outline levels (33_markers.js, exact for 0, 2, 3) | table done; used from P3 |
| `status-3` | colour | #A62B1F | #E2735C | -- | markers outline levels (33_markers.js, exact for 0, 2, 3) | table done; used from P3 |
| `status-4` | colour | #6B6B6B | #8E8E8E | -- | markers outline levels (33_markers.js) | table done; used from P3 |
| `family-meta` | colour | #5E574C | #C4BAA8 | -- | -- | table done; used from P3 |
| `family-about` | colour | #7E4206 | #E8A85A | `--about` (2 uses; base.css :88-89 the bar icons); `--fam` the current section (30) | -- | table done; used 2× |
| `family-media` | colour | #1E4E8C | #8DB8EA | `--media` (3 uses; base.css :88-89 the bar icons); `--fam` the current section (30) | -- | table done; used 3× |
| `family-guides` | colour | #245733 | #8FD19E | `--listen (the Listen section: Audio Guide; guess)` (1 uses; base.css :88-89 the bar icons); `--fam` the current section (30) | -- | table done; used 1× |
| `family-stories` | colour | #7A2E5C | #E3A3C8 | `--stories` (7 uses; base.css :88-89 the bar icons); `--fam` the current section (30) | -- | table done; used 8× |
| `family-ar` | colour | #0F5E5A | #7FD1C9 | -- | -- | table done; used from P3 |
| `family-play` | colour | #9A2F1F | #F2A38F | `--play` (4 uses; base.css :88-89 the bar icons); `--fam` the current section (30) | -- | table done; used 4× |
| `family-visit` | colour | #5A4A8A | #C3B5F0 | `--visit` (3 uses; base.css :88-89 the bar icons); `--fam` the current section (30) | -- | table done; used 3× |
| `family-community` | colour | #7A4B00 | #E8C27C | -- | -- | table done; used from P3 |
| `family-navigation` | colour | #4A5A12 | #C4D678 | -- | -- | table done; used from P3 |
| `family-meta-ink` | colour | #FFFFFF | #14110D | -- | -- | table done; used from P3 |
| `family-about-ink` | colour | #FFFFFF | #14110D | white on a filled pill (generic) | -- | table done; used from P3 |
| `family-media-ink` | colour | #FFFFFF | #14110D | white on a filled pill (generic) | -- | table done; used from P3 |
| `family-guides-ink` | colour | #FFFFFF | #14110D | white on a filled pill (generic) | -- | table done; used from P3 |
| `family-stories-ink` | colour | #FFFFFF | #14110D | white on a filled pill (generic) | -- | table done; used from P3 |
| `family-ar-ink` | colour | #FFFFFF | #14110D | -- | -- | table done; used from P3 |
| `family-play-ink` | colour | #FFFFFF | #14110D | white on a filled pill (generic) | -- | table done; used from P3 |
| `family-visit-ink` | colour | #FFFFFF | #14110D | white on a filled pill (generic) | -- | table done; used from P3 |
| `family-community-ink` | colour | #FFFFFF | #14110D | -- | -- | table done; used from P3 |
| `family-navigation-ink` | colour | #FFFFFF | #14110D | -- | -- | table done; used from P3 |
| `family-meta-text` | colour | #38332C | #DBD6CC | -- | -- | table done; used from P3 |
| `family-about-text` | colour | #532902 | #F1D0B3 | `.sec .ey` eyebrow in `--about` (base.css:68) | -- | table done; used from P3 |
| `family-media-text` | colour | #123461 | #C5D8F3 | section eyebrows in `--fam` | -- | table done; used from P3 |
| `family-guides-text` | colour | #163B21 | #B9E0C1 | section eyebrows in `--fam` | -- | table done; used from P3 |
| `family-stories-text` | colour | #581F41 | #EECDDF | section eyebrows in `--fam` | -- | table done; used from P3 |
| `family-ar-text` | colour | #063A37 | #B0DFDA | -- | -- | table done; used from P3 |
| `family-play-text` | colour | #621B10 | #F7CDC4 | section eyebrows in `--fam` | -- | table done; used from P3 |
| `family-visit-text` | colour | #382D59 | #DAD2F5 | section eyebrows in `--fam` | -- | table done; used from P3 |
| `family-community-text` | colour | #4C2D00 | #EED2A7 | -- | -- | table done; used from P3 |
| `family-navigation-text` | colour | #2D3808 | #CEDD96 | -- | -- | table done; used from P3 |
| `sheet-open` | colour | #F8F6F2 | #0F1012 | `--paper` #FBFAF7 / #141312 as the sheet ground (83: states.css 12, blocks.css 10, present.css 10); exact literals: dark 1× (base.css 1) | `--sheet` light #F8F6F2 @0.97 (D.themes.sheet; 35: ds.css 18) (dark #1C1A17 = surface: see PROPOSED panel) | table done; used 83× |
| `locate-teal-hi` | colour | #7FE3C4 | #7FE3C4 | the Locate pin (card/sections.js, states.css, core/assets.js); exact literals: light 6× (sections.js 3, states.css 2, assets.js 1); dark 6× (sections.js 3, states.css 2, assets.js 1) | -- | table done; used from P3 |
| `locate-teal-lo` | colour | #0E9A7C | #0E9A7C | the Locate pin (card/sections.js, states.css, core/assets.js); exact literals: light 6× (sections.js 3, assets.js 2, base.css 1); dark 6× (sections.js 3, assets.js 2, base.css 1) | -- | table done; used from P3 |
| `locate-teal-rim` | colour | #086C56 | #7FE3C4 | the Locate pin (card/sections.js, states.css, core/assets.js); exact literals: dark 6× (sections.js 3, states.css 2, assets.js 1) | -- | table done; used from P3 |
| `locate-green-hi` | colour | #B8EF7A | #B8EF7A | the Locate pin (card/sections.js, states.css, core/assets.js); exact literals: light 1× (sections.js 1); dark 1× (sections.js 1) | -- | table done; used from P3 |
| `locate-green-lo` | colour | #3F9E2C | #3F9E2C | the Locate pin (card/sections.js, states.css, core/assets.js); exact literals: light 1× (sections.js 1); dark 1× (sections.js 1) | -- | table done; used from P3 |
| `locate-green-rim` | colour | #286E1C | #B8EF7A | the Locate pin (card/sections.js, states.css, core/assets.js); exact literals: dark 1× (sections.js 1) | -- | table done; used from P3 |
| `locate-cyan-hi` | colour | #8BE4FF | #8BE4FF | the Locate pin (card/sections.js, states.css, core/assets.js); exact literals: light 1× (sections.js 1); dark 1× (sections.js 1) | -- | table done; used from P3 |
| `locate-cyan-lo` | colour | #0A84B8 | #0A84B8 | the Locate pin (card/sections.js, states.css, core/assets.js); exact literals: light 1× (sections.js 1); dark 1× (sections.js 1) | -- | table done; used from P3 |
| `locate-cyan-rim` | colour | #076A94 | #8BE4FF | the Locate pin (card/sections.js, states.css, core/assets.js); exact literals: dark 1× (sections.js 1) | -- | table done; used from P3 |
| `locate-disc` | colour | #FFFFFF | #FFFFFF | the Locate pin (card/locate.js, sections.js, states.css) | -- | table done; used from P3 |
| `locate-disc-edge` | colour | #E7ECE9 | #E7ECE9 | the Locate pin (card/locate.js, sections.js, states.css) | -- | table done; used from P3 |
| `locate-shadow` | shadow | #000000 @0.28 | #000000 @0.5 | the Locate pin (card/locate.js, sections.js, states.css) | -- | table done; used from P3 |
| `locate-halo` | shadow | #000000 @0.8 | #000000 @0.8 | the Locate pin (card/locate.js, sections.js, states.css) | -- | table done; used from P3 |
| `locate-chip` | colour | #FFFFFF @0.96 | #FFFFFF @0.96 | the Locate pin (card/locate.js, sections.js, states.css) | -- | table done; used from P3 |
| `locate-chip-ink` | colour | #123B32 | #123B32 | the Locate pin (card/locate.js, sections.js, states.css); exact literals: light 1× (sections.js 1); dark 1× (sections.js 1) | -- | table done; used from P3 |
| `locate-glass` | colour | #FFFFFF @0.78 | #FFFFFF @0.78 | the Locate pin (card/locate.js, sections.js, states.css) | -- | table done; used from P3 |
| `locate-glass-line` | colour | #FFFFFF @0.95 | #FFFFFF @0.95 | the Locate pin (card/locate.js, sections.js, states.css) | -- | table done; used from P3 |
| `section-neutral` | colour | #6B6359 | #CFC8BC | -- | -- | table done; used from P3 |
| `family-meta-glass` | colour | #DFDAD0 @0.55 | #282621 @0.7 | -- | -- | table done; used from P3 |
| `family-about-glass` | colour | #F0DAC0 @0.55 | #362614 @0.7 | `--famtint` rgba(fam, .07 / .08) (6: card-base.css 3) (guess) | -- | table done; used from P3 |
| `family-media-glass` | colour | #CED7E5 @0.55 | #1A2332 @0.7 | `--famtint` rgba(fam, .07 / .08) (6: card-base.css 3) (guess) | -- | table done; used from P3 |
| `family-guides-glass` | colour | #CEE0CB @0.55 | #1A2A1D @0.7 | `--famtint` rgba(fam, .07 / .08) (6: card-base.css 3) (guess) | -- | table done; used from P3 |
| `family-stories-glass` | colour | #E4CFDF @0.55 | #2C1C2D @0.7 | `--famtint` rgba(fam, .07 / .08) (6: card-base.css 3) (guess) | -- | table done; used from P3 |
| `family-ar-glass` | colour | #CCE1D8 @0.55 | #182B27 @0.7 | -- | -- | table done; used from P3 |
| `family-play-glass` | colour | #EFD0C4 @0.55 | #351D17 @0.7 | `--famtint` rgba(fam, .07 / .08) (6: card-base.css 3) (guess) | -- | table done; used from P3 |
| `family-visit-glass` | colour | #DAD2E3 @0.55 | #241F30 @0.7 | `--famtint` rgba(fam, .07 / .08) (6: card-base.css 3) (guess) | -- | table done; used from P3 |
| `family-community-glass` | colour | #EDDEC2 @0.55 | #332915 @0.7 | -- | -- | table done; used from P3 |
| `family-navigation-glass` | colour | #DDDEC2 @0.55 | #262915 @0.7 | -- | -- | table done; used from P3 |
| `glass-edge` | colour | #FFFFFF @0.75 | #FFFFFF @0.14 | glass rims rgba(255,255,255,.75 / .14) (dock.css, present.css, panel-look.css) | white rims on glass (ds.css, lab.css exact); the app glass rim is `--tb` (see PROPOSED app-glass-edge) | table done; used from P3 |
| `glass-hi` | colour | #FFFFFF @0.9 | #FFFFFF @0.18 | lit top edges rgba(255,255,255,.9 / .18) (dock.css, present.css) | top highlights (ds.css) | table done; used from P3 |
| `glass-shadow` | shadow | #14110D @0.16 | #000000 @0.35 | rgba(0,0,0,.35) shadows (card-base.css, present.css, hero-start.css) | panel shadows rgba(0,0,0,.35) dark (tap.css, ds.css) | table done; used from P3 |
| `correct` | colour | #17541B | #8FD19E | `--bt-ok` #86DCA5 dark (6, present.css); `--listen` #1F6B3A reused as ok; states.css #8CC99B | -- | table done; used from P3 |
| `wrong` | colour | #8E1B12 | #F2A38F | `--bt-no` #FFA494 dark (3, present.css) | -- | table done; used from P3 |
| `result-fill` | colour | #1E3A8A @0.14 | #F2B544 @0.1 | -- | -- | table done; used from P3 |
| `video-letterbox` | colour | #000000 | #000000 | video bars #000 (generic) | video bars #000 (generic) | table done; used from P3 |
| `clear` | colour | #000000 @0 | #000000 @0 | transparent (generic) | transparent (generic) | table done; used from P3 |
| `family-about-light` | colour | #E28C32 @0.55 | #E28C32 @0.42 | `.gsec.glasslit` color-mix(--fam 22%) (controls.css:23) | -- | table done; used from P3 |
| `family-about-light-2` | colour | #F0C86E @0.45 | #F0C86E @0.3 | `.gsec.glasslit` color-mix(--accent2 14%) (controls.css:23) | -- | table done; used from P3 |
| `family-about-sweep` | colour | #FFFFFF | #F0C86E @0.45 | `.bprev.rimlit` color-mix(--fam 70%, #fff) (controls.css:24) | -- | table done; used from P3 |
| `family-stories-light` | colour | #AA5ABE @0.55 | #AA5ABE @0.42 | `.gsec.glasslit` color-mix(--fam 22%) (controls.css:23) | -- | table done; used from P3 |
| `family-stories-light-2` | colour | #EB8CAA @0.45 | #EB8CAA @0.3 | `.gsec.glasslit` color-mix(--accent2 14%) (controls.css:23) | -- | table done; used from P3 |
| `family-stories-sweep` | colour | #FFFFFF | #EB8CAA @0.45 | `.bprev.rimlit` color-mix(--fam 70%, #fff) (controls.css:24) | -- | table done; used from P3 |
| `family-media-light` | colour | #467DDC @0.55 | #467DDC @0.42 | `.gsec.glasslit` color-mix(--fam 22%) (controls.css:23) | -- | table done; used from P3 |
| `family-media-light-2` | colour | #6EC8E6 @0.45 | #6EC8E6 @0.3 | `.gsec.glasslit` color-mix(--accent2 14%) (controls.css:23) | -- | table done; used from P3 |
| `family-media-sweep` | colour | #FFFFFF | #6EC8E6 @0.45 | `.bprev.rimlit` color-mix(--fam 70%, #fff) (controls.css:24) | -- | table done; used from P3 |
| `family-guides-light` | colour | #46A564 @0.55 | #46A564 @0.42 | `.gsec.glasslit` color-mix(--fam 22%) (controls.css:23) | -- | table done; used from P3 |
| `family-guides-light-2` | colour | #BED26E @0.45 | #BED26E @0.3 | `.gsec.glasslit` color-mix(--accent2 14%) (controls.css:23) | -- | table done; used from P3 |
| `family-guides-sweep` | colour | #FFFFFF | #BED26E @0.45 | `.bprev.rimlit` color-mix(--fam 70%, #fff) (controls.css:24) | -- | table done; used from P3 |
| `family-play-light` | colour | #DE5F46 @0.55 | #DE5F46 @0.42 | `.gsec.glasslit` color-mix(--fam 22%) (controls.css:23) | -- | table done; used from P3 |
| `family-play-light-2` | colour | #F0AA5A @0.45 | #F0AA5A @0.3 | `.gsec.glasslit` color-mix(--accent2 14%) (controls.css:23) | -- | table done; used from P3 |
| `family-play-sweep` | colour | #FFFFFF | #F0AA5A @0.45 | `.bprev.rimlit` color-mix(--fam 70%, #fff) (controls.css:24) | -- | table done; used from P3 |
| `family-visit-light` | colour | #7D69D2 @0.55 | #7D69D2 @0.42 | `.gsec.glasslit` color-mix(--fam 22%) (controls.css:23) | -- | table done; used from P3 |
| `family-visit-light-2` | colour | #B496EB @0.45 | #B496EB @0.3 | `.gsec.glasslit` color-mix(--accent2 14%) (controls.css:23) | -- | table done; used from P3 |
| `family-visit-sweep` | colour | #FFFFFF | #B496EB @0.45 | `.bprev.rimlit` color-mix(--fam 70%, #fff) (controls.css:24) | -- | table done; used from P3 |
| `family-community-light` | colour | #D7A03C @0.55 | #D7A03C @0.42 | -- | -- | table done; used from P3 |
| `family-community-light-2` | colour | #F0CD78 @0.45 | #F0CD78 @0.3 | -- | -- | table done; used from P3 |
| `family-community-sweep` | colour | #FFFFFF | #F0CD78 @0.45 | -- | -- | table done; used from P3 |
| `family-ar-light` | colour | #32AAA0 @0.55 | #32AAA0 @0.42 | -- | -- | table done; used from P3 |
| `family-ar-light-2` | colour | #78D7C8 @0.45 | #78D7C8 @0.3 | -- | -- | table done; used from P3 |
| `family-ar-sweep` | colour | #FFFFFF | #78D7C8 @0.45 | -- | -- | table done; used from P3 |
| `family-meta-light` | colour | #A09178 @0.55 | #A09178 @0.42 | -- | -- | table done; used from P3 |
| `family-meta-light-2` | colour | #CDC3AA @0.45 | #CDC3AA @0.3 | -- | -- | table done; used from P3 |
| `family-meta-sweep` | colour | #FFFFFF | #CDC3AA @0.45 | -- | -- | table done; used from P3 |
| `family-navigation-light` | colour | #96AF3C @0.55 | #96AF3C @0.42 | -- | -- | table done; used from P3 |
| `family-navigation-light-2` | colour | #C8D778 @0.45 | #C8D778 @0.3 | -- | -- | table done; used from P3 |
| `family-navigation-sweep` | colour | #FFFFFF | #C8D778 @0.45 | -- | -- | table done; used from P3 |
| `rim-band` | size | 3px | 1px | `.bprev.rimlit` inset 1.5 px | -- | table done; used from P3 |
| `rim-sweep-arc` | number | 62 | 40 | -- | -- | table done; used from P3 |
| `line-width` | size | 1px | 1px | 1px borders (generic) | 1 pt borders | table done; used 3× |
| `size-body` | type | 16px | 16px | body 16 px (13 uses) and 15 px (50) | Body / Label 16 pt (32_design.js:140) | table done; used 3× |
| `size-body-open` | type | 17px | 17px | 17 px (16) | -- | table done; used from P3 |
| `size-title` | type | 24px | 24px | -- | Title 24 pt | table done; used from P3 |
| `size-caption` | type | 14px | 14px | 14 px (52) | Caption 14 pt | table done; used 1× |
| `size-lede` | type | 19px | 19px | 18.5-19 px (8; audiences.css `data-type=up` 18.5) | -- | table done; used from P3 |
| `size-heading` | type | 20px | 20px | 20 px (7) | Heading 20 pt | table done; used from P3 |
| `size-hero` | type | 40px | 40px | 40 px (2) | -- | table done; used from P3 |
| `size-hero-narrow` | type | 34px | 34px | 34 px (1) | -- | table done; used from P3 |
| `size-dropcap` | type | 48px | 48px | 56 px (2) (near) | -- | table done; used from P3 |
| `size-quote` | type | 22px | 22px | 22 px (7) | -- | table done; used from P3 |
| `radius-s` | radius | 6px | 6px | 6 px (17); 4 px (16), 3 px (26), 2 px (16) off-scale | `--r-s` = corner × 0.3 → 6 (radiiFor, 32_design.js:28) | table done; used from P3 |
| `radius-m` | radius | 12px | 12px | 12 px (46); 10 px (12), 14 px (31) off-scale | `--r-m` = corner × 0.6 → 12 (6 uses) | table done; used 1× |
| `radius-l` | radius | 20px | 20px | 20 px (4); 16 px (16), 18 px (9), 22 px (20), 24 px (9) off-scale | `--r-card` = corner → 20 (16 uses) | table done; used 1× |
| `radius-xl` | radius | 30px | 30px | 30 px (7); 28 px (6) | `--r-sheet` = corner × 1.5 → 30 (8 uses) | table done; used from P3 |
| `radius-round` | radius | 50% | 50% | 50% (84) | 50% (icon discs) | table done; used 1× |
| `space-1` | space | 4px | 4px | 4 px (80) | 4 pt (grid; `--sp` × density) | table done; used 1× |
| `space-2` | space | 8px | 8px | 8 px (127) | 8 pt | table done; used 2× |
| `space-3` | space | 12px | 12px | 12 px (84) | 12 pt | table done; used 6× |
| `space-4` | space | 16px | 16px | 16 px (60) | 16 pt | table done; used 5× |
| `space-5` | space | 24px | 24px | 24 px (4) | 24 pt | table done; used 2× |
| `space-6` | space | 32px | 32px | -- | 32 pt | table done; used from P3 |
| `duration` | motion | 0.22s | 0.22s | .22s (7); .2s (18), .25s (17), .24s (5) near | `--dur` 0.22 s (29, ds.css) | table done; used from P3 |
| `ease` | motion | ease-out-cubic | ease-out-cubic | `--ease` (2, base.css); cubic-bezier(.2,.8,.2,1) literals | `--ease` cubic-bezier(.33,1,.68,1) = ease-out-cubic (5) | table done; used from P3 |
| `font-body` | type | AtkinsonHyperlegibleNext-Regular | same | `--body` Atkinson Hyperlegible Next (228: present.css 48, blocks.css 40, dock.css 25) | `--font` DS_FONT Atkinson (178: ds.css 92) | table done; used 227× |
| `font-display` | type | Fraunces-SemiBold | same | `--display` Fraunces (39: base.css 13) | `--tfont` (64) when Titles = Fraunces | table done; used 34× |
| `font-body-halo` | type | AtkinsonHyperlegibleNext-Regular Halo | same | -- | -- | table done; used from P3 |
| `font-display-halo` | type | Fraunces-SemiBold Halo | same | -- | -- | table done; used from P3 |

### 1.1 · Drifts: the same role, different values
Unity's value wins in every row below (the Lab specifies Unity, `_4.2_Lab_Merge_Plan.md` §1; confirmed by the developer on 2026-11-16, §5 call 6); P1b's test pins it.

| Role | Unity | B | W |
|---|---|---|---|
| Accent heritage, light / dark | #1E3A8A / #F2B544 | #1B4691 / **#8FB0EC (blue in dark)** (`card/controls.js:9`) | #1E3A8A / #F2B544 |
| Accent azulejo | #17457F / #8DB8EA | #13507E / #7DB8E8 | #13507E / #7DB8E8 |
| Accent terracotta | #8E2F14 / #F0A487 | #823114 / #F0A07A | #823114 / #F0A07A |
| Accent garden | #174F33 / #8FD19E | #24532F / #8CC99B | #24532F / #8CC99B |
| Ink | #14110D / #F6F1E8 | #1A1814 / #F1EEE8 (`--ink1`); the dock #F4F1EA; Aurora #F3F5FF | exact |
| Secondary ink | #3D362D / #D9D1C3 | #47423B / #C9C3B8 (`--ink2`); dock #C9C2B6 | #38312A / #D9D1C3 |
| Hairline | #D9D2C6 / #3A362F (solid) | ink @0.14 / paper @0.16 (see-through) | ink @0.12 / white @0.14 |
| Sheet ground | #F8F6F2 / #0F1012 | `--paper` #FBFAF7 / #141312 | `--sheet` #F8F6F2 @0.97 / #1C1A17 @0.98 |
| Wells, chips | #F4F0E8 / #26231F | `--box` #F3EEE6 / #1F1D1A; chips #26231F and #26232B | `--tonal` #E9E3D8 / #302D28 |
| Bar | #FFFFFF / #1C1A17 | `--bar` #FFFFFF / #24221F | nav `--navbg` #FFFFFF / **#26231F** (= surface-2, not surface) |
| Family fills (about) | #7E4206 / #E8A85A | `--about` #7A3E12 / #E3A774 (each family 1-6 steps off) | -- |
| Shade under words on a photo | #0A0806 @0.94 (measured fixed rule) | rgba(0,0,0,.6) radial | 0.85 (Premium), 0.88 (Playful) |

### 1.2 · No `--ts-*` match: verdicts
Every B or W value that means something with no token. **Map** = use the named existing token. **PROPOSED** = a new token for the
ThemeAsset's "additions" list, named in the `--ts-*` scheme, to be approved by the developer (P3's gate; P1b builds them marked
`proposed`). Many come from `_4.1.7_App_Design_System.md` §6 already.

| Value (where) | Uses | Verdict | Now |
|---|---|---|---|
| W `--tg` app glass over the camera: #FBFAF7 @0.78 / #1A1B1E @0.80 (D.themes.glass) | 9 (ds.css 5, tap.css 4) | PROPOSED `--ts-app-glass` | done (table); used by the wall's Explore button |
| W glass Solid: the tint alone at 0.84 / 0.98 (D.ds.glass = solid) | -- | PROPOSED knob `glass` with options `blur` (default) / `solid`, laid over `--ts-app-glass` (a knob, not a token) | done (knob `glass`) |
| W `--gblur` 28 / 24 px | 5 | PROPOSED `--ts-app-glass-blur` (number; Unity: the camera-blur shader's radius, `_4.1.7` §3.2) | done (table); used by the Explore button |
| W `--tb` glass rim: ink @0.12 / white @0.14 | 18 | PROPOSED `--ts-app-glass-edge` (the card's `--ts-glass-edge` stays white @0.75) | done (table); used |
| W `--sheet` panels: #F8F6F2 / #1C1A17 | 35 | PROPOSED `--ts-panel` (`_4.1.7`: = sheet-open light, surface dark) | done (table); the panels from P3 |
| W `--tonal` #E9E3D8 / #302D28 (tonal button) | 11 | PROPOSED `--ts-tonal` | done (table); P3 |
| W notification dot #A1201A + white / #F2B8B5 + #3A0A08 (ds.css) | 2 + 2 | PROPOSED `--ts-notify`, `--ts-on-notify` | done (table); P4 (the notification dot) |
| `_4.1.7` §6's `surface-3` (no value given; no role in either Lab) | 0 | Dropped until a screen needs a third surface | dropped |
| W `--btn` icon (action) buttons: #FFFFFF @0.94 / #1C1A17 @0.94 | 10 | PROPOSED `--ts-action-button` (its ink is `--ts-ink`) | done (table); used by the ✦ button |
| W `--btl` the label pill under an action button, @0.85 | 4 | Map: `--ts-action-button` (one ground; the 0.85 is not worth a token) | P4 (the action labels) |
| W `--tagbg` tags: #FFFFFF @0.94 / #1C1A17 @0.96 | 4 | PROPOSED `--ts-tag` | done (table); P4 |
| W `--lbl` / `--lbl-fg` wall labels: #FAF8F4 @0.94 + ink / #0A0806 @0.92 + white | 3 + 6 | PROPOSED `--ts-wall-label`, `--ts-wall-label-ink` | done (table); P4 (wall labels) |
| W overlay shadow 0.40 (third level) | in `--depth` | PROPOSED `--ts-elevation-3` | done (table); P3 |
| W `--sdim` sheet backdrop 0.35 (Playful 0.45); `_4.1.7` names `--ts-sheet-dim` 0.60 | 3 | Map: `--ts-scrim` (the ground behind anything modal; one scrim, measured) (guess: the developer may prefer 0.60 as its own token) | done: Unity's scrim 0.7 (no sheet-dim) |
| W durations 120 / 360 / 500 ms (`--dur-f`, `--dur-s`, `--dur-x`); B .12-.15 s, .3-.42 s, .5-.7 s | 8; B ~100 | PROPOSED `--ts-duration-fast` 0.12s, `--ts-duration-slow` 0.36s, `--ts-duration-slower` 0.5s; B's 45 different durations map to the nearest of the four | done (table); B's durations P3 |
| W `--spring` expressive cubic-bezier(.34,1.56,.64,1); B cubic-bezier(.2,.8,.2,1) springs | 6; B ~30 | PROPOSED `--ts-ease-emphasis` = `ease-out-back` (UI Toolkit's closed list: a cubic-bezier throws, `ThemeAsset.cs:74`); B's custom curves map to `--ts-ease` | done (table); B's curves P3 |
| W spacing 48 pt (the touch target) | grid | PROPOSED `--ts-space-7` 48px | done (table); used (Explore) |
| W type Display 28 pt | 1 role | PROPOSED `--ts-size-display` 28px | done (table); used (Explore's title) |
| W chip 15 pt; B 15 px (50 uses) | -- | PROPOSED `--ts-size-chip` 15px | done (table); used (buttons) |
| W eyebrow and nav label 13 pt (the 13 pt floor); B 13 px (50) | -- | PROPOSED `--ts-size-eyebrow` 13px | done (table); the sizes this round touched (the card's eyebrows, captions, the bar's labels) moved up to it; the rest on the ratchet (186), P3 |
| W large page title 32 pt; B 32 px (2) | -- | PROPOSED `--ts-size-title-large` 32px | done (table); P4 |
| W label 16 pt / 700 | -- | Map: `--ts-size-body` + bold | done |
| B text under 13 px: 12 (70), 11 (43), 10.5, 12.5, 11.5, 10, 9, 9.5 (165 of 426 phone font sizes) | 165 | Map up to `--ts-size-eyebrow` (13) or `--ts-size-caption` (14): **a fixed-rule break** (minimum text 13 pt, `_4.1.7` §2); P3 lists each | P3 (the ratchet counts 186 phone sizes under 13 px) |
| B 999 px pills (61); W `--r-chip`, `--r-btn` 99 | 61 + 10 | PROPOSED `--ts-radius-pill` 999px (guess: UI Toolkit clamps a radius to half the side; check in Unity) | done (table); used (Explore, the ✦ button) |
| B off-scale radii 2, 3, 4, 10, 14, 16, 18, 22, 24, 28 px (~180 uses) | ~180 | Map to the nearest of s / m / l / xl / pill | P3 |
| W roundness from one value (`corner`, `radiiFor`); `_4.1.7` Sharp 4/8/12/16, Soft 6/12/20/30, Round 8/16/24/32; B `rd-sharp` / `rd-round` (card) and the audiences' `corners` tight / base / round | -- | PROPOSED knob `roundness`: `sharp` / `soft` (default = today's tokens) / `round`, laid over the 4 radii (a knob, not tokens) | done (knob `roundness`; the card's corners key on its classes) |
| W density ×0.75-1.25 (`--sp`); `_4.1.7` Comfortable / Compact | 7 | PROPOSED knob `density`: `comfortable` (default) / `compact` (inner paddings one step down; targets stay 48); W's free multiplier dropped (no calc in USS) | done (knob `density`; Explore reads the spaces) |
| W `--stroke` icon line 1.5 / 2 / 2.25 px | 3 | PROPOSED `--ts-icon-stroke` (number) (guess: Unity draws icons as sprites, so each weight is a sprite set; the token picks the set) | done (table); P3 (icons) |
| W `--outline` (0 Premium, 2 pt Playful) and its ink `--ink` (#1B1446) | 3 + 4 | PROPOSED `--ts-outline-width`, `--ts-outline` | done (table; Playful 2 px); P3 (components) |
| W `--hs` hard shadow #0E2466 / #060A22 (Playful) | 11 | PROPOSED `--ts-shadow-hard` | done (table; Playful); P3 |
| W Playful accent #2638B8 / #FFC83D (pinned, outside the closed list) | 6 | PROPOSED a 5th accent knob option `cobalt` (light #2638B8 / dark #FFC83D, measured like the four) (guess on the name) | done (accent option `cobalt`, Playful picks it) |
| W Fredoka SemiBold (Playful titles, `DS_ROUNDED`) | -- | Addition: a font key in the ThemeAsset's font library; the Playful look swaps `--ts-font-display` to it (no new token) | done (`fredoka_semibold`, proposed; Playful's display face, embedded) |
| B read-along wash `--hla` / `--hlt` (accent @0.24-0.26) and line `--hlln` | 3 | PROPOSED `--ts-highlight` (the wash); the line maps to `--ts-accent` | done (table); the read-along keeps its values on the ratchet, P3 |
| B gold #FFD98A (38: active chips, star, mic, focus ring in dark) and #F2B544 | 38 | Map: `--ts-accent` (dark) | P3 (ratchet) |
| B #1F4E9C (old heritage blue) in audiences.css, guide/focus.css | ~8 | Map: `--ts-accent` (the uses in `mascot/azu2d.js` are the mascot's art, not theme) | P3 (ratchet) |
| B the dock's own terracotta accent #A63F1C / #F0A27E / #9E3B1A / #7A3A1E / #F6DCC9 (dock.css, present.css) | ~30 | Map: `--ts-accent`, `--ts-accent-text`, `--ts-highlight` (a panel never picks its own accent: the closed list holds terracotta) | P3 (ratchet) |
| B Aurora mint #5EF0C8, violet and sky glows, #121833 (panel-look.css) | ~25 | Mint dropped (outside the closed accent list); the glows: see §2.3 (Aurora) | P3 (ratchet; Aurora kept as the dark panels' option) |
| B orange coach badge #E07A1F with white 13 px (tour.css:22) | 11 | Map: `--ts-accent` + `--ts-on-accent` (white on #E07A1F is about 3:1, under 7:1) | P3 (ratchet) |
| B and W always-dark grounds #16181C, #1C1A21, rgba(18,18,20,.88) (captions, ✦ menu, Ask about the view) | ~20 | Map: `--ts-app-glass` for panels over the wall; captions `--ts-chrome-glass-hero` + `--ts-on-scrim` (§2.3) | P3 (ratchet); the ✦ button is on the app's action button now |
| B iOS-style alert and settings sheet #2C2C2E, #1C1C1E, #000, #5AA9FF (guidance/present.css) | ~10 | LAB: it imitates the phone's own permission dialog, which is native in the app; never themed | LAB |
| B and W Editor skin `--u-*` (B `core/base.css:8-11`, W `css/lab.css:7`; same names, `--u-dim` #9A9A9A vs #8E8E8E) and their Editor literals | ~150 | LAB: the Editor's look, never in the token table; one `--u-*` set | LAB |
| W Playful 9-slice frames (`--k-*`, `--kb-*`, `--kt-*` sprites per mode) and `--kq` | ~300 | Not tokens: the Playful look's sprite set (Unity: 9-sliced sprites the look references) | P4 (the Playful kit's sprites) |
| W `--P`, `--u`, `--kq` units; W `--ts` text scale; B `calc()` (36) and `color-mix()` (28) | 3,000+ | Dropped in the port: USS has no `calc()` or `color-mix()` (`_4.1.9d` §124); `applyTokens` writes resolved px and colours | done: `applyTokens` writes resolved values |

### 1.3 · Tokens neither Lab uses (40)
- **`hero-glow`, `chrome-glass`, `accent-2`, `line`, `metal-hi`, `metal-lo`, `metal-groove`, `rim-sweep-arc`, `section-neutral`,
  `result-fill`, `font-body-halo`, `font-display-halo`**: the card's Unity-only parts (the hero's focal button, the metal rim and its
  bands, a neutral section tint, a poll's result bar, the halo font twins). The Lab draws them otherwise or not at all. They stay in the
  table (the test checks their values) and come into use as P3 and P6 move the card onto the tokens.
- **The families `meta`, `ar`, `community`, `navigation`** (28 tokens: fill, ink, text, glass, light, light-2, sweep each): B's card
  has six sections (about, stories, media, listen, play, visit); Unity has ten families. They stay; a Lab section may name them later.

## 2 · Looks and modes

### 2.1 · W: what changes with the look and with the mode
W stores `D.looks[k] = { name, base, tokens, colours: { light, dark } }`; a look keeps only what differs from its base (Premium);
reads resolve base → look → mode → text scale once per profile change (`js/34_profiles.js:1-13`, `view()`, `installViews()`).

| Changes with | What | Onto |
|---|---|---|
| **The mode** (light / dark), every look | colours only: glass, text main / soft, border, accent and its ink, buttons (ground, icon, label), tags, sheet (ground, text, soft), wall labels (`D.themes`, `30_tap.js:43-60`); `--tonal`, `--navbg`, `--page` (`ds.css:298, 574`); shadows: three levels in light, none on panels in dark (`_4.1.7` §3.4) | the `--ts-*` colours of the mode; PROPOSED app glass, panel, tonal, tags, labels, action button |
| **Premium** (the base) | `D.ds` (`32_design.js:9-25`): titles serif, text_size 1, corner 20, density 1, icon_stroke 1.5, selected tint, accent heritage, button 1 / rounded, glass blur, depth 1, motion follow, scheme standard, scrim 0.85, nav (raised, floating, solid, labels always, 4 tabs, large title), sheet (grabber, backdrop 0.35, half) + `LOOK_NEW` (outline 0, soft shadows, display titles, markers by level) | the tokens as Unity has them: Premium = `Theme.default` with no look option |
| **Playful** (`34_profiles.js:96-99`) | tokens: text ×1.0625, corner 24, density 1.1, icon line 2.25, selected fill, action buttons ×1.15, glass solid, scheme expressive, scrim 0.88, nav 3 tabs, sheet backdrop 0.45, outline 2 pt, hard shadows, rounded titles (Fredoka), markers Storybook; colours per mode: glass #FFF8EC / #221D45, text #1B1446 / #FFF8EC, soft #3B3272 / #DCD6F5, border #1B1446 / #0B0820 opaque, accent #2638B8 / #FFC83D, buttons, tags, sheet, labels; the 9-slice kit (`kids_00_kit.css`), `--tonal` / `--navbg` / `--page` (#FFFDF8 / #263070, #F7F2E7 / #10173A) | a knob option `look: playful` = W's shape exactly (`ThemeKnobOption` holds `shared` + per-mode token sets): its sizes (text ×1.0625 precomputed), `roundness: round`, `glass: solid`, `--ts-ease-emphasis` for motion, `--ts-outline-width` 2, `--ts-shadow-hard`, `--ts-icon-stroke` 2.25, the display font swapped, its colours per mode, accent `cobalt`. **Dropped:** scrim 0.88 and backdrop 0.45 (the scrims are measured fixed rules, §1.2); density 1.1 (the knob has comfortable / compact). Nav tabs, selected-chip style, button size and markers are component and app options of the look, not tokens |

### 2.2 · B's ten dark modes
B has no token table; dark is written ten times, switched four ways (`P.dark` → `.cv.dark` / `#ph.dark`; `prefers-color-scheme` /
`data-theme`; `D.guide.pSkin`; the card's `D.card.cardCfg.mode` and the visitor's `P.vis.mode`).

| # | Dark mode (files) | Switch | What it changes | Verdict for the one dark mode | Now |
|---|---|---|---|---|---|
| 1 | **The Lab page** (`core/base.css:13-14`) | the OS / `data-theme`, never `P.dark` | `--bg`, `--ink`, `--muted`, `--accent`, `--rule` of the page around the phone | **Kept, LAB**: the page is not the app; it stays out of the token table | kept (LAB) |
| 2 | **The card's table** `.cv.dark` (`core/base.css:49-50`; card-base.css:73, hero.css:15, states.css:56) | `P.dark` → `cv dark` on the phone and every Editor thumb (`phone/redraw.js:12`) | `--paper` #141312, `--ink1`, `--ink2`, `--hair`, `--box`, `--accent2`, `--fam`, `--famtint`, six section colours, `--bar`, `--teal`; ~27 literals | **Merged:** it is the one switch's core; its values become Unity's dark tokens (§1.1). Its good choice kept: one class on the phone, thumbs drawn in the same mode | done: one class (`ts-mode-dark`) on every marked root, the values Unity's, the table renamed to `var(--ts-…)` |
| 3 | **The card's accent** (`card/controls.js:9, 114-115`, `v8vars`) | `D.card.cardCfg.accent`, a style tag per dark selector | `--accent2` per accent, light / dark | **Merged** into the accent knob with Unity's values; B's blue-in-dark heritage **dropped** (the asset's heritage dark is gold #F2B544) | done: the accent knob, Unity's values (heritage gold in dark) |
| 4 | **The card's mode setting** (`card/controls.js:106-108`, `editor/card-tab.js:87`, `phone/settings.js:12`, `card/controls.js:154`) | `D.card.cardCfg.mode` light / dark / system and `P.vis.mode` light / dark / phone | sets `P.dark`; "system" / "phone" resolve differently in the two paths (`controls.js:108` asks the OS, `card-tab.js:87` does not) | **Merged:** the author's row becomes the wall's Default Mode (`ThemeModeRule`'s wallDefault); the visitor's stays in Settings; one resolver (visitor › wall › theme). The inconsistent "phone" path is gone with it | done: the wall's Default Mode (App tab), the visitor's Settings, one resolver |
| 5 | **The guidance beats** `--bt-*` (`guidance/present.css:46, 100, 147-224, 287`) | `.cv.dark` | per Look (paper #1F1D1A, player #1C1B19, prompter #141312): `--bt-bg`, `--bt-ink` #F4F1EA, `--bt-ink2`, `--bt-acctx` #F0A27E, `--bt-hl`, `--bt-ok` #86DCA5, `--bt-no` #FFA494; `--pgtint`; ~63 literals | **Merged:** the `--bt-*` names duplicate the dock's one for one; both become the mode's tokens (surface, ink, ink-2, accent, highlight, correct, wrong). The iOS-style alert and settings sheet are LAB (§1.2) | switch done (`.ts-mode-dark`); values on the ratchet, P3 |
| 6 | **The dock's Looks** `lk-paper`, `lk-player`, `lk-prompter`, `lk-ink` (`guide/dock.css:118-306`) | `.cv.dark .gdock.lk-*`; the Look is `D.guide.look` | paper: `--surf`, `--surfsolid` #1F1D1A, `--ink`, `--ink2`, `--faded`, `--acctx`, `--hl`, `--chip`, `--line`; player #1C1B19; prompter #141312; **Ink glass is dark in both modes** (#1C1A21, a gradient, gold accent), dark only brightens its rim; ~59 literals | **Merged:** Paper glass, Player sheet and Teleprompter are panel styles reading the mode's tokens (their dark = the mode's dark). **Ink glass kept** as the one always-dark style (like the hero's glass, `--ts-chrome-glass-hero`), its gold = the dark accent. The light Paper's own terracotta accent **dropped** (§1.2) | switch done; values on the ratchet, P3 (W's panels) |
| 7 | **The panel skins** `sk-light`, `sk-aurora`, `sk-dark` (`guide/panel-look.css`, `panel-look.js:16-18`) | its own setting `D.guide.pSkin` (Follows the theme = `P.dark ? aurora : light`) | aurora: ink #F3F5FF, mint accent #5EF0C8, glass #121833 @0.92, violet and sky glows drifting 26-28 s, blur 22 px, a mint rim, measured (`--sksurf`, `--skglow`); dark: #2C2834, no glow; light: lavender #5B5BD6; ~48 literals | **Merged:** the light / dark choice goes (the mode decides: "Follows the theme" is the only behaviour). **Aurora kept** as an option of the dark panels (a "wow": living light), its glows drawn like the card's living glass (two light tokens, PROPOSED `--ts-aurora-light`, `--ts-aurora-light-2`, dark only (guess on the names)) and its accent = the app's accent (mint **dropped**: outside the closed list). Its measured contrast (`--sksurf`, `--skglow`) kept as the test's method | done: the mode decides (Light glass in light; Aurora or Ink glass in dark: The panels in dark mode); values on the ratchet, P3 |
| 8 | **The chips: games, Ask, Tell me more, the tour's end** (`guide/game-ask.css:9-43`, `guide/tell-more.css:11-13`, `guide/tour-end.css:7-74`) | `.cv.dark`, `#ph.dark`, `.dark` | chips #26231F (games) and #26232B (the rest), text #E4DED3 / #F4F1EA, gold #FFD98A active, ok #8FD3A6; inverse "show back" #F4F1EA; ~38 literals | **Merged:** one chip on `--ts-surface-2` (#26231F: the games chip already has it exactly) + `--ts-ink`; active = `--ts-accent`; ok = `--ts-correct`. #26232B **dropped** (a one-off) | switch done; values on the ratchet, P3 (W's chip) |
| 9 | **The read-along highlight** (`guide/highlight.css:20, 25`) | `.cv.dark`, `.dark` | `--hla`, `--hlln` (#FFD98A in one rule, #F2B544 in the next), `--hlt` | **Merged:** PROPOSED `--ts-highlight` + `--ts-accent`; the two golds become one | switch done; values on the ratchet, P3 |
| 10 | **Always-dark surfaces** (`world/camera-ask.css:12`, `agent/agent.css:5-13`, `world/guide-button.css:10, 33`, `guidance/present.css:11`, `guide/tour.css:14`, `guide/controls.css:1`) | none: dark in light mode too | Ask about the view #16181C, the ✦ disc and menu, the sign, the captions rgba(18,18,20,.88), the Stop toast | **Merged:** panels over the wall (Ask about the view, the ✦ menu, the Stop toast) take `--ts-app-glass` and follow the mode (W's rule: light by default, sunlight first); **captions keep always-dark** (a caption convention: `--ts-chrome-glass-hero` + `--ts-on-scrim`); the four near-blacks become those two tokens | the ✦ button done (W's action button); the rest on the ratchet, P3 |

Not a dark mode of the app: the Editor's skin (`--u-*`, always dark like Unity's Editor) is LAB and stays as it is.

### 2.3 · The one dark mode
- **One resolved mode**, `ThemeModeRule` as in Unity: the visitor's choice (phone / light / dark) › the wall's default (light /
  follow the phone / dark, default **light**: rule D1, sunlight first) › the theme's own. The resolved value is computed, not stored.
- **One class on one root** (`ts-mode-dark` beside `ts-app ts-theme-default ts-look-<k>`), as `ThemeController.SetMode` swaps it;
  every module's dark rule keys on it, and no module owns a dark switch or setting.
- **The values are Unity's dark tokens** (warm near-blacks #100F0D / #1C1A17 / #26231F, ink #F6F1E8, gold accent), plus the
  PROPOSED app-glass, panel, tonal and tag values from W.
- **Kept good choices:** the card's thumbs in the same mode (B); Ink glass as an always-dark panel style (B); Aurora's living light
  for the dark panels (B); no shadows on dark panels, depth from the surface tone and a rim (W, `_4.1.7` §3.4); light glass over the
  camera in light mode (W); captions always dark.
- **Dropped:** B's blue heritage in dark, the Aurora mint and the dock's own accent (the closed accent list); `#26232B` and the other
  one-off near-blacks; the per-module light / dark settings (`pSkin` light / dark, the card's mode row as a card setting).

## 3 · The profile model
One model for B's audiences (`audiences/audiences.js`: `P.vis.aud`, `P.vis.len`, `D.wall.aud`) and W's profiles (`js/34_profiles.js`:
`D.profile`, `D.visitors`, `D.time`, `D.looks`): **three audiences** (W's `children` = Family, `adults` = Explorer, plus B's Expert),
**three times** (B's Short / Standard / Deep = W's Quick / Classic / Deep), the look, the mode, text size and reduce motion. The
visitor's choices are `P` (W kept them in `D.profile`: they move); the author's definitions are `D`. "The bar" is the one profile
bar of the Lab (§4): Look · Mode · Audience · Time · Text · Motion · Language.

| Field | Values | Default | Root | Unity home | Set in the Lab by |
|---|---|---|---|---|---|
| Audience | `family` (shown "7-11"), `explorer` ("12+"), `expert` ("12+, Expert") | `explorer` | `P.profile.audience` (was B `P.vis.aud`, W `D.profile.visitors`) | the visitor's profile on the phone: PlayerPrefs (guess) | the bar's Audience; Settings; Who's exploring |
| Age band | 7-11 / 12+ | from the audience | derived, not stored | -- | -- |
| Time | `quick`, `classic`, `deep`; **Expert forces `deep`** (`_4.2_Lab_Merge_Plan.md` §5.1): the control shows Deep, locked, "Expert" | the audience's own time (Family `quick`, Explorer `classic`, Expert `deep`; from B's `looks.<a>.len`) | `P.profile.time` (was B `P.vis.len`, W `D.profile.time`); empty = the audience's | PlayerPrefs (guess) | the bar's Time; Settings |
| Look | `auto`, `premium`, `playful` (auto = the audience's look) | `auto` (Family → Playful; Explorer, Expert → Premium) | `P.profile.look` | PlayerPrefs (guess); the look itself is a ThemeAsset knob option | the bar's Look; Settings; onboarding's Pick a look |
| Mode | `phone`, `light`, `dark` | `phone` = the wall's default | `P.profile.mode` (was B `P.vis.mode` + the resolved `P.dark`, W `D.profile.appearance` + `ui.forceMode`) | PlayerPrefs; resolved by `ThemeModeRule` | the bar's Mode; Settings |
| Wall's default mode | `light`, `follow`, `dark` | `light` | `D.theme.defaultMode` (was B `D.card.cardCfg.mode`, W `D.app.appearance`) | `Theme.default.asset` `defaultMode` + the wall's Default Mode in config.json (guess) | Editor (App › Look and mode) |
| Text size | `phone`, `s` 0.9, `m` 1, `l` 1.2, `xl` 1.36; every text (W), never the paragraphs alone (B's `tx-large` 118 % / `tx-larger` 136 % on `.el-par` only: dropped, labels must grow too) | `phone` (W's `follow_phone_text`; the Lab previews it as `m`) | `P.profile.text` (was B `P.vis.text`, W `D.profile.text`) | PlayerPrefs + the OS font scale (guess); sizes precomputed per step (no `calc()` in USS) | the bar's Text; Settings |
| Reduce motion | `phone`, `reduced` (the phone's setting always wins, `_4.1.7` §2: W's visitor `off` and author `full` **dropped**) | `phone` | `P.profile.motion` (was B `P.vis.motion` + `P.reduce`, W `prefs.motion`) | PlayerPrefs; the OS accessibility setting (guess) | the bar's Motion; Settings |
| Author's motion level | `follow` the phone, `reduced` always | `follow` | `D.theme.motion` (was B `D.card.motionLevel`, W `D.ds.motion`) | ThemeAsset knob `motion level` (named in its tooltip) | Editor (App › Depth and motion) |
| Motion style | `standard`, `expressive` | per audience / look: Family and Playful `expressive`, others `standard` | `D.app.audiences.<a>.motionStyle`, the look's option | ThemeAsset knob option (guess) | Editor |
| Audience definitions | per audience: label, age band, default look, default time, roundness (B `corners` tight / base / round), type step (B `type` up), density (Expert compact), icon line (Family 2), ornaments (B `orn`), the mascot on (W `mascot`) and its life (B `life`), children's text (W `kid_text`), games first (W), coach line (W), Guidance preset (B `gdPreset`), hidden blocks (B `hide`) | Explorer = the base, no deltas | `D.app.audiences.<a>` (was B `D.wall.aud.looks`, `D.wall.aud.hide`, W `D.visitors`) | config.json app settings: AudienceSettings (guess; STATE_MAP's open guess) | Editor (App › Profiles › Audience) |
| Time definitions | minutes, levels at rest, card text layer, deep-dive block, longest tour, pointers (W `D.time`) | Quick 10, Classic 30, Deep 60 min | `D.app.times.<k>` | config.json app settings: TimeSettings (guess) | Editor (App › Profiles › Time) |
| Where Who's exploring is asked | `start`, `card`, `never` (B `D.wall.aud.where`; W `D.app.onboarding.ask_visitors`) | `card` (B's today) | `D.app.onboarding.askAudience` | `AudienceSettings.whereToAsk` (B's comment) | Editor |
| Interests, the visitor's prefs | W `D.profile.interests`, `D.profile.prefs` (minimap, labels, pointers, life, sound, haptics); B `P.vis.feedback` | none; all on | `P.profile.interests`, `P.prefs` | PlayerPrefs (guess) | Settings (P4 for the wall's) |
| Language | `P.lang` (unchanged) | the wall's opening language | `P.lang` | PlayerPrefs | the bar's Language; Settings |

**Rules of the model.** Choices are a sum of dimensions, never their product (W): the theme resolves base → look → mode → knobs
(accent, roundness, density, glass, motion) → text step, once per `profileChanged`. An audience never overrides a value one by one:
it **picks named options** (a roundness, a type step, a motion style; define, assign, place). The per-audience words (`D.words.ov`)
and hidden blocks stay content, not theme. `D.guide.life`'s write from the audience (STATE_MAP's open guess) becomes a read of
`D.app.audiences.<a>.mascotLife`.

## 4 · The Editor kit
B builds HTML strings with data attributes and one delegated listener on the Editor (`src/editor/shell.js`); W builds DOM with
closures bound to `obj[key]` (`js/20_editor_kit.js`). **The kit is B's pattern** (its ~520 rows keep working, and undo comes free from
the one capture point, `core/undo.js`), with W's good widgets ported onto it.

| Widget | B | W | Verdict |
|---|---|---|---|
| Row (label, control, help) | `row()` `shell.js:10` (~520) | `row()` `20_editor_kit.js:49` (~35) | **Merge:** B's row + W's options: indent (both), read-only (W `ro`, 13), a scope tag slot (W) |
| Help ? | `help()`, inline text (row's 3rd arg) | `helpBtn`/`helpText`, central `HELP` table | **Same:** B's inline text (one place per row; app-agnostic text rule unchanged) |
| Segmented | `seg` / `pseg` (~300) | profile bar only | **Same (B):** 2-4 choices and every On / Off |
| Select | `sel` / `psel` (64) | `fSelect` (81) | **Same:** 5 or more choices |
| Slider | `slider` / `pslider`: range + number box (84) | `fRange`: range + output, one undo step per drag (69) | **Merge:** B's (typed value) + W's one step per drag (B's capture point gives it) |
| Toggle | `seg` On / Off; 9 raw checkboxes | `fToggle` checkbox (41) | **Drop W's checkbox** (one boolean widget: On / Off) |
| Text, textarea | `data-text` / `data-field` / per-language cells | `fText` (11) | **Same (B)** |
| Colour | none | `fColor` picker + hex (6) | **Merge, developer-only:** the closed lists are swatch rows (accent, roundness, the looks); a free colour stays a developer-only row with a live 7:1 badge (`_4.1.7` §5) |
| Foldout | `fo` `<details>` (91), `comp` (54), `locked` (9) | `fold` with a lazy body, header `extra` slot (52) | **Merge:** B's `fo` + W's lazy body + a header actions slot |
| Accordion, sticky headers | `accordion.js` (Collapse all, one open, counts, two pinned levels) | `stickHeads`, `grp` bars (`groupify`) | **Merge:** B's accordion on every tab; W's group bars become its sections |
| List element (used by, ▶, move, duplicate, remove) | `mv2` reorder (5) | `listElement` (4), `lifeTable` (11) | **Merge:** one reorderable list (Unity: ReorderableList) with add, remove, duplicate, move, ▶, "used by" |
| Table | `.tbl` (37) | `h("table")` (11) | **Same (B's class)** |
| Badges | `.badge` new / ext / test / lab / nat / bld / bro, LAB ONLY, PROPOSED, EXISTING | `tagChip` AS TODAY / NEW / EXTENDED / MOVED / MERGED / LATER | **Merge:** one `badge(kind)` with the union |
| Planned placeholder | `locked` fold | `planned()` (26) | **Merge:** one planned fold (purpose text + badge) |
| Override marking | -- | scope tag + `resetScoped` ↺ (a look over Premium); `orow` bold + blue bar + ↺ per POI | **Keep** the bold + blue bar + ↺ for a look over Premium only (plan §2); **drop** the per-POI `orow` (a POI picks a named style, P4) |
| Group reset | `data-gdreset` (guidance) | -- | **Same (B)** |
| Profile bar | the preview bar under the phone (dark, reduce, audience, language, devices) | `renderProfileBar` (Look, Mode, Audience, Time, Text) | **Merge:** one bar (§3) that previews on the phone **and** scopes the design rows (they save into its look and mode, W); the device and camera switches stay on the preview bar (P1a.2) |
| Demo | per row: `rowDemos` + the label's ▸ + the ring + the "NO DEMO" self-test | per fold: `fold({demo})` + `runDemo` | **Merge both:** row demos (B) and a fold's demo on opening (W; B's `FOCUS` map is the same idea, made one) |
| Undo / redo | one capture point, counts in the title, Ctrl+Z / Y | `snap()` by hand (54) | **Same (B)**; W's manual `snap()` dropped |
| Search + category chips | `data-q` (2) | -- | **Same (B)** |
| Words / Demo content bar | `ownbar` (17) | -- | **Same (B)** |
| Notes | `.note` / `.warn` (~171) | `.guide` paragraphs | **Merge:** one note (info / warn) |
| Thumbnails | live renders `thumb`, `patThumb`, `cardThumb` | canvas style swatches | **Both kept, in their domains** (card / world) |
| Domain widgets | importance dots, the mix curve, the timeline SVG (guidance) | precision chip (world) | **Stay in their domains**, not the kit |
| Popups, dialogs, toasts | none in either Editor | none | -- (the Editor's decisions stay rows; the kit has none) |
| Editor skin | `--u-*` (`core/base.css:8-11`) | `--u-*` (`css/lab.css:7`) | **Same names:** one set (LAB) |

**The one kit P1b builds** (`src/editor/`): row (indent, read-only, scope slot), help, segmented, select, slider (+ number, one
step per drag), text, the swatch row (closed lists) and the developer-only colour row with its contrast badge, foldout (lazy body,
header actions) inside the accordion, the reorderable list, the table, one badge set, the planned fold, the look-over-Premium mark
(bold + blue bar + ↺) and the group reset, the profile bar, row and fold demos, undo / redo, search, the words bar, notes. Each widget
is one UI Toolkit control in the POI Editor (Foldout, Toggle as a two-button segment, DropdownField, Slider + FloatField,
TextField, ReorderableList / MultiColumnListView, Button).

## 5 · What P1b's builder must decide
1. **How far B's dark rules move in P1b.** Recommended: P1b builds the one switch (one resolved mode, one class, the per-module
   settings removed) and ports the **card's table** (`--paper`, `--ink1`, `--ink2`, `--hair`, `--box`, `--accent2`, the section
   colours, `--bar`: about 520 uses) to `var(--ts-…)` by direct rename (no alias layer: `00-process` "no shims"); the dock, beats,
   skins and chips (`--ink`, `--surf`, `--bt-*`, `--tl*`, `--sk*`) keep their values under a **ratchet** counting non-token colours,
   ported in P3 as the plan lists them. The other way (all of B in P1b) doubles the round.
2. **Where the table lives and how it is written.** Recommended: `D.theme` with the ThemeAsset's shape (`defaultMode`, `shared`,
   `modes`, `knobs`, `type`), its proposed tokens flagged `proposed: true`; `applyTokens(root)` writes the resolved table **as class
   rules** (`.ts-app.ts-theme-default.ts-mode-dark.ts-look-playful …`) in one style element, regenerated on `configChanged`, so a
   mode or look switch swaps one class as `ThemeController.SetMode` does. Inline `setProperty` per root (W's `applyDS`) is the
   simpler other way, but it does not specify Unity.
3. **Looks as knob options.** Recommended: Playful = `ThemeKnobOption { knob: "look", option: "playful" }` (W's base-plus-difference
   is exactly its shape), roundness / density / glass / motion level / text step likewise. The other way is a second ThemeAsset per
   look (duplicates 163 colours).
4. **Text size steps.** Recommended: phone / S 0.9 / M 1 / L 1.2 / XL 1.36 over every text, precomputed. Keeping B's paragraph-only
   steps fails labels and buttons.
5. **The scrims.** Recommended: W's sheet backdrop and dialog scrim map to `--ts-scrim` (0.7, measured); the per-look scrims (0.85,
   0.88, 0.35, 0.45) go. If the developer wants `_4.1.7`'s 0.60 for sheets, it is one PROPOSED token `--ts-sheet-dim`.
6. **The Playful accent.** Recommended: a fifth measured option of the accent knob (`cobalt`, guess) that Playful picks, rather than a
   pinned colour outside the list.
7. **Aurora.** Recommended: keep it as an option of the dark panels with two PROPOSED light tokens and the app's accent; drop the
   mint. The other way (drop Aurora) loses the one "living light" panel the guide has.
8. **"Look" means one thing.** Recommended names: *look* = Premium / Playful only; the dock's Looks become *panel styles*
   (`D.guide.look`), a block's look a *block style* (`k_look`), the audiences' looks *audience options* (`D.wall.aud.looks`).
9. **The kit's file.** Recommended: the widget builders move from `editor/shell.js` into `editor/kit.js` (Unity: the POI Editor's
   controls), `shell.js` keeping renderEditor, the tabs and `setOpt`; the structure check counts a widget built elsewhere.
10. **The test's reach.** Recommended: one Node test reads `Theme.default.uss` and the asset's `knobs:`; every non-proposed token in
    `D.theme` (premium, light and dark, and the 4 accents) equals Unity's; every proposed token is absent from the `.uss` (when Unity
    adds one, the test says so and the flag comes off). Contrast checks compose in linear light.
11. **Linear vs sRGB.** Recommended: the Lab keeps sRGB blending (CSS cannot change it) and says so in the README; the contrast test
    measures in linear like `ThemeContrastTests`, so a token that passes in the Lab passes on the phone.

**Rough size of P1b** (guess): **about 45-55 files**. New: `core/theme.js` (the table, `applyTokens`), `core/profile.js` (the
model), `editor/kit.js`, 3-4 tests (the token test, the profile model, the bar by real clicks, the one dark mode). Touched: the ~20
files with dark rules (§2.2), `core/base.css`, `core/state.js`, `card/controls.js`, `editor/card-tab.js`, `editor/shell.js`,
`editor/components.js`, `audiences/*` (4), `phone/settings.js`, `phone/preview-bar.js`, `phone/redraw.js`, `phone/shell.js`,
`guide/panel-look.js`, `mascot/size.js`, the ~40 files that read the card's variables (the rename of decision 1), and STATE_MAP,
PROTOTYPE_STRUCTURE, README. Two rounds, as the plan says: **P1b.1** the theme (table, test, looks, the one mode, the rename);
**P1b.2** the profile model, the bar and the Editor kit.
