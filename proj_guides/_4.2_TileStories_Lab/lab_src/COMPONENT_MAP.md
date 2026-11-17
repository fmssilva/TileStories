# The Component Map: the visitor's UI on W's components

Version 0. **Audited at commit `7f75c38`** ("Brief the parallel component and voice maps in the worker guide"). B was read from
`git archive 7f75c38`, not from the working tree, so the P1b.1 builder's uncommitted work in `src/` (the token table, the one dark
mode, the rename of the card's table to `var(--ts-…)`, the colour ratchet) is **not** included: every B count below is from
before P1b.1. W is the frozen World Editor Lab (`proj_guides/_4.1.2_Prototype/lab_src`, v20.1). Unity was read on disk
(`TileStories/Assets/Framework/Runtime/UI/`, read only).

This map prepares merge **P3** (`_4.2_Lab_Merge_Plan.md` §3): one component registry, B's card and guide moved onto it, the
literal ratchet at 0 and the reach test. It is the same kind of map as `STATE_MAP.md` and `THEME_MAP.md`, for the components.
Names: **B** = this Lab's code (the Block Components Lab), **W** = the World Editor Lab. "(guess)" marks a judgement.
**PROPOSED** = a new option, component or token for the developer to approve (P3's gate). Paths are under `src/` (B),
`js/` / `css/` (W) and `Framework/Runtime/UI/` (Unity) unless written in full.

## 0 · Summary
- **B draws the same few ideas many times.** The visitor sees about 15 chip classes on the card and about 28 chip-like styles in
  the guide; about 24 button styles on the card and about 30 in the guide (at least 15 different "primary" fills); 6 speech-bubble
  styles plus 3 chat bubbles; 13 + 6 badge and label styles; 8 + 6 toasts, banners and notes; 9 drawings of progress; three copies
  of one palette in the guide (`--ink/--surf/--chip`, `--bt-*`, `--tl*`, plus `--hl*`). The plan's "18 chips, 7 buttons, 5 bubbles"
  undercounts the buttons and the bubbles (§1.4).
- **W has one UI kit** (`js/32_design.js`, `css/ds.css`): 30 App components with options read from `D.ds` (button kinds and sizes,
  the action button, chip, segmented, switch, slider, search, list and row, three panels, sheet, dialog, toast, banner, badge,
  dot, progress, skeleton, empty state, nav bar, top bar, type roles, option cards…), a Premium look and a Playful look made of
  9-slice frames (§1.3).
- **Unity has no generic button, chip, segmented, switch, badge, toast, dialog, bubble or dock** (§3). `Components/` holds the
  magazine elements (text, quote, list, media), the Open Row, the Page Top Bar and the Page Foot. Buttons and chips are written per
  block (`card-quiz__button`, `card-dialogue__button`, `card-story__button`…; 8 chip styles). The component metrics already have a
  home: `Screen/Cards/CardTokens.uss` (159 card-level tokens: `--ts-chip-height`, `--ts-touch-target`, `--ts-grabber-*`,
  `--ts-navbar-*`…), which the THEME_MAP's count of 167 does not include.
- **The verdict (§2):** **21 registry components from W** (each B variant mapped to one, with W's options plus the good B options),
  **16 B parts kept and redrawn on the tokens** (no W part for the role: the Open Row, the talk button, the dock's Mini, captions…),
  **25 PROPOSED options** and **8 PROPOSED tokens**, and a list of what is dropped with its reason.
- **P3's size (§5, guess): 4 rounds, about 100-110 files** (about 22 new, about 75 touched, docs and tests). Its order: the guide's
  three palettes onto `--ts-*`, then the components, then the card's chrome, then the dock and panels.
- **Blocks P3 (§7):** P1b.1 and P1b.2 (the table, the profile model, the bar and the Editor kit); the developer's approval of the
  PROPOSED list; the call on W's metrics versus `CardTokens.uss` (§6, decision 1).

## 1 · Inventory

### 1.1 · B, the card side
Visitor parts only; the Editor (`--u-*`, `src/editor/`) and the Lab's tools are named LAB. B has colour and type variables only
(`--paper --ink1 --ink2 --hair --box --accent2 --fam --famtint --bar`, six section colours, `--display --body --mono`): no radius,
shadow, space, size, z-index or duration variable (about 1,600 px literals in the card's CSS). A variant is chosen four ways: a
class from a `D.*` value; a `#ph` class with `!important` (`rd-sharp`, `tx-large`, `data-corners`); an inline `style=` built in JS
(the Navigation Bar, the Locate pin, the section tint: no stylesheet at all); a regex over an HTML string (`card/elements.js:91-104`,
`card/block-looks.js:6-9`).

| Family | Files | Variants (count) | Picked by |
|---|---|---|---|
| Card sheet | `phone/sheet.js:3`, `card/controls.js:113`, `phone/sheet.css:12`, `card/card-base.css:6-16` | 4 stops (closed, peek 64, half, full), landscape side sheet 360 px, wide container | `D.card.cardCfg.openAt / halfMax / fullHeight (top, strip) / tapOutside / backBtn` |
| Grabber | `phone/sheet.css:13-14`, `phone/grabber.css:3-4`, `phone/grabber.js:14` | 1 (36×5 white bar in a 96×32 hit area) + the nudge cue | Guidance's cue |
| Peek row | `card/controls.css:4-5`, `card/hero.css:6` | 1 | -- |
| Top band | `phone/sheet.css:16-21`, `card/chrome.js:2-4` | 3 CSS (fade, glass, clear), 2 offered | `D.card.cardCfg.topBand` |
| Hero (Header block) | `core/base.css:59-64`, `card/hero.js`, `card/blocks.js:28-32`, `card/blocks.css:34-40` | look 7, words 3 (halo, scrim, halo only), motion 4, zoom 3, crop | per block `k_look`, `words`, `motion` |
| Navigation Bar | `card/sections.js:42-51` (inline), `core/base.css:85-89`, `card/controls.css:29-31` | indicator 4, chosen icon 2, chosen colour 3, icon colour 3, labels 4, shape 2, fill 3, position 2, show at / from, more 4-6; 28 section icons | `D.card.ctl.bar.*` |
| Close / settings discs | `core/base.css:65-66`, `phone/grabber.css:2`, `card/chrome-pinned.js:3` | 1 (34 px dark translucent) + the top band's box disc | `D.card.ctl.top.*` |
| Sticky action | `phone/sheet.css:22-23`, `card/controls.css:32` | 1 (+ 2 near copies: `.asticky`, `.arfocal`) | `D.card.ctl.sticky`, `.stickySlide` |
| Page top bar, crumb | `core/base.css:92-94`, `phone/shell.js:4`, `card/chrome.js:9-10` | crumb pages / off; per block chrome override | `D.card.ctl.crumb` |
| Page foot (Previous / Next) | `phone/sheet.css:34-37`, `card/chrome.js:11` | 1 | `D.card.ctl.prevnext` |
| Card layouts | `phone/sheet.css:26-33`, `card/controls.css:18-19`, `card/sections.js:77-87` | scroll / tabs `.lt-tabs` / pages `.lt-pages` + `.pgedge` / accordion `.lt-acc` | `D.card.cardCfg.layout` |
| Next up, page chips, chapters | `card/controls.css:15-17`, `card/blocks.css:41-42` | 1 each | `D.card.secCfg.nextUp`, `D.card.ctl.pageChips` |
| Locate pin + label | `card/sections.js:55-59` (inline), `card/locate.js`, `phone/grabber.css:8-11` | style 3 (pin, pin + label, glass) × colour 4 × size, float, fold | `D.card.ctl.locate.*` |
| Open Row | `card/card-base.css:31-33`, `core/base.css:175` | plain / nudge / shine (+ per block inherit) | `D.card.ctl.openrow`, block `orow` |
| Settings sheet | `card/controls.css:6-11`, `phone/settings.js:9-15`, `audiences/audiences.js:61-65` | 5 groups + audience + length; options as `.vo button` pills | `P.vis.*` |
| Contents sheet | `card/controls.css:6-14`, `card/contents.js` | rows sections / every, ticks, filter chips, row kinds 3 | `D.card.ctl.contents.*` |
| Term popup / page | `editor/texts.css:21-29` (visitor CSS in an Editor file), `card/controls.css:33`, `card/terms.js:19-29` | popup / under; underline first / every | `D.card.ctl.term.*` |
| Picture viewer | `card/pages.js:20-22`, `card/blocks.css:47-49` | counter chips / count / none, zoom | `D.card.ctl.viewer.counter`; hard-coded dark |
| Block pages (kpage) | `card/pages.js:6-28` | 10 page kinds (Timeline, Dialogue, Video, 3D, 360, Knowledge Check, Hotspot, Today Map…) | the block |
| Chips | see §1.4 | 15 visitor classes | various |
| Buttons | see §1.4 | 41 rules, about 24 styles | mostly fixed |
| Badges and labels | `card/elements.css:13`, `audiences/lenses.css:11`, `card/blocks.css:31-44`, `card/card-base.css:55-63`, `core/base.css:148-169` | 13 (7 of them the same dark label over a picture, `rgba(0,0,0,.55-.78)`) | fixed |
| Toasts, banners, tips | `card/states.css`, `card/hints.css:3`, `phone/sheet.css:15` | 8 (`.st-toast .st-banner .st-bubble .st-pill .st-slow .st-card .hinttip .peeknext`) | `D.card.states.*` |
| Cues | `card/hints.js`, `card/hints.css` | 6 (grabber nudge, Locate label, bar ring + tip, Before/After sweep, strip slide, term glow) | Guidance |
| States | `card/states.js`, `card/states.css` | loading 5 looks, lost 3, empty 2, offline 2, done 3 = 15 | `D.card.states.*` |
| Section opener | `card/card-base.css:23`, `card/sections.js:27-38`, `card/effects.js:5-25`, `card/controls.css:21-24` | start 7 + 9 SVG edges + own strip; 6 line dividers; colour section / accent / neutral; tint | `D.card.secCfg.*` |
| Block frame and section look | `card/card-base.css:26-34`, `card/blocks.css:33`, `card/controls.css:23-24`, `card/block-looks.js` | section look open / cards (glass `.glasslit`, rim `.rimlit`); frame normal / featured / quiet | `D.card.secCfg.look / glass / rim`, block `frame` |
| Elements | `card/elements.js:10-44`, `core/base.css:110-158`, `card/elements.css` | 24 kinds, 49 named styles, 10 patterns; tone 4, size 3, align, width, frame, carousel 5 layouts | `D.blocks.styles`, the row |
| Block widgets | `card/card-base.css:35-109` | 17 (`.audio .kq .info .ba .hs .turn .pano .anim .map .tl .sw .flip .hold .deck .scrub .tilt .choice`), each with its own radius, shadow and label | the block |
| Tiles | `guide/tour-end.css:34-35`, `screen/screen.css:14-17`, `card/blocks.css:4,30` | point card `.te-card` (also Explore's `.xp-row`), timeline / related tiles | -- |
| Who's exploring | `audiences/audiences.css:12-23`, `audiences/audiences.js:57-59` | 3 choices + Skip | `D.wall.aud.where` |
| See it as | `audiences/lenses.css:7-11`, `audiences/lenses.js:13-19` | chip row + badge on the chosen topic | `D.card.lensCfg.*`, `P.lens` |
| Audience looks | `audiences/audiences.css:3-10` | `data-type` up, `data-orn` stickers / off, `data-corners` round / tight | `D.wall.aud.looks.<a>` |
| Explore | `screen/explore.js`, `screen/screen.css:10-19` | 1 (header, rows, the wall's dark "Explore" pill) | -- (W's Explore in P4) |

Things that ignore B's dark switch today: the viewer, the video bar and captions, the over-picture labels, the audience stickers,
`.kq .ok / .no`, the "done" green. Likely dead (guess): `.prev` (`core/base.css:79-84`) and `.sec .ey` (`core/base.css:67-69`).

### 1.2 · B, the guide side
Three palettes for one set of roles: the dock's (`--ink --ink2 --faded --acc --onacc --acctx --hl --chip --line --surf --surfsolid`…,
`guide/dock.css:5-6, 116-219`), the beats' (`--bt-*`, `guidance/present.css:206-224`, the same hex values under other names), the
teleprompter's (`--tl*`, `guide/teleprompter.css:6-7`), and the highlighter's `--hl*` (`guide/highlight.css`). The dark hook is
spelled three ways (`.dark`, `.cv.dark`, `#ph.dark`). About 455 hex and 370 `rgba()` in the guide's CSS; two golds for one role
(`#FFD98A` ×34, `#F2B544` ×23); the "guide dark" `#16181C` / `#F4F1EA` / `#FFD98A` as literals in 6 files.

| Family | Files | Variants (count) | Picked by |
|---|---|---|---|
| Guide Dock | `guide/dock.js:112-150`, `guide/dock.css` (341 lines) | stops 4 (compact, mini, peek = Open, full); Looks 4 (paper glass, ink glass, player sheet, teleprompter) × light / dark = 8 palettes, each with its own Mini shape and Open layout | `D.guide.look`, `dockStart`, `miniCtl`, `pWords`, `pBars`, `bubbles`, `talk`, `chips` |
| Panel skins | `guide/panel-look.css`, `guide/panel-look.js:15-18` | light glass, aurora, dark, follows the theme; they reach only Ink glass and the teleprompter strip | `D.guide.pSkin` |
| Talk button | `guide/dock.css:81-91` | 6 states (idle, listening, thinking, speaking, interrupted, not heard) × 4 Looks × 2 modes × 2 skins | `D.guide.talk` |
| Dock controls | `guide/dock.css:20-76, 110-113, 311, 339` | prev / next, ±10 s, play, stop, speed chip, Aa, Read all, keyboard, send, hide chevron, grabber | -- |
| Thread (whole text) | `guide/dock.css:100-108` | 3 bubbles (Azu, me, note) + typing | -- |
| Teleprompter strip | `guide/teleprompter.css:6-43` | 5 states; skins 3 | `D.guide.tourWords`, `teleH` |
| Classic panel | `guide/tour.css:14-36`, `guide/readalong.css:3` | captions bar `.gcap`, control bar `.gbar`, ask panel `.gask` | `D.guide.panel = classic` |
| Bubbles | `guide/dock.css:270-278`, `guidance/present.css:5-52`, `mascot/azu2d.css:39-40`, `guide/tour-end.css:51-74` | 6 (`.gbubble`, `.gdbub`, `.azbub`, `.tebub`, `.teinv button`, `.gcap`) with 5 radii and tail shapes | per item `goes`, `tail` |
| Chips | see §1.4 | about 28 chip-like styles, 10 class names with "chip" | various |
| Buttons | see §1.4 | 42 selectors, about 30 styles in 14 families | various |
| Beats | `guidance/present.css:204-319`, `guidance/present.js:470` | 4 Looks × light / dark = 8 skins; 6 kinds (promise, quiz finale, guess the year, don't say it, find it on the wall, breather); the tail strip `.gdstrip` | `gdBeatLook()` = the dock's Look |
| Pages (first launch) | `guidance/present.css:99-164` | 4 pages; the iOS alert and Settings stand-ins are LAB | `D.guidance.items` |
| Things to try | `guidance/present.css:168-183` | 1 (+ done rows) | `D.guidance.tryMenu` |
| Tour end | `guide/tour-end.css:5-91` | panel / pill / invitation; sections head, questions, ask, nearby, actions | `D.guide.teOn / teStart / teSec…` |
| Toasts | `guide/controls.css:3-18`, `guidance/present.css:84, 188-189` | 1 family, 3 placements (stop toast, stop question, tips toast) + 3 notes (`.ga-note`, `.tl-msg`, `.gdpg-standin`) | `D.guide.stopUndo` |
| Badges, counters | `guide/tour.css:22`, `guide/tour-end.css:13-19`, `guidance/present.css:261-327`, `guide/dock.css:296-306` | about 6 (orange coach `#E07A1F`, score star, ok tick, pip, quiz star, big numbers) | -- |
| Progress | `guidance/present.css:104-107, 174, 238`, `guide/tour.css:17`, `guide/dock.css:15, 54-58, 150-153, 184`, `world/guide-button.css:21-23`, `guide/game-ask.css:33` | 9 drawings (page dots, step segments, seek dots / segments / hairline per Look, bar, ring, ✦ ring and bar, try bar, countdown ×2) | per Look |
| Guide overlays | `guide/focus.css`, `guide/demo-hand.css`, `guidance/present.css:28-79`, `guide/highlight.css`, `guide/tour-scroll.css` | spotlight, focus ring (orange), told bar, Azu's arm, two ghost fingers, ring and glint, 4 highlight recipes, scroll cue | `D.guide.cue / scrim / zoom / hand / hl` |
| Game layer | `guide/game-ask.css` | paused chip, back row, countdown, back-to-tour pill, note | `D.guide.ga*` |
| Guide button ✦ | `world/guide-button.css:11-44` | icon star / face / word, tour ring and bar, pause, Azu beside, menu, the band behind it | `D.guide.gbtn / gbIcon / gbSide / gbFold` |
| Ask about the view | `world/camera-ask.css:3-21`, `world/camera-ask.js:24` | panel, pins (central, lit, dim), a blue-gradient mic pill; it reuses the dock's chips outside the dock (their colours fall back, guess) | `D.wall.camCfg.*` |
| Agent's visitor parts | `agent/agent.css:2-15` | links, offered actions, sources (the rest of `agent.css` is LAB) | `D.agent` |

### 1.3 · W, the App components
Every size is in pt (`--P`); the "v16 final tokens" block (`css/ds.css:271-440`) and the v20 blocks (`ds.css:854+`) override the
older rules above them and are the spec. Colours come from `applyGlass` (`js/30_tap.js:74`), sizes and motion from `applyDS`
(`js/32_design.js:42`). Premium is the base; Playful (`js/34_profiles.js:93-99`) swaps tokens and adds 9-slice frames
(`css/kids_00_kit.css:53-98`: `--k-*` sprites, `--kb-*` bundles, pressed = the flat frame moved by `--kt-*`), Fredoka titles,
2 pt outlines and hard shadows. Premium refinements are scoped `.tapc:not(.look-playful)` in `css/prem_*.css`.

| Component | Builder, classes | Options (count) | Used on |
|---|---|---|---|
| Button | `dsBtn` `32_design.js:61`; `ds.css:289-300, 666-675`; `prem_foundations.css:5-17`; `kids_10_controls.css:18-61` | kind 5 (main, tonal, glass, outline, text; text soft), size 3 (S 40, M 48, L 56), icon, states pressed / disabled (lock) / focus ring | Start, Onboarding, Coming soon, kit |
| Action (icon) button | `dsIBtn` `32_design.js:63`; `.a-rad .c`, `.tib .i`, `.irow .i`, `.ii .i`; `tap.css:54-125`; `ds.css:546-588` | size ×0.85-1.25, shape rounded / circle, surface glass / solid, states normal / pressed / selected (✓) / disabled / more (+N), label pill under it; 13 action types | On tap, kit |
| Close / back | `.ds-x` (32 pt in a 48 target, `ds.css:38, 307`), `.ds-back` | -- | sheets, top bar |
| Chip | `dsChip` `32_design.js:64`; `.lchip`; `ds.css:309-311, 595-599, 768` | selected tint (wash + 1.5 pt ring + bold) / fill; ✓ in Premium; dot; icon; 32 visual in 48 | Main AR lens row, Onboarding, Settings |
| Chip row | `.mlens` `31_main.js:66-68`, `tap.css:204` | at filter / search / bottom / off; edge fade | Main AR |
| Segmented | `dsSeg` `32_design.js:65`; `ds.css:314-316, 600-603, 750-754` | 2-5 segments; raised thumb, or accent + ✓ with `sel-fill` | Settings (×8), Onboarding language |
| Switch | `dsSwitch` `32_design.js:66`; `ds.css:317-319, 634-635` | 52×32, thumb 24 | Settings (×6) |
| Slider | `.ds-range` `ds.css:320-322` | track 6, thumb 28 | kit only |
| Search | `.msearch`, `.mgear` `31_main.js:63`; `ds.css:312, 989-995` | at top / bottom / off, pill / bar, voice, filter, gear | Main AR |
| List and row | `dsList`, `dsRow` `32_design.js:67-71`; `ds.css:324-329` | leading icon tile / dot, title, line (56 / 72), trailing chevron / switch / badge | Explore, Settings, You |
| Panel | `dsPanel` `.ds-panel.g`, `.ds-solid`, `dsPhoto` `32_design.js:72-76`; `ds.css:302-306, 359-362` | glass / solid / photo card (fixed shade); Playful paper frame, polaroid | kit, On tap |
| Sheet | inline `.ds-sheet`, `.ds-dim`, `.grab`, `.khd`, `.sfoot`; `ds.css:338-354` | stops peek 57 / half 40 % / full −48; grabber on / off; backdrop; detent; landscape column | Explore, Settings, You, album |
| Dialog | inline `.ds-dialog`, `.ds-scrim`; `ds.css:347-353` | 342 wide, title 20 / 700, actions 48 | kit only |
| Toast, banner | `.toast.g` (`tap.css:104`), `.ktoast`, `.ds-banner` (`ds.css:333-335`) | toast: message + icon; banner: icon + text + one text button | On tap, Onboarding |
| Badge, dot | `dsBadge` `32_design.js:73`, `.ds-dot`; `ds.css:330-332` | accent wash + rim / soft; dot 20 | kit only |
| Progress, skeleton | `.ds-prog`, `.ds-skel`, Start's `.ds-tiles`; `ds.css:80-83, 251-257, 336` | bar 6 pt, shimmer (Premium: breathing) | Start, kit |
| Empty state | `dsEmpty` `32_design.js:77` | icon disc 64 + title + text + CTA; Playful shows Azu | Navigation › Saved |
| Nav bar | `dsNavBar` `32_design.js:85-94`; `ds.css:366-443` | style 4 (raised, pill, underline, none), shape 3 (floating, dock, full bar), fill 2, labels 3, tabs 3-5, title 2, rail in landscape | kit only |
| Top bar | inline `.ds-top`, `.ds-back`, `.ds-x`; `ds.css:121-128, 406-408` | title large / inline | kit |
| Type roles | `ds.css:272-286` | display 28, title 24, heading 20, body 16, label 16 / 700, chip 15, caption 14, eyebrow 13 caps | everywhere |
| Coach tip | `.mcoach`, `.mtip`, `.mhint` `31_main.js:99-107`; `tap.css:228-259` | tail up / down, with Azu | Main AR |
| Option card | inline `.ds-opt` `34_profiles.js:179`; `ds.css:705-937` | look, audience, time; selected ring + ✓ / accent fill | Onboarding |
| Tag, label pill | `.t-label`, `.t-chip`, `.t-card` `30_tap.js:85`; `.mk .lbl` | 5 tag components; label pills `--btl`, `--tagbg`, `--lbl` (about 7 variants) | On tap, the wall |
| Action bar, mini-map, Near | `31_main.js:73-111` | 2-5 buttons, Azu as mascot / button / off, rail | Main AR (P4) |

### 1.4 · The counts
| What | B card | B guide | Plan's guess | Method |
|---|---|---|---|---|
| Chip-like classes | 15 (+3 LAB) | about 28 styles, 10 names with "chip" | about 18 | rules with a 999 px radius or a label pill, read and grouped (the grouping is a judgement) |
| Button styles | 41 rules, about 24 styles | 42 selectors, about 30 styles, 14 families | about 7 | rules with `appearance:none` / `:where(button)`, grouped by fill × shape × size |
| Speech bubbles | 2 (`.st-bubble`, `.hinttip`) | 6 + 3 chat | about 5 | rules with a fill, a 12-18 px radius and a tail, checked in JS |
| Badges, labels | 13 | about 6 | -- | read |
| Toasts, banners, notes | 8 | 3 + 3 | -- | read |
| Progress drawings | 4 (dots, count, ring, segments) | 9 | -- | read |
| Primary fills | 7 accent buttons | at least 8 | -- | read |

Colour literals at `7f75c38` in the visitor CSS (the Editor's files left out): about 730 hex and 460 `rgba()`; about 3,500 px values;
about 250 durations; 61 JS files write inline styles (some of them Editor). P1b.1 sets the ratchet's starting number.

## 2 · Verdicts

**How they were made** (`_4.2_Lab_Merge_Plan.md` §5, call 7; `00-process` "Framework Completeness"): each B variant maps to one W
component; W's look and options win; a good B option the W component lacks is merged in as an option (PROPOSED when it is new to
W); a B variant that only differs in padding, radius or colour is dropped (its values become tokens); where W has nothing for the
role, B's part is kept and **redrawn on the tokens**. Every component reads only `--ts-*` tokens (the theme and the card-level
metrics, decision 1) and never names its host (Unity's `UiRenderTargetGuardTests` rule).

### 2.1 · The registry: W's components with their merged options

| # | Component | From W | Options after the merge (W's, then **B's merged**, then PROPOSED) | B variants it absorbs | Dropped (one-line reason) |
|---|---|---|---|---|---|
| 1 | **Button** | `dsBtn` | kind main / tonal / glass / outline / text (+ text soft); size S / M / L; icon; pressed / disabled / focus. **PROPOSED:** `width: fit / full` (the sticky action, the pages' buttons, the stop banner's Continue); `state: done` (B's `.st-morph`: the label becomes ✓ and the done words, `--ts-correct`) | card: `.sticky button`, `.asticky button`, `.arfocal`, `.nextup button`, `.collect button`, `.st-morph`, `.resume button`, `.st-btn`, `.apills` (tonal S; `.focal` = main), `.hlisten` (glass S, over the photo), `.xp-open` (glass), `.gpop button` and `.onb-skip` (text / text soft), `.ubtn2` (tonal). Guide: `.gstart` (glass L over the hero), `.gcont` / `.gagain` (glass / text), `.gresume`, `.gtoast` and `.gstopask` buttons (main / outline / text), `.gdpg-b.pri / .sec` (main L / tonal L), `.gdpg-skip`, `.gdshowback` and `.ga-showback` (one: glass S with a back icon), `.ga-btn(.pri)`, `.te-acts` + `.te-cont`, `.gdcta` (tonal S), `.gdfewer` (text S), `.vagain`, `.vtry`, `.tl-full`, `.gdtry-show` (text), `.bt-chip.pri` (main S) | `.arfocal`'s extra accent ring (the fill already marks the one focal action, D5); accent outlines (accent text sits only on solid surfaces; W's outline is ink); the gold `#FFD98A` and navy `#14213D` fills (closed accent list); 15 private paddings and radii (size tokens) |
| 2 | **Icon button** | `dsIBtn` + `.ds-x` + `.ds-back` | size S (32) / M (44) / L (52) × `D.ds.button.size`, always a 48 target; shape rounded / circle; surface glass / solid; states pressed / selected (✓) / disabled / more (+N); label under it. **PROPOSED:** surface `hero` (over a photo: `--ts-chrome-glass-hero` + `--ts-on-scrim`, as Unity's `poi-card__mark-disc`); kind `main` (an accent disc: the one Play in view) | card: `.chrome i`, `.topband i`, `.topbar button`, `.lt-pages button`, `.vh button`, the viewer's `.vtop > button` (hero), `.xp-wall`, `.acirc i` (the Actions block's circles = W's action button with its label), `.carr`, `.audio button` (main). Guide: `.gd-pn .gd-skip .gd-mback .gd-mnext .gd-stopb .gd-mstop .gd-openb .gd-mc`, `.gd-play` / `.gd-mpp` (main), `.gd-kb`, `.gd-send`, `.te-chev .te-x .te-mic .te-po`, `.gdtry-x`, `.gdx`, `.ca-x`, `.ca-pill` (main), `.bt-mic`, `.gb-pause` | the blue gradient of `.ca-pill` and the gradients of the skins' Play (no gradients in USS, and outside the accent list); 11 disc sizes (three sizes) |
| 3 | **Chip** | `dsChip` | selected style tint / fill (`D.ds.selected`); ✓ when selected; dot; icon; 32 visual in a 48 target, 15 pt. **PROPOSED:** `kind: filter / suggestion` (a suggestion acts on a tap and has no selected state: questions, answers, Tell me more; W has only filter chips); `value` (a suggestion that shows its value, for the speed chip) | card: `.lensbar button`, `.pchips button`, `.cchips button` (filter); `.achap button`, `.peeknext`, `.aspeed` (suggestion). Guide: `.gd-chips [data-chip]` and `.ca-chips` (suggestion), `.gd-speed` (suggestion with value), `.gd-aa`, `.gd-all` (filter on / off), `.bt-chip`, `.gdstrip .bt-chip` (suggestion), `.gtmore` (suggestion; "more coming" = selected), `.te-q`, `.agchip` (suggestion) | the per-panel chip palettes (`--chip`, `--bt-chip`, `#26232B`); 9 padding and size combinations; 7 selected encodings (W's two stay); 12-13 px chip text (the 13 pt floor; chip 15 pt) |
| 4 | **Chip row** | `.mlens` | horizontal scroll, edge fade, place. **B's merged:** wraps or scrolls (B's question chips wrap; guess) | `.lensbar`, `.pchips`, `.cchips`, `.achap`, `.lt-tabs` (the tabs layout = a filter row with one selected), `.gd-chips`, `.bt-chips`, `.te-qs` | -- |
| 5 | **Segmented** | `dsSeg` | 2-5 segments; raised thumb or accent fill + ✓ (`sel-fill`); full width in a row | `.vset .vo button` groups (language, light / dark / phone, motion, text size, feedback, audience, length), `.gdpg-lang` | the pills-as-radio of `.vo` (a group of exclusive choices is a segmented control, W) |
| 6 | **Switch** | `dsSwitch` | 52×32, thumb 24, on = accent | the On / Off pairs of the visitor's Settings (feedback; guess for the others) | On / Off as two pills in the visitor's UI (the Editor keeps its On / Off segment, `THEME_MAP` §4) |
| 7 | **Slider** | `.ds-range` | track, thumb, value fill. **PROPOSED:** `marks` (the two ends' words and the answer's pin, for Guess the year) | `.bt-range` (+ `.bt-track .bt-rail .bt-pin .bt-ends`), the dock's seek `.gd-seek` (per Look: dots, segments, hairline → one slider over the progress) | per-Look seek drawings (one slider; the panel style picks the surface) |
| 8 | **List and row** | `dsList`, `dsRow` | leading icon tile / dot; title, line (56 / 72); trailing chevron / switch / badge. **PROPOSED:** leading `picture` (B's 52 px thumb); trailing `meta` (the minutes, `.cmin`) and `tick` (read, done: `.ctick`, `.gdtry-tick`); `indent` (a sub-row) | `.crow` / `.crow.sec` / `.crow.sub` (Contents), `.xp-row` (Explore: W's Explore is a list of rows), `.gbmenu button` (✦ menu), `.gdtry-row`, `.vhow`, `.lt-acc` (accordion head) | B's hand-made row heights and dividers |
| 9 | **Tile (compact card)** | the tag card `.t-card` | thumb + name + line; surface glass / solid | `.te-card` (Nearby; 178×60), and for P6 the block tiles `.tcard`, `.rcard`, `.nextalong` | -- |
| 10 | **Panel** | `.ds-panel.g`, `.ds-solid`, `dsPhoto` | kind glass / solid / photo card (the fixed shade); elevation 1-3, none on dark panels. **PROPOSED:** kind `ink` (B's Ink glass: always dark, `--ts-chrome-glass-hero`, `THEME_MAP` §2.2 row 6); the `aurora` light (dark mode only, two light tokens, `THEME_MAP` §5.7); `anchor` (a panel placed by a part, for the term popup) | `.camask`, `.gbmenu` (glass), `.gdtry`, `.tepanel`, `.gpop` (solid, anchored), `.gdbeat` (the dock's panel style) | the panels' own surfaces (`#16181C`, `#1C1A21`, `#26232B`, `#121833`); Light glass's indigo and Aurora's mint (closed accent list); sk-light / sk-dark (the mode decides) |
| 11 | **Sheet and grabber** | `.ds-sheet`, `.ds-dim`, `.grab` | stops peek / half / full + detent; grabber on / off; backdrop (one `--ts-scrim`, `THEME_MAP` §5.5); landscape column. **B's merged:** half height max 0.30-0.60, full height top / camera strip, tap outside closes, back button closes (all in Unity's `CardOptions`) | the card sheet (`.scr`, radius 22 → `--ts-radius-xl`), `.vset`, `.csheet`, `.onb` (full), the dock at Open and Full | the white grabber literal; two private bottom-sheet radii |
| 12 | **Top bar** | `.ds-top`, `.ds-back`, `.ds-x` | back, title large / inline, trailing close. **B's merged:** breadcrumb pages / off; surface fade / glass (the card's top band, Unity `ChromeBand`); shown at Full after the hero | `.topband`, `.topbar`, `.xp-h` | the top band's third CSS look "clear" (not offered; remapped to fade already) |
| 13 | **Dialog** | `.ds-dialog`, `.ds-scrim` | title, body, one or two actions | none in B's visitor UI today (guess: the stop question becomes one if the developer wants it modal, decision 8) | -- |
| 14 | **Toast and banner** | `.toast`, `.ktoast`, `.ds-banner` | toast: message, icon, one at a time; banner: icon, text, one text action. **PROPOSED:** banner `actions: 1-2` (Continue + Start again); `place: top / bottom` | `.gtoast .gtsv` and `.gstopask` (banner), `.gdtoast` (toast, top), `.st-toast` (toast), `.st-banner`, `.st-slow`, `.ga-note`, `.tl-msg` (banner) | the stop toast's third quiet button (the banner times out or is swiped away; guess); the literal guide dark (panels over the wall take `--ts-app-glass`, `THEME_MAP` §2.2 row 10) |
| 15 | **Badge and tag** | `dsBadge`, `.ds-dot`, the tags (`--tagbg`) | badge accent / soft, dot; tag. **PROPOSED:** badge tone `correct` (`.te-ok`, Verified) and `status` (Draft); tag surface `over-media` (one rule for B's 7 dark labels over pictures: `--ts-chrome-glass-hero` + `--ts-on-scrim`) | `.lensbadge`, `.gcoach` (accent; the orange goes), `.te-star`, `.te-ok`, `.tl-pip`, `.gd-qstar`, `.srcs em.v / .d`, `.agsrc.gen` (soft), `.ga-chip` (tag), `.qchips span` (tag), `.hloop`, `.hsplit em`, `.ba .lb`, `.hint2`, `.el-vid span`, `.trv .sl span`, `.vbadge` (over-media icon tag) | orange `#E07A1F` (about 3:1 under white, outside the accent list); `#1F4A2A` / `#6B5313` (status tokens) |
| 16 | **Progress** | `.ds-prog`, `.ybar`, Start's `.ds-tiles` | bar; shimmer / breathing. **PROPOSED:** forms `dots`, `segments`, `ring`; `countdown` (a bar that empties) | `.gdpg-dots`, `.gseg`, `.sk-track`, `.gd-mprog`, `.gd-ring`, `.gb-ring` / `.gb-bar`, `.gdtry-bar`, `.bt-bar` + `.ga-bar` (one), `.cmarks` (dots / "2 of 4"), `.vchips` (dots), `.ringrow` (ring), `.chapbar` (segments) | 9 private drawings |
| 17 | **Bubble** | the coach tip `.mtip` (glass with a tail, Azu beside; Playful: a paper bubble) | surface solid (on the card) / glass (over the camera). **B's merged:** tail up / down / left / right / none; anchor a part / Azu's head (flips); signed ✦; actions slot (a CTA chip, close, "Fewer like this?"); "more ›" | `.gbubble`, `.gdbub` (tips, remarks, walkthrough steps, invitations), `.azbub`, `.tebub`, `.teinv button`, `.st-bubble`, `.hinttip` | 5 radii and tail geometries; the private dark `#26232B`; the 13 px bubble text (13 pt floor) |
| 18 | **Choice** | the option card `.ds-opt` | selected: 2 pt accent ring + ✓, or accent fill + ✓ (3-up). **PROPOSED:** result `correct / wrong` with ✓ / ✗ marks (`--ts-correct`, `--ts-wrong`); `picture` option; result fill and percent (`--ts-result-fill`); layout list / grid / true-false | `.kq button`, `.kq.tf .tfb button`, `.kpic button`, `.dopts button`, `.poll button`, `.thumbs2 button`, `.choice .opts button`, `.bt-choice` + `.bt-mark`, `.onb-c` (= W's audience option card) | hard-coded green / red `.ok / .no`; three private radii |
| 19 | **Nav bar** | `dsNavBar` | indicator raised / pill / underline / none; shape floating / dock / full; fill solid / glass; labels; 3-5 items; rail in landscape. **B's merged** (already in Unity's `NavBarView`): fill `clear`; labels icon and text / icon / text / text on the chosen; chosen icon plain / filled; chosen colour section / accent / ink; icon colour; position bottom / top; show always / after the hero; "More" opening Contents. Hosts: the card's sections (P3) and the app's tabs (P4) | `#navbar` and its inline styles (they move to a stylesheet), the bar pill indicator | B's 30-40 px disc and 11 px labels (W's 48 pt disc and 13 pt labels, `_4.1.7` §4; decision 1) |
| 20 | **Type roles and eyebrow** | `ds.css:272-286` | display, title, heading, body, label, chip, caption, eyebrow (13 / 700 caps); the title face by look | B's 8 small-caps labels (`.bt`, `.gsec .ey`, `.el-eyebrow`, `.nextup small`, `.vset small`, `.pfoot small`, `.tnmap small`, `.achap small`) and about 40 private `font:` shorthands | 10-12 px eyebrows (13 pt floor) |
| 21 | **Empty state, skeleton, loader** | `dsEmpty`, `.ds-skel`, `.ds-tiles` | icon disc + title + text + CTA (Playful: Azu); skeleton; tiles loader. **B's merged:** loading look skeleton (default) / tiles / mascot / spinner / AR scan (P4); empty and lost: icon / mascot / picture frame; offline: banner or empty + button | `card/states.*`: 15 looks | the scan teal and done green literals; per-look shadows |

W components with no B variant (the search, the action bar, the mini-map, the coach mark, the Near chip, the option cards for the
look and time, the action views): ported with their screens in P4, from the same registry. The dialog and the switch are built in P3
because the registry is the plan's (`_4.2_Lab_Merge_Plan.md` §2 "Components").

### 2.2 · B kept, redrawn on the tokens (W has no part for the role)
Each keeps its options; its colours, sizes, radii, shadows and durations become tokens; its parts use the components above.

| Part | B | Kept options | Built from | Unity |
|---|---|---|---|---|
| **Open Row** | `.bprev .open` | plain / nudge / shine; per block inherit | type roles | `OpenRowView` |
| **Page foot** | `.pfoot` | on / off | eyebrow, tile | `PageFootView` |
| **Section opener** | `.gsec`, `secCfg.start`, `effects.js` | start 7 + 9 edges + own strip; 6 dividers; colour section / accent / neutral; tint | eyebrow | `SectionOpenerView`, `OpenSectionView`, LookKit `TileRow` / `Wave` / `Fade` |
| **Block frame and section look** | `.bprev`, `.fr-*`, `.glasslit`, `.rimlit` | open / cards; normal / featured / quiet; glass; rim | panel tokens, `--ts-family-*-glass / -light / -sweep` | `BlockFrame`, `SectionSheets`, `RimLightView` |
| **Hero words** | `.hero .w` | look 7, words 3, motion 4, crop | the photo card's rule (fixed shade `--ts-scrim-hero`, eyebrow `--ts-on-scrim-2`) | `HeaderBlockView`, `HeroWordsOnPicture` |
| **Locate pin** | `card/sections.js:55-59`, `card/locate.js` | style 3, colour 4, size, float, fold | the 34 `--ts-locate-*` tokens | `LocatePin`, `LocateFabView` |
| **Picture viewer** | `.viewer` | counter chips / count / none; zoom | icon button (hero), progress dots; always dark (a fixed rule: photos on black, `--ts-video-letterbox`) | `PictureViewer`, `TakeoverView` |
| **Media frame** | element picture | width, frame rectangle / arch, ratio, tap | radius tokens | `MediaFrame` |
| **Mini player** | the dock's Mini, `.tepill` | its form follows the panel style: pill / ring capsule / bar with progress / words only | icon buttons, progress | `MiniPlayerView` (`poi-card-mini*`) |
| **Talk button** | `.gd-talk` | 6 states, tap / hold | icon button M / L | new |
| **Guide button ✦** | `.gbtn`, `.azfab` | icon star / face / word; tour ring and bar; side; fold; Azu beside | icon button, progress ring | new (guess: in P4 the action bar's Azu slot uses it) |
| **Captions and teleprompter** | `.gcap`, `.gtele` | words during a tour teleprompter / bubble / off; height | always dark: `--ts-chrome-glass-hero` + `--ts-on-scrim` (a caption convention, `THEME_MAP` §2.2 row 10) | new |
| **Spotlight, focus ring, told bar** | `.gspot`, `.gfocus`, `.gtold`, `.gdring` | cue bar / spotlight / ring / none; scrim; zoom | `--ts-scrim`, `--ts-accent` (the orange goes) | new |
| **Demo hand** | `.dhand` (absorbs `.gdfinger`) | shown / off | -- | new |
| **Read-along highlight** | `guide/highlight.css` | comet / sentence / fill / word | PROPOSED `--ts-highlight` + `--ts-accent` (one recipe, not four) | new (rich text marks; guess) |
| **Chat message** | `.gm-azu`, `.gm-me`, `.gm-note`, typing | speaker guide / visitor / note; typing | bubble tokens | `card-dialogue__bubble(--visitor)`, lifted into a component |

**The composites** (no component of their own; one module each, built only from the parts above):
- **The Guide Dock** = sheet (Open, Full) + mini player + talk button + chip row + icon buttons + slider + progress + captions +
  chat messages. Its four Looks become one option, **panel style**: `glass` (Paper glass = W's glass panel), `sheet` (Player sheet =
  W's solid sheet), `prompter` (words over a fade, no box: B kept), `ink` (always dark, B kept); Aurora's light is an option of the
  dark panels. Eight palettes become the mode's tokens.
- **The beat card** = panel (the dock's style) + chips + choices + slider + progress + badge + the tail strip.
- **The tour end** = panel / mini player / bubble + chips + tiles + buttons + badges.
- **The first-launch pages** = W's onboarding parts (button L, progress dots, segmented language); merged with W's Onboarding in P4.
- **Who's exploring** = sheet (full) + choice (W's audience option cards); W's Onboarding in P4.
- **Ask about the view** = panel glass + chip row + icon buttons + the pins (wall labels in P4).

### 2.3 · PROPOSED (for the developer's approval)
**Options (25):** button `width full`, `state done`; icon button surface `hero`, kind `main`; chip `kind suggestion`, `value`; chip
row `wrap`; slider `marks`; row leading `picture`, trailing `meta`, `tick`, `indent`; panel kind `ink`, light `aurora`, `anchor`;
banner `actions 1-2`, toast `place`; badge tone `correct`, `status`; tag surface `over-media`; progress forms `dots`, `segments`,
`ring`, `countdown`; choice result marks, `picture` and fill (counted as one: the quiz's needs). The bubble's tail, anchor, signed and
actions and the nav bar's B options are B's merged, not new to Unity (the nav bar's are in `NavBarView` already).

**Tokens (8), beside `THEME_MAP` §1.2's 29:**
- `--ts-icon-button-s / -m / -l` (32 / 44 / 52; guess: Unity has `--ts-chrome-disc` 36 for one of them);
- `--ts-bubble-tail` (the tail's size; guess 12 px);
- `--ts-switch-width / -height / -thumb` (52 / 32 / 24);
- `--ts-slider-thumb` (28);
- `--ts-chip-text` = `THEME_MAP`'s PROPOSED `--ts-size-chip`, not a new one.

Everything else a component needs is a theme token (`THEME_MAP` §1) or an existing card-level token in `CardTokens.uss`
(`--ts-touch-target` 48, `--ts-chip-height` 32, `--ts-chip-radius`, `--ts-pill-radius`, `--ts-progress-height` 6,
`--ts-dot-size`, `--ts-bubble-max-width`, `--ts-chosen-border`, `--ts-choice-border`, `--ts-grabber-*`, `--ts-navbar-*`…).

### 2.4 · Dropped (beyond the rows above)
- **Per-panel palettes:** the dock's 8, the beats' `--bt-*`, the teleprompter's `--tl*`, the highlighter's `--hl*` (one set of
  roles: the mode's tokens).
- **The talk button's per-Look and per-skin skins** (one talk button; the panel style changes its surface only).
- **The classic guide panel** (`D.guide.panel = classic`: `.gcap` bar, `.gbar`, `.gask`): its captions become the Captions part
  and its controls repeat the Mini's (decision 4).
- **Two of everything:** the second ghost finger `.gdfinger`, the second countdown `.ga-bar`, the second "back to the tour" pill,
  the three extra highlight recipes, the second bottom-sheet rule (`.vset,.csheet` becomes the sheet).
- **Lab-only lookalikes stay LAB:** the iOS alert and Settings stand-ins (`.gdpg-alert`, `.gdpg-set`, `.gdpg-sw`), the preview
  bar, the demo tile, `.hapring`, the Editor's `.chip`, `.tag`, `.badge`.
- **Dead rules (guess):** `.prev`, `.sec .ey`; W's `ind-dot`, `ind-glow`.

## 3 · Unity: each component's home

The rule (`Framework/Tests/Editor/UI/UiRenderTargetGuardTests.cs`): what lives in `Blocks/` and `Components/` never names the
card's sheet or host types (`PoiCardSheetView`, `PoiCardHost`, `NavController`, `TakeoverView`, `BlockStackView`,
`CardChromeView`), the screen size or `PanelSettings`; no uGUI in UI Toolkit folders; no UI Toolkit in `World/`. The Lab's
components follow it (a component never names its host), so each one is a `Components/` class.

| Component | Existing USS classes and VisualElements | The block copies it absorbs (P6) | Home |
|---|---|---|---|
| Button | `.card-pill` (`Screen/Cards/CardParts.uss:722`, the one shared button), `card-tap` (a marker) | `card-quiz__button`, `card-story__button`, `card-dialogue__button` (same rule), `__next` / `__continue`, `card-show-on-wall__button`, `card-place-in-ar__button`, `card-collect__add`, `card-actions--sticky_cta`, `card-hero__primary / __listen`, `search-button`, `ts-zoom-control__button` | new `Components/ButtonView` (a `Button` + `ts-btn--<kind>--<size>`), `card-pill` folded in |
| Icon button | `poi-card__close / __settings / __menu-button / __mark-disc`, `ts-page-bar__back`, glyphs `CardIcons.VectorGlyph` (15 shapes) | `card-audio__play`, `card-video__overlay`, `ts-el-video__play` | new `Components/IconButtonView` |
| Chip, chip row | `nav-chip*` (`KindChipsNavView`, `NavChipRow`), `facet-chip` (a `Toggle`), `NavStrip` (sideways strip) | `card-takeover__chip`, `card-audio__chip`, `poi-card-chip`, `card-facts--chips`, `card-sources__chip` | new `Components/ChipView` (a `Toggle` for filters, a `Button` for suggestions); the row = `NavStrip` + a `FadePainter` edge |
| Segmented | `card-settings__choice` (`CardVisitorSettingsView`), `view-mode` (`ViewModeControl`, Search) | -- | new `Components/SegmentedView` (guess: Unity 6's `ToggleButtonGroup` restyled) |
| Switch | the `Toggle` behind `facet-chip` | `card-feedback__vote--on` | new: a `Toggle` restyled (track and thumb) |
| Slider | `AudioScrubber` | `card-before-after__handle` (stays the block's) | `AudioScrubber` lifted into `Components/` |
| List and row | `CompactRowView` (`card-compact*`), `result-row` (`ResultsListView`, a virtualised `ListView`) | `nav-contents__row`, `nav-drawer` rows, `card-practical__row`, `card-sources__row` | `CompactRowView` lifted into `Components/ListRowView`; long lists in a `ListView` |
| Tile | -- | `card-related` cards, `card-timeline` events | new `Components/TileView` |
| Panel | `.search-panel`, `card-menu__panel`, `SectionSheets` | -- | new `Components/PanelView`; glass is the dense tint until the camera-blur shader exists (`_4.1.7` §3.2) |
| Sheet, grabber | `PoiCardSheetView`, `SheetStopRule` (`Runtime/Blocks/SheetStopRule.cs`), `SheetDrag`, `.poi-card__handle / __grabber` | -- | a host-free `SheetView` in `Components/` that `PoiCardSheetView` uses (guess: the guard forbids the reverse) |
| Top bar | `PageTopBar` (`ts-page-bar*`), `.poi-card__chrome-band(--glass)` + `FadePainter` | -- | `PageTopBar` gains the band's surface |
| Dialog | `CardMenuOverlay` (`card-menu--popover`, `__scrim`, `__panel`) | -- | new `Components/DialogView` on the overlay layer |
| Toast, banner | none | -- | new `Components/ToastView` on its own layer |
| Badge, tag | `ts-el-video__badge`, `nav-item__count` | `card-video__length`, `card-collect__stamp`, `card-preview__teaser-name`, `card-hero__label / __loop`, `card-sources__chip--verified / --draft` | new `Components/BadgeView` |
| Progress | `nav-progress*` (`ProgressMapNavView`), `poi-card-mini__fill` | `card-story__segment`, `card-gallery__dot`, `card-collect__fill`, `card-audio__fill`, `card-status` rings (`Rings/*.png`) | new `Components/ProgressView` (the ring by a `Painter2D` arc) |
| Bubble, chat message | `card-dialogue__bubble(--visitor)` | the Dialogue block's | new `Components/BubbleView`; the chat message lifted from the Dialogue block |
| Choice | -- | `card-quiz__choice`, `card-dialogue__choice`, `card-poll__option` | new `Components/ChoiceView` |
| Nav bar | `NavBarView` (`Screen/Cards/Navigation/NavBarView.cs`, one `Painter2D` path), `nav-bar*` (21 classes) | -- | `NavBarView` gains the dock shape and the rail; moves to `Components/` when the app's tabs use it (P4; guess) |
| Type roles, eyebrow | `LineElement` (`ts-el-title / -eyebrow / -heading / -lede`) | `card-hero__eyebrow`, `card-open-section__eyebrow`, `card-rich__heading` | `LineElement` |
| Empty, skeleton, loader | `card-image--unavailable`, `card-preview__loading`, `results-empty` | -- | new `Components/StateView`; the tiles loader on LookKit `TileRow` |
| B kept (§2.2) | `OpenRowView`, `PageFootView`, `SectionOpenerView`, `OpenSectionView`, `BlockFrame`, `HeroWordsOnPicture`, `LocatePin`, `PictureViewer`, `MediaFrame`, `MiniPlayerView` | -- | as they are |
| The guide's parts | none: `UI/World/GuideCharacter/` is empty and `World/` may not use UI Toolkit | -- | new `UI/Screen/Guide/` (the dock, the talk button, captions, spotlight, demo hand; guess on the folder) |

### 3.1 · What UI Toolkit cannot do, and each component's path
(`skills/Feature_Prototyping.md` 3.2; the tags are the Unity lint's.)
| Limit | Where the Lab uses it | Path in Unity |
|---|---|---|
| **No backdrop blur** | W's glass (`--gblur`), the dock's glass, the top band's glass, Aurora (22 px), `.camask` | the dense tint (`--ts-app-glass` solid 0.84 / 0.98; the card's glass tokens); later the camera-blur render texture (`unity: camera-glass`, `_4.1.7` §3.2). The Lab shows blur as the target, labelled |
| **No gradients** | the hero shade, the top band's fade, the section tint, `.glasslit`, Aurora's drifting glows, the Open Row's shine, the edge fade of chip rows, W's photo shade, the skeleton shimmer | `TonePainter` (hero), `FadePainter` (top band, section tint, chip-row edges), tinted ramp PNGs (`card_glass.png`, `top_shade.png`, `hero_scrim.png`), the `SectionGlass` / `RimSweep` shader graphs (glass, rim, Aurora's light; guess for Aurora), `PostProcessTextVertices` (shine) (`unity: ramp`) |
| **One text-shadow** | the hero's words, the captions over a picture | `HeroWordsOnPicture`'s second halo label |
| **No z-index** | the dock over the card, bubbles and the spotlight over the dock, toasts, dialogs, the Locate pin | layers by child order, one root each (§3.2) |
| **No box-shadow** | every elevation, W's three levels, the floating discs | the 9-sliced `card_block_shadow.png` tinted by `--ts-elevation-1/2` (and PROPOSED `-3`); none on dark panels (W's rule) |
| **No `calc()`, `color-mix()`** | B's 36 `calc()` and 28 `color-mix()`, W's `--P` units | `applyTokens` writes resolved values (`THEME_MAP` §1.2) |
| **No cubic-bezier, no keyframes** | the talk button's rings, the nudge, the shimmer, Azu's hops, W's springs | a closed list of easings (`--ts-ease`, PROPOSED `--ts-ease-emphasis` = ease-out-back); C# tweens for keyframes (`tween:`, `unity: ease-out-back`) |
| **No CSS masks, no inverse clip** | the spotlight's hole, the chip rows' edge fade | the spotlight as a `Painter2D` path with a hole (guess: an even-odd fill) or four rects + a ring; the fade by `FadePainter` |
| **No `text-transform`, no `line-height`** | eyebrows in capitals, every leading | `ElementStyle.Capitals`, `--ts-el-leading` with rich text |
| **No SVG** | B's 28 section icons, W's icons, the edges | `CardIcons.VectorGlyph` / sprites (`unity: sprite`); edges by LookKit painters; the icon line by sprite set (`THEME_MAP` §1.2, guess) |
| **9-slice: yes** | W's Playful frames (`border-image`) | native: `-unity-slice-*` on a background image (`unity: sprite`) |
| **999 px pills** | B's 61 pills, W's `--r-chip` 99 | PROPOSED `--ts-radius-pill` (guess: UI Toolkit clamps a radius to half the side; check in Unity) |

### 3.2 · The layers (draw order instead of z-index)
From the bottom: the camera and the wall (World, uGUI) › the screen › the card sheet › the Locate pin (`LocateFabView`'s own layer) ›
the guide's layer (dock, mini, ✦, captions, teleprompter) › cues and spotlight (bubbles, rings, the demo hand) › toasts and banners ›
dialogs and their scrim. In the Lab the same order is one z-index scale in `core/theme` (PROPOSED: `--ts-layer-*` as Lab-only values,
LAB; Unity has the child order).

## 4 · The reach test
**What it does** (P3's "done when"): for each visible part, it switches each profile option by the profile bar (real clicks), reads
the part's computed colour, background, border, radius, font size, padding and transition, and checks that each one **equals the
resolved token for that profile** and **changes when its token changes**; a static scan checks that no rule for the part holds a
literal. A part missing from the list fails the test (so a new part must join it).

**The options:** look (Premium / Playful), mode (light / dark), text size (S / M / L / XL), audience (Family / Explorer / Expert:
roundness, density, icon line, type step, motion style), motion (the phone's / reduced). ✓ = follows; **fixed** = must not change, and
the test checks that it does not (with the rule's reason); -- = has nothing that option changes.

| Visible part | Look | Mode | Text | Audience | Motion |
|---|---|---|---|---|---|
| **Card chrome** | | | | | |
| Sheet (surface, radius, shadow), grabber | ✓ | ✓ | -- | ✓ roundness | ✓ the stop transition |
| Peek row | ✓ | ✓ | ✓ | ✓ | -- |
| Top band, page top bar (back, crumb, title) | ✓ | ✓ | ✓ | ✓ | ✓ |
| Hero words, eyebrow, line | ✓ face | fixed (words on a photo, both modes) | ✓ | ✓ type step | ✓ hero motion |
| Hero shade | -- | fixed (`--ts-scrim-hero`) | -- | -- | -- |
| Nav bar (surface, items, icons, disc, labels, pill, underline) | ✓ | ✓ | ✓ labels | ✓ roundness, icon line | ✓ the disc's slide |
| Close and settings discs (over the hero, after it) | ✓ | over the hero fixed, after it ✓ | -- | ✓ icon line | -- |
| Sticky action | ✓ | ✓ | ✓ | ✓ | ✓ slide |
| Locate pin and label | ✓ label | ✓ rim only (the pin's colours are fixed locate tokens) | ✓ label | -- | ✓ float, fold |
| Open Row | ✓ | ✓ | ✓ | -- | ✓ nudge, shine (reduced = plain) |
| Section opener (eyebrow, edge, tiles, tint, divider) | ✓ | ✓ | ✓ eyebrow | ✓ ornaments | ✓ reveal, drift |
| Block frame, glass section, rim | ✓ | ✓ | -- | ✓ roundness | ✓ the rim sweep |
| Page foot, next up, page chips, chapter chips, layout tabs and pager | ✓ | ✓ | ✓ | ✓ | -- |
| Contents sheet (rows, ticks, minutes, filter chips) | ✓ | ✓ | ✓ | ✓ density | ✓ |
| Settings sheet (segmented, switches, rows) | ✓ | ✓ | ✓ | ✓ | ✓ |
| Term underline, popup, term page | ✓ | ✓ | ✓ | ✓ | ✓ |
| Picture viewer chrome | ✓ | fixed (always dark) | ✓ counter | -- | ✓ |
| See it as row and badge | ✓ | ✓ | ✓ | ✓ | -- |
| Who's exploring (choices, skip) | ✓ | ✓ | ✓ | -- | ✓ |
| Explore (header, rows, the Explore pill) | ✓ | ✓ | ✓ | ✓ | -- |
| States: skeleton, tiles, mascot, spinner, empty, lost, offline banner, done toast and button | ✓ | ✓ | ✓ | -- | ✓ (reduced: still) |
| Cues: grabber nudge, bar ring and tip, term glow, strip slide | ✓ | ✓ | ✓ tip | -- | ✓ (reduced: none) |
| **The blocks' shared parts** | | | | | |
| Elements: title, eyebrow, heading, lede, paragraph, drop cap, callout, quote, list, caption | ✓ face | ✓ | ✓ every text (not the paragraphs alone) | ✓ type step, ornaments | -- |
| Media frame, carousel marks, over-media tags | ✓ | tags fixed (over a photo) | ✓ tags | ✓ roundness | ✓ entrances |
| Buttons, chips, choices, badges, progress and sliders inside blocks | ✓ | ✓ | ✓ | ✓ | ✓ |
| **The guide** | | | | | |
| ✦ guide button, its ring and bar, Azu beside, the band | ✓ | ✓ | -- | ✓ icon line | ✓ |
| ✦ menu (panel, rows) | ✓ | ✓ | ✓ | ✓ density | ✓ |
| Dock Mini (each panel style; Ink: fixed dark) | ✓ | ✓ (Ink fixed) | ✓ | ✓ | ✓ |
| Dock Open / Full: header pills, controls, Play, seek, chips, ask field, send, chat messages | ✓ | ✓ (Ink fixed) | ✓ | ✓ | ✓ |
| Talk button (6 states) | ✓ | ✓ | -- | ✓ | ✓ (reduced: no rings) |
| Teleprompter strip, captions | ✓ face | fixed (always dark) | ✓ | -- | ✓ scroll |
| Read-along highlight | -- | ✓ | ✓ | -- | ✓ (reduced: word only, guess) |
| Bubbles (tips, remarks, walkthrough, invitations, Azu's, tour end) | ✓ | ✓ | ✓ | ✓ | ✓ |
| Spotlight scrim, focus ring, told bar, glint | -- | scrim fixed (`--ts-scrim`); ring ✓ | -- | -- | ✓ |
| Demo hand, scroll cue | -- | -- | -- | -- | ✓ (reduced: none) |
| Beat card (chips, choices, slider, verdict, score), its strip | ✓ | ✓ | ✓ | ✓ | ✓ |
| First-launch pages (dots, language, buttons) | ✓ | ✓ | ✓ | -- | ✓ |
| Things to try | ✓ | ✓ | ✓ | ✓ | ✓ |
| Tour end: panel, pill, invitation, star, tick, questions, nearby tiles, actions | ✓ | ✓ | ✓ | ✓ | ✓ |
| Tell me more, Game paused, back to the tour | ✓ | ✓ | ✓ | ✓ | ✓ |
| Stop banner and question, tips toast | ✓ | ✓ | ✓ | -- | ✓ |
| Ask about the view (panel, chips, pins, mic) | ✓ | ✓ | ✓ | ✓ | ✓ |
| Coach badge, hero Start / Continue / Start again | ✓ | ✓ / over the hero fixed | ✓ | ✓ | ✓ |

**Not in the test (not theme):** the mascot's art, the pictures, the 3D models, the Lab's tools (preview bar, demo tile, self-test),
the Editor (its own `--u-*` skin).

**The fixed list** (each with its reason, kept in the test so a "fix" that themes them fails): the hero's words and shade, the
over-media tags, the picture viewer, captions and the teleprompter (always dark), Ink glass (always dark by choice), the spotlight's
scrim, the Locate pin's colours. The section family colours follow the mode only (guess: Playful keeps them; the developer may want
Playful's own family tints).

## 5 · The order of work (P3)
The plan gives P3 3-4 rounds. Proposed, each round ending green on its tier with its matrix captures:

| Round | What | Files (guess) |
|---|---|---|
| **P3.1 · The guide's palettes onto `--ts-*`** | the dock's 8 palettes, `--bt-*`, `--tl*`, `--sk*`, `--hl*` and the guide's literals renamed to theme roles (no alias layer); the PROPOSED tokens and options listed in the Editor's "additions for the ThemeAsset" for approval; `_4.1.7` → v1; the colour ratchet down by the guide's share (about 800 literals at `7f75c38`) | about 25: `guide/*.css` (16), `guidance/present.css`, `world/*.css` (2), `agent/agent.css`, `guide/panel-look.js`, `guide/dock.js`, `core/theme.js`, a test, docs |
| **P3.2 · The registry** | `src/components/` (a new area between theme and screen, `componentDef(id, {options, html, demo})` like `screenDef`): button, icon button, chip and row, segmented, switch, slider, list and row, tile, panel, sheet and grabber, top bar, dialog, toast and banner, badge and tag, progress, bubble and chat message, choice, type roles, states; each with its Playful frame, its Editor rows with a demo (App › Design system, W's kit pages), its reach entry and a test by real clicks; W's metrics vs `CardTokens.uss` settled (decision 1) | about 30: 12 component modules (`.js` + `.css` = 24), `core/theme.js`, the Playful sprites in `assets/playful/`, 2 tests (the components, the reach test's first half), the Editor's Design system rows, docs |
| **P3.3 · The card's chrome onto it** | sheet, grabber, peek row, top band and top bar, nav bar (its inline styles to the stylesheet), close and settings, sticky action, Locate label, Open Row, section opener, block frame, page foot, Contents, Settings, term popup, viewer, states, cues, See it as, Who's exploring, Explore; the blocks' shared parts (elements, the 17 widgets) mapped to tokens without a redesign (decision 7) | about 30: `card/*.css` (10), `card/*.js` that write markup or inline styles (about 12: `sections`, `controls`, `chrome`, `chrome-pinned`, `hero`, `locate`, `contents`, `pages`, `terms`, `states`, `hints`, `elements`), `phone/sheet.css`, `phone/grabber.css`, `phone/settings.js`, `core/base.css`, `audiences/*` (3), `screen/*` (2), the reach test's card half |
| **P3.4 · The dock and the panels** | the dock as a composite with the panel style; the beats, bubbles, chips, toasts, tour end, game layer, ✦ and its menu, Ask about the view, captions and teleprompter, spotlight, demo hand and highlight; the reach test complete; **the ratchet at 0**, after which the build refuses a new literal | about 25: `guide/*.js` that build markup (about 12), `guidance/present.js`, `world/*.js` (2), `mascot/azu2d.css`, the reach test complete, docs (PROTOTYPE_STRUCTURE, README, STATE_MAP, THEME_MAP, this map) |

**In all: about 100-110 files over 4 rounds** (about 22 new). P3.2 can start before P3.1 ends only if the PROPOSED list is approved
first; P3.3 and P3.4 depend on P3.2. P4's screens then take the same registry (the search, action bar, nav bar tabs, option cards).

## 6 · Open decisions (each with a recommendation)
1. **W's metrics or Unity's card metrics.** `CardTokens.uss` says nav bar 62 high with a 40 px disc and 11 px labels, grabber 40×4,
   discs 36, pills radius 24; W (`_4.1.7` §4) says 64, 48 pt disc, 13 pt labels, grabber 36×5, close 32, full pills. Call 6 (Unity's
   values win) is about the theme's colours; call 7 (W's design wins) is about the components. **Recommended:** W's metric wins where
   it follows a fixed rule (13 pt minimum, the 48 target) or `_4.1.7` §6 already lists it as Unity work (nav bar 64, disc 48, labels
   13, grabber 36×5); Unity's `CardTokens.uss` value wins elsewhere. Those W values go on the additions list as **CardTokens changes**,
   and the token test is extended to `CardTokens.uss` (the same method as P1b's test).
2. **A suggestion chip.** W's chip is a filter (it has a selected state). B's question chips, quiz answers and Tell me more act on a
   tap. **Recommended:** one chip with `kind: filter / suggestion` (PROPOSED), not a second component.
3. **The dock's four Looks.** **Recommended:** keep all four as the panel style (glass, sheet, prompter, ink), each built from W's
   panel and sheet; the Mini's form follows the style; one talk button; Aurora as the dark panels' light. Dropping Player or Teleprompter
   would lose a taste-dependent option (`Framework Completeness`).
4. **The classic guide panel** (`D.guide.panel = classic`). **Recommended:** drop it; the dock covers its roles, and its captions
   become the Captions part. The other way keeps a second guide UI to port and test in every matrix.
5. **The ✦ guide button and W's Azu in the action bar.** **Recommended:** one component with the ✦'s options, floating on the card
   and without the camera (P3), placed in the action bar's Azu slot when the bar shows (P4).
6. **Playful's 9-slice kit in P3.** **Recommended:** port each component's frames with the component (P3.2): the reach test's
   matrix includes Playful, and W's Playful is the frames, not only the tokens. The other way (tokens only: outline, hard shadow,
   roundness, Fredoka) is half the work but looks unlike W's Playful until a later round.
7. **The blocks' own literals.** P3's "done when" asks for no literal in anything the visitor sees, and P6 rebuilds the blocks.
   **Recommended:** P3.3 maps the blocks' and the elements' literals to the nearest token (a token-only pass, no redesign); P6
   rebuilds each block from the components. The other way (exclude block files from P3's ratchet) leaves the ratchet above 0 until P6.
8. **The stop question.** **Recommended:** a banner with two actions (non-modal, the tour waits under it); a dialog only if the
   developer wants Stop to be modal.
9. **The spinner loader.** **Recommended:** keep it as an option (a 3D model or a panorama has no layout to sketch); the skeleton
   stays the default.
10. **The section family colours under Playful.** **Recommended:** follow the mode only for now (Unity has one family set); a
    Playful family set is a later PROPOSED if the matrix shows they clash with the Playful kit.
11. **Where the guide's UI lives in Unity.** `World/GuideCharacter/` is empty and `World/` may not use UI Toolkit. **Recommended:**
    `UI/Screen/Guide/` for the dock and its parts, with the generic parts in `Components/` (guess on the folder name).

## 7 · What blocks P3
- **P1b.1** (running): the token table and `applyTokens`, the one dark mode, the card's table renamed, the colour ratchet. P3
  counts its literals from P1b.1's ratchet, not from this map's numbers.
- **P1b.2:** the profile model (the audience's roundness, density, icon line, type step and motion style are what the reach test
  switches), the one profile bar (the test's real clicks) and the Editor kit (each component's rows and demo).
- **The developer's approval** of the PROPOSED options and tokens (§2.3, `THEME_MAP` §1.2) and decision 1: P3's gate.
- **W's Playful sprites** (`_4.1.2_Prototype/lab_src/media/*.txt`, data URIs): copied into `assets/` as files (the page stays under
  its size target).
- Nothing else found: the capture tools (P1a.2) and the router (P1a.2) are in place.
