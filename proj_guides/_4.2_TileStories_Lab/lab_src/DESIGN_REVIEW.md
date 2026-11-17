# The Design Review: the best of both Labs (version 0)

**What this is.** A screen-by-screen and part-by-part judgement of the two prototypes, made from real captures, saying how
the merged app becomes **premium, minimalist, immersive, 3D, "wow" and modern**. Nothing was built. It feeds P3 (components),
P4 (screens), P5 (media) and P6 (blocks) of `_4.2_Lab_Merge_Plan.md`; the developer reads it before any of them starts.

Names: **W** = the App prototype (World Editor Lab, frozen at v20.1, `_4.1.2_Prototype/World_Editor_Lab.html`). **B** = this Lab
at **HEAD `4a6ca97`** (P1b.1 done; the P1b.2 builder's work in `src/` is not in it). "(guess)" marks a judgement not measured.

**The calls this review works inside** (`_4.2_Lab_Merge_Plan.md` §5): W's App design wins where both style the same role (call 7);
the dark is W's warm dark `#1C1A17` and Playful's azulejo navy (call 10); sizes follow the fixed rules first (13 pt text, 48 pt
targets), Unity's `CardTokens.uss` otherwise (call 9); the Lab specifies Unity, so every effect below names its path.

**Production paths** (`skills/Feature_Prototyping.md` 3.2): **Native** (a Unity feature as is) · **Build** (code or a shader we
write, with its rough mobile cost) · **Import** (an asset from another tool) · **Browser-only** (rejected). UI Toolkit limits: no
backdrop blur, no gradients, one text-shadow, no 3D transforms, no z-index; the world content (the wall) may use 3D, glow and
gradients.

## 0 · How it was made
- **Captures, real pixels, device scale 2.** W through its own `tools/review/state_shots.py` states (plus `start`, `explore`,
  `album`, `main_coach`, `tap_default`); B from a `git archive HEAD` copy built in the scratch folder (`python build.py`: every
  check at 0, the theme ratchet 1403 / 186) and served at `LAB_PORT=8644`, through its `tools/review/labpage.py` base state plus
  the states this review needed (the card scrolled through its sections, Settings to its end, the dock at Mini / Open / Full, the
  ✦ menu, Ask about the view, Who's exploring, the three first pages, Story, Gallery, the viewer, a block page).
- **The matrix:** Premium / Playful × light / dark × text M / L (B's text steps are `default` / `large` at HEAD: B's "L" is
  118 % on paragraphs only), phone portrait; one tablet-portrait cell per screen. W: 19 states, 190 cells. B: 29 states, 228 cells.
- **Measured, not only seen:** a script read every visible text in the phone (font size, its contrast on a solid ground,
  clipping) and every button's box. W draws its phone at 0.956 px per pt, so W's 13 pt floor reads 12.43 px; B draws 1 px per pt.
- **Sheets in `../design_review/`:** `sbs_01`…`sbs_11` (W | B side by side) and `issues_1.jpg` (the crops of §3). The raw cells
  and the scripts are in the review's scratch folder (not committed).
- **Not captured:** motion as frame sequences (the stills judge layout, type and colour; motion is judged from what each Lab
  offers, §2.4) and landscape (out of P1b-P4's scope).

## 1 · Summary
**Verdicts: 27 parts judged. W better 13 · B better 5 · both have something 9.** W gives the system: type roles on the 13 pt
floor, grouped panels, segmented controls, one floating nav bar with a raised disc, the warm layered dark, Playful as a real
kit. B gives the content and the soul: the full-bleed hero, the card's editorial body (azulejo tile bands, drop caps, section
colours, before / after), Azu in 3D, the guide's dock with read-along and talk. The merged app is **W's skin on B's body**, plus
the "wow" layer of §2, which neither Lab has yet.

**The top 10** (§4 has the sizes): 1 one floating bar instead of B's five floating things · 2 W's type roles and the 13 pt floor
on every B text (with text L reaching all of it) · 3 W's Settings with B's guide and tips inside · 4 the tap → card hand-off
(W's On tap card grows into B's hero) · 5 the warm dark and the navy Playful on every B sheet · 6 the dock on W's panels and
the app accent · 7 Azu as one 3D character in three homes · 8 the camera-blur glass · 9 the wall's lost buildings rising in 3D ·
10 motion tokens with springs and a shared-element transition.

## 2 · Verdicts, screen by screen and part by part

### 2.1 · Screens

| # | Screen | Sheet | Verdict | What exactly is better | The best of both |
|---|---|---|---|---|---|
| S1 | **Start and the first pages** | `sbs_01` | **Both** | **W's Start** is the one real hero of either Lab: the wall's own tiles under a warm shade, one focal Start button, a tile-row loader with a line of humour, Fraunces "TileStories" (one focal point, depth from the photo). **B's Hello page** has Azu rendered in 3D, a language segment in the top bar and a three-dot progress; but its middle is empty and the 3D Azu floats on a flat ground with no light or shadow | W's Start as the app's first screen (always dark by rule, `_4.1.7` §5); then B's pages (Hello, Who's exploring, the camera) on W's onboarding layout: B's 3D Azu standing **on** W's tile hero (a contact shadow, a rim light) instead of a flat page; B's progress dots and language segment kept |
| S2 | **Choosing who and how** (W's onboarding, B's Who's exploring) | `sbs_01` | **Both** | **W** asks look + audience + time + interests on one scrolling page, with a **live preview** ("On the wall, ages 12+ get…"), option cards with a picture of each look, ✓ marks and the 13 pt floor. **B** has the third audience (**Expert**), clean icon tiles in a 36 pt square and a calm single question with Skip; its choice cards are 52 pt high (good targets) | W's page and preview, with B's three audiences (Family 7-11 / Explorer / Expert) as W's option cards and B's icons; Expert's row says "Deep, always" (call 1). Where it is asked stays B's `askAudience` (start / card / never) |
| S3 | **The wall** (W's Main AR, B's wall) | `sbs_02` | **W** | W: a search pill and a settings disc in the top band, a soft top shade so the status bar reads, **markers by level with an icon and a label pill in the look**, one floating nav bar with a raised accent disc. B: white dots with 11 px labels (measured), an amber focal disc, and three loose controls at the bottom (✦, Azu, Explore) that do not form one bar; text L does not reach the labels | W's whole wall chrome; B keeps its painting (a stand-in wall), its 3D Azu (S11, §2.3 "Azu") and the ✦ guide button as the nav bar's centre (COMPONENT_MAP decision 5) |
| S4 | **Tapping a place** (W's On tap card, B's card at peek / half) | `sbs_03` | **Both** | **W's tapped card** is the best small object in either Lab: photo, a family dot + eyebrow, title, one line, a row of action buttons with one accent (Story), a heart; anchored to its marker (the leader line in layout 4). It keeps the wall in view (AR rule). **B's half sheet** gives the place its full photo, the mascot inviting "Start the guide · 3 min" and the Locate pin. B's **peek** is weak: the grabber sits on the title (I1) and the eyebrow is 11 px | Tap → W's card on the wall; its **Story** action grows the card's photo into B's sheet at half (a shared-element transition, §2.4); peek becomes W's card itself (one object, not two). The Locate pin moves into the card's action row as "Show me where" when the card is open |
| S5 | **Words on a photo** (W's Coming soon page, B's hero) | `sbs_04` | **Both** | **B's hero at full** is the most immersive screen of both Labs: the painting edge to edge, the title in Fraunces over a fixed shade, the CTA as a glass pill. **W's photo card** is tighter: a badge ("Coming soon") on the picture's edge, the title below the photo on the panel, the 13 pt floor | B's full-bleed hero is the card's head; W's rules on it: the eyebrow at 13 pt (B's is 11), the shade `--ts-scrim-hero` measured at the worst pixel, the CTA as W's glass button L |
| S6 | **The card's body** (sections, blocks) | `sbs_09` | **B** | Only B has it, and it is rich: an azulejo **tile band** opening each section (an identity no other app has), section colours (about / stories / media…), a drop cap and a plum eyebrow on the Story page, Before / After with a handle, a picture carousel, a quiz, an audio block with chapters, Up next. Its chrome is not W's: hairline panels, 10.5-12 px labels (measured 8-12 texts under 13 per screen), private buttons | B's blocks and its tile bands stay (B kept, COMPONENT_MAP §2.2); every button, chip, panel and label in them becomes W's component (P3), every block rebuilt on them (P6) |
| S7 | **The card's bottom: nav bar and floating controls** | `sbs_09`, `issues_1` I6 | **W** | W: one floating bar, 64 pt, a 48 pt raised accent disc, 13 pt labels, nothing else floating. B: ✦, Azu, the Locate pin, Up next and the section bar all stack in the bottom third and cover the text (I6); the disc is 34 px, the labels 11 px | W's bar (call 9: 64 / 48 / 13); the section icons and colours from B; ✦ in the disc slot; Azu beside the bar only while he speaks; the Locate pin into the card's action row (S4) |
| S8 | **Settings** | `sbs_05` | **W** | W: a title with an icon, grouped boxes per area (Your visit, Look and reading…), each row with an icon tile, a title and a hint line, segmented controls with ✓, interest chips, the 13 pt floor; it grows at text L. B: one long panel of pills-as-radio at 11 px with 40 pt targets (23 measured under 44), no grouping, and nothing grows at L (I9, `sbs_10`) | W's 5-area Settings (P4); B's groups (the guide, music, tips, games on the tour) become W rows inside "The guide" and "Sound" areas; B's pills become W's segmented and switches |
| S9 | **Explore without the camera** | `sbs_06` | **Both** | **W**: sections (Tours, Games, Moments on the wall), a hero card for the tile album, rows of 56 / 72 pt with an icon tile and a chevron. **B**: each point's **picture** as the row's leading thumb (it reads as places, not features) and the subtitle with the dates | W's sections and row component with B's picture thumbs (COMPONENT_MAP's PROPOSED row leading `picture`); B's points become a "Places" section |
| S10 | **Progress and collection** (W's You and album, B's Contents) | `sbs_07` | **W** | W's You: a profile card with three stat tiles, a progress line, an empty state with a dashed frame, the album as a grid of found tiles with ✓ badges: warm, layered, rewarding. B's Contents is a functional list (minutes, ticks, filter chips) at 12 px | W's You and album as they are; B's Contents rows become W's list rows (trailing minutes and tick, COMPONENT_MAP §2.1 row 8) |
| S11 | **The guide** (B's dock, W's coach tip) | `sbs_08` | **B** | Only B has a guide that tells, reads along, takes questions and talks: the dock at Mini / Open / Full, the teleprompter with the said word highlighted, suggestion chips, the talk button. Its skin is off-system: a mint-ringed Pause in dark, a magenta Pause and a pink-to-blue wash in light (outside the closed accent list), 7-12 px meta texts, and the Tips text white on lavender (I2). W's **coach tip** shows what is right: a bubble with Azu's head and a tail that points at the marker | B's dock as the composite of COMPONENT_MAP §2.2 on W's panels: the app accent for Play and the talk button, W's chips, the 13 pt floor; Ink glass kept as the always-dark option; W's coach-tip bubble for every tip B shows |
| S12 | **The ✦ menu and Ask about the view** | `sbs_08` | **Both** | Only B has them (the functions are right), but their look is not: an always-dark menu, five suggestion chips squeezed into one row with four-line words, the field's placeholder dark on dark, a blue gradient pill (I3). W's glass panel and chip row are the parts they need | W's glass panel over the camera (`--ts-app-glass`), W's chip row that scrolls, W's main button; B's content and actions |
| S13 | **Story and block pages** | `sbs_09` | **B** | B's Story page is editorial (a drop cap, a caption with a credit, See it as chips); the Timeline page is sparse (four rows, then empty space). W has no page of this kind; its top bar is the part to use | B's pages with W's top bar (back 48 pt, B's 30 × 34 measured) and W's chip row for the page chips |
| S14 | **Coming soon** | `sbs_04` | **W** | W only: a place not yet ready is still a promise (a picture, what is on its way, Save for later). B has no such state | Kept from W (P4) |
| S15 | **Text size L** | `sbs_10` | **W** | W grows every text: titles, labels, chips, buttons. B grows paragraphs only: Settings and the hero are pixel-identical at L (the measured minimum stays 10.5-11 px) | W's rule (P1b.2 builds it: every text, steps S 0.9 / M 1 / L 1.2 / XL 1.36) |
| S16 | **Tablet** | `sbs_11` | **W** | W puts sheets in a centred column (about 400 pt) over the wall. B stretches the card and Settings across the whole tablet: 11 px labels on an 800 pt panel | W's column rule for every sheet and the dock; B's Before / After may use the width (a block's choice) |

### 2.2 · Parts

| # | Part | Verdict | What exactly is better | The best of both |
|---|---|---|---|---|
| P1 | **Type** | **Both** | W: eight roles (display 28 … eyebrow 13 caps), one scale, Fraunces for titles, Atkinson for UI, the 13 pt floor. B: the **editorial** roles W lacks: drop cap, lede, quote, a plum section eyebrow, the hero title at 34-40 | W's roles + B's editorial roles as named roles (`--ts-size-dropcap`, `-lede`, `-quote`, `-hero` already in the table) |
| P2 | **Colour and the dark** | **W** | W's dark is layered: page `#100F0D`, panel `#1C1A17`, grouped boxes a step lighter, gold accent; it reads warm and expensive. B's sheets measure **`#0F1012`** (cool, flat, one level) in Premium dark and **`#221D45`** (violet) in Playful dark | Call 10 on every B sheet: Premium `#1C1A17` with boxes `#26231F`; Playful W's navy (`#182050` sheet, `#10173A` page, measured on W) |
| P3 | **Buttons** | **W** | W: five kinds, three sizes, one accent fill per view, 48 pt. B: about 24 styles, several accent fills at once (the hero CTA, the Next pills, the quiz) | W's button; B's "done" morph as an option (COMPONENT_MAP §2.1) |
| P4 | **Chips** | **Both** | W's filter chip (wash + ring + bold, ✓) is the cleanest selected state. B has the **suggestion** chip (questions, answers) W lacks | One chip, kind filter / suggestion (COMPONENT_MAP decision 2) |
| P5 | **Segmented and switch** | **W** | W's segmented with a raised thumb and ✓ reads as one control; B's pills-as-radio read as many buttons | W's |
| P6 | **Panels and glass** | **W** | W: glass over the camera, solid panels for sheets, grouped boxes inside, no shadow on dark panels, a 1 pt rim. B: four panel palettes, gradients, an always-dark default | W's three panel kinds; B's Ink glass and Aurora as options of the dark panels |
| P7 | **Icons** | **W** | W: one line set, 1.5 pt (2 for Family), on a 24 grid, inside icon tiles. B: section icons at 18 px, mixed weights | W's set and tiles; B's 28 section icons redrawn at W's weight (P3) |
| P8 | **Section identity** (tile bands, family colours) | **B** | The azulejo tile band between sections is the app's own signature; the family colours tell where you are | Kept (B kept, P3/P6), tokens `--ts-family-*` |
| P9 | **The mascot** | **B** | B has Azu in 3D (five GLB characters) and a 2D tile-cube face; W has a hand-drawn Azu for Playful only | One Azu: the 3D model everywhere (§2.3), the drawn one as Playful's 2D fallback; the tile-cube face (`sbs_08`, the dock) retired or kept only as Playful's sticker (Q3) |
| P10 | **Touch targets** | **W** | W's kit draws every control in a 48 pt target (`_4.1.7` §4; the script found no W `button` under 44 pt, but some W controls are not buttons, so this is partly a judgement); B measured 23 in Settings, 5-11 per card screen (40 pt pills, a 30 × 34 back, a 29 × 23 send) | W's 48 pt rule, everywhere |
| P11 | **Playful** | **Both** | W's Playful is a kit: 9-slice frames, Fredoka, 2 pt outlines, hard shadows, the navy dark: it looks drawn by hand. B's Playful at HEAD changes the title face and the colours only. W's frames have one flaw (I4) | W's kit (COMPONENT_MAP decision 6), I4 fixed |

### 2.3 · Verdict counts
| | Screens | Parts | All |
|---|---|---|---|
| W better | 7 (S3, S7, S8, S10, S14, S15, S16) | 6 (P2, P3, P5, P6, P7, P10) | **13** |
| B better | 3 (S6, S11, S13) | 2 (P8, P9) | **5** |
| Both | 6 (S1, S2, S4, S5, S9, S12) | 3 (P1, P4, P11) | **9** |
| All | 16 | 11 | **27** |

### 2.4 · Motion (judged from what each Lab offers, not from frame captures)
- **W:** durations 120 / 220 / 360 / 500 ms, ease-out-cubic, an expressive spring for Playful, a breathing skeleton, a raised
  disc that slides. **B:** a sheet with four stops and a drag, a hero motion (Ken Burns), a rim sweep on glass sections, the
  Open Row's shine, Azu's hops, the talk button's rings, about 45 private durations.
- **Best of both:** W's four durations and two easings as tokens (P1b.1 has them); B's effects kept as named motions on them;
  reduce motion = whole and still (fixed rule). Unity: `--ts-duration*` in USS transitions (Native); springs and keyframes as C#
  tweens (Build, negligible cost).

## 3 · The "wow" layer
Each proposal: where it appears, then its path. Costs are rough, for a mid-range phone (guess, unmeasured).

| # | Area | Proposal | Where | Path |
|---|---|---|---|---|
| X1 | **Depth and layers** | Three planes, always in this order: the world (camera + wall), the glass chrome (search, bar, tips), the solid sheets. The world dims and the chrome steps back (scale 0.98, 120 ms) when a sheet reaches Full | every screen over the camera | **Native**: UI Toolkit `scale` + opacity transitions; the dim is `--ts-scrim` on a full-screen element |
| X2 | | **Parallax of the chrome against the wall**: a tilt of the phone moves the wall's labels 2-4 pt against the glass bar | Main AR, the card at peek | **Build**: gyroscope → a translate on the label layer (World canvas); negligible |
| X3 | **Glass and light over the camera** | **The camera-blur glass** (`_4.1.7` §3.2): the camera image blurred into a 1/8 render texture under the app glass tint, with the luminance limit | search, nav bar, tips, the On tap card, the dock in glass style | **Build**: dual-Kawase blur on a 1/8 RT, about 0.3-0.6 ms a frame; the Solid tint is the fallback (and Reduce Transparency) |
| X4 | | **Light from the scene**: the glass rim and the On tap card's top highlight take the AR light estimate's colour and direction | the On tap card, the bar | **Native** (ARFoundation light estimation) + **Build** (two USS colour writes a second) |
| X5 | | **A light sweep** on the rim of the focal glass object when it appears (once, 500 ms) | the On tap card, the dock's Open | **Build**: B's `RimSweep` shader graph on a 9-sliced rim image; cheap |
| X6 | **Azu in 3D, and where he lives** | **One Azu, three homes**: (a) **on the wall**, standing on a tile ledge beside the focal marker, turning to point at it (World, anchored); (b) **in the bar's centre disc** as a live portrait (W's slot); (c) **in the dock and bubbles** as a portrait that lip-syncs | (a) Main AR and tours; (b) every screen with the bar; (c) the guide | (a) **Import** GLB + Animator (idle, point, talk, wave), **World**; (b, c) **Build**: one 256 px RenderTexture from a second camera, about 1 ms when he moves, a still sprite otherwise; visemes from B's `mascot/mouth.js` as blend shapes (**Import**, rigged in Blender) |
| X7 | | Contact shadow and rim light on Azu wherever he stands (the first page today floats him on a flat ground) | the first pages, the wall | **Build**: a blob shadow quad + a rim term in his URP shader; cheap |
| X8 | **Motion and transitions** | **Shared element**: the On tap card's photo grows into the sheet's hero (one object, 360 ms, ease-out-cubic); the title moves with it | tap → card | **Build**: C# tween of `translate` + `scale` on the photo element, then a swap; cheap |
| X9 | | **Springs**: sheet stops, the raised disc, chips selected (Expressive for Family / Playful, Standard otherwise) | sheets, bar, chips | **Build**: a C# spring tween (USS has no cubic-bezier, `ease-out-back` for the simple case is **Native**) |
| X10 | | **Staggered entrances**: list rows and option cards arrive 30 ms apart; the tile bands slide in sideways when their section enters | Explore, Settings, onboarding, the card | **Native** USS transitions with per-item delays |
| X11 | **The hero photo** | **2.5D depth**: the hero painting split into two or three depth layers that drift apart slightly while the sheet moves (a "living painting") | the card's hero, Start | **Import** (layers cut once per POI, in an image tool) + **Native** (each layer an element, translated); a single-image depth-map shader is **Build** with a RenderTexture, medium cost (guess) |
| X12 | | **Ken Burns with intent**: the slow drift ends on the detail the title names (a focus point per POI, already B's "crop") | the hero | **Native** (scale + translate tween) |
| X13 | **The wall's 3D objects** | **The lost building rises**: on a place's first tap, its 1755 model grows out of the tiles (a dissolve along the tile grid), then settles as a translucent ghost aligned with the wall | Main AR, the On tap card's 3D action | **Import** GLB + **Build** a dissolve shader on the tile grid (World URP); medium (one model on screen) |
| X14 | | **Then & now on the wall**: the old painting fades in over the live tiles, aligned, with a wipe the visitor drags | Main AR | **Build**: a world quad with an alpha-mask material (World); cheap |
| X15 | | **Markers that breathe**: the focal marker's glow pulses once a few seconds; found places get a gold rim | the wall | **Build**: emissive + a sine in the marker shader (World, glow allowed); negligible |
| X16 | **The card's 3D block** | A turntable in the card (`ModelTurntableBlockView`) with **"Put it on the wall"** handing the same model and orbit to the AR view (merge plan §2: one configuration, three hosts) | the card's 3D block, the On tap 3D action | **Native** RenderTexture in UI Toolkit + **Import** GLB; P5 |
| X17 | **Sound and haptics** | A ceramic "tink" for a tile tapped or found; a soft tick at each sheet stop; a rising two-note chime when the album gains a tile; Azu's voice ducks the music | everywhere, all off with the phone's silent switch and the visitor's Sound setting | **Native** `AudioSource` (sounds); **Import** a haptics plugin (Unity has only `Handheld.Vibrate`; e.g. a package such as Nice Vibrations, guess); **Build** the mapping table (one sound token per event) |
| — | **Rejected (Browser-only)** | CSS backdrop blur in the UI (→ X3); 3D-transformed cards (→ a scale); box-shadow glows on UI (→ 9-sliced sprites); B's drifting Aurora gradients drawn in CSS (→ a shader on the panel's background image, **Build**, or dropped); `color-mix` at runtime (→ resolved tokens) | -- | -- |

## 4 · The issues found
Each with its image and the round that should fix it. Measured values are from the audit script (§0).

| # | Lab | Where | What | Image | Round |
|---|---|---|---|---|---|
| I1 | B | the card at peek | the grabber is drawn over the title; it is also white on the light sheet | `issues_1` I1 | P3 (sheet and grabber) |
| I2 | B | the dock at Full, light | the Tips text is white on a lavender wash (unreadable) | `issues_1` I2 | P3.4 (the dock) |
| I3 | B | Ask about the view | five chips in one row wrap to four lines each; the field's placeholder is dark on dark; a blue gradient pill (off the accent list, gradients) | `issues_1` I3 | P3.4 |
| I4 | W | Playful chips with an icon (Settings, interests) | the icon disc breaks out of the chip's 9-slice frame, so each chip shows a second frame behind it | `issues_1` I4 | P3.2 (the chip's Playful frame) |
| I5 | W | Coming soon, Playful, text L | the main button falls under the fold of the page (scroll needed to see the one action; guess: a sticky foot fixes it) | `issues_1` I5 | P4 |
| I6 | B | the card, every section | ✦, Azu, the Locate pin and Up next float over the text above the section bar | `issues_1` I6 | P3.3 + P4 (the one bar) |
| I7 | B | the card's About section | "TEST · NOT IN THE APP · Demo Components" shows inside the visitor's card | `issues_1` I7 | P3.3 (developer-only, hidden in the visitor's view) |
| I8 | W | onboarding, text L | the eyebrow wraps to three lines beside the Light / EN / Large chips | `issues_1` I8 | P4 |
| I9 | B | Settings | 11 px group labels, 40 pt pills, 23 targets under 44 measured | `issues_1` I9, `sbs_05` | P4 (W's Settings) |
| I10 | B | every screen | text under 13 px: wall labels 11, the hero eyebrow 11, Up next 10.5, the card's section labels 11, Contents 12, the dock's meta 12 (8-12 texts per card screen measured; the ratchet counts 186) | `sbs_09`, `sbs_10` | P3 (the ratchet to 0) |
| I11 | B | every screen | text L does not reach labels, buttons, chips or the hero (the measured minimum is the same at L) | `sbs_10` | P1b.2 (running) |
| I12 | B | every dark sheet | Premium dark `#0F1012` and Playful dark `#221D45`, not call 10's `#1C1A17` and navy | `sbs_06`, `sbs_01` | P3.1 (the palettes) |
| I13 | B | the dock | the app accent is not used: mint ring (dark), magenta Pause and a pink-to-blue wash (light) | `sbs_08` | P3.4 |
| I14 | B | Story and block pages | back button 30 × 34, page chips 29 high | `sbs_09` | P3.3 |
| I15 | B | tablet | sheets and Settings stretch across the whole width | `sbs_11` | P4 (the column rule) |
| I16 | B | the guide | two mascots: the 3D Azu on the wall and a tile-cube face in the dock | `sbs_08` | P5 / Q3 |
| I17 | W | the tapped card | the eyebrow at 11.5 pt and the action labels at 12.5 pt (under the 13 pt floor) | `sbs_03` | P4 (On tap) |
| I18 | W | onboarding's top chips (Light / EN / Text) | labels at 12 pt | `sbs_01` | P4 |
| I19 | B | the Timeline page | four rows then an empty page; the Previous / Next foot floats mid-screen | `sbs_09` | P6 |
| I20 | B | first pages | Azu floats on a flat ground with no shadow; half the page is empty | `sbs_01` | P4 + X7 |

## 5 · The priorities: the 10 changes with the most effect on "premium wow"
| # | Change | Why it matters most | Size (guess) | Round |
|---|---|---|---|---|
| 1 | **One floating bar** (W's, 64 / 48 / 13) replacing B's five floating things; ✦ in the disc, Azu beside it while he speaks, the Locate pin into the card's actions | the bottom third is where the eye rests; today it is the noisiest part of B | M (the nav bar component + the card's bottom) | P3.2-P3.3 |
| 2 | **W's type roles and the 13 pt floor** on every B text, text L reaching all of it | the single biggest "cheap → premium" step; fixes I10, I11, most of I9 | L (the ratchet's 186 sizes) | P1b.2 (L) + P3 |
| 3 | **W's Settings**, with B's guide, music and tips inside its areas | Settings is the screen that looks most like a prototype today | M | P4 (P2 for the guide group) |
| 4 | **The tap → card hand-off**: W's On tap card on the wall, its photo growing into B's hero (X8) | the moment the visitor first touches the wall; it joins the two Labs into one experience | M | P4 + P3.3 |
| 5 | **Call 10 on every B surface**: the warm layered dark and the navy Playful | the dark is half of the matrix; B's flat cool dark undoes W's look | S-M | P3.1 |
| 6 | **The dock on W's panels** with the app accent, W's chips and bubbles | the guide is B's best feature; today its skin fights the app's | L | P3.4 |
| 7 | **Azu as one 3D character in three homes** (X6, X7) | the "wow" and the warmth; one character instead of four drawings | L | P5 (wall, RT) + P4 (bar slot) |
| 8 | **The camera-blur glass** (X3) | it is what makes chrome over a live camera look like an app made for AR | M (a shader + its luminance test) | P4 (Unity work, `_4.1.7` §6.2); the Lab shows it now |
| 9 | **The wall's lost buildings rising in 3D** (X13, X14) | the one effect that tells the app's story (Lisbon before 1755) without words | L (models per POI) | P5 |
| 10 | **Motion tokens with springs, staggered entrances and the shared element** (X8-X10) | motion is what makes "minimal" feel alive instead of empty | S-M | P3 (tokens, components) + P4 (screens) |

## 6 · Questions for the developer
1. **Where does Azu live on the main screen?** W puts him in the nav bar's centre disc; B puts a ✦ guide button there and Azu
   stands on the wall. What it changes: whether the main control of the app is "the guide" (a character) or "the guide's menu"
   (a symbol). **Recommended:** the disc shows Azu's live portrait (X6 b) and opens B's ✦ menu; Azu also stands on the wall only
   during a tour. One character, one control.
2. **Does the tapped place open W's small card first, or B's sheet at once?** What it changes: with W's card the wall stays in
   view and the visitor picks Story, 3D, Viewer or 360 first; with B's sheet the place's story takes the screen at once.
   **Recommended:** W's card first (the AR rule: no full-screen takeover over the camera), its Story action opening B's sheet
   at half with the shared-element move (X8).
3. **The tile-cube Azu face in the dock: keep or retire?** B has two mascots: the 3D Azu and a 2D face drawn on a tile. What it
   changes: one recognisable character, or a second "tile" identity for small places. **Recommended:** retire it in Premium; keep
   it only as Playful's sticker (W's Playful already draws Azu by hand).
4. **The living painting (X11): worth cutting each hero into depth layers?** It costs an image-editing step per POI (about 15
   minutes each, guess) and gives the hero a slow 3D drift. **Recommended:** yes for the five or six showcase places of the user
   study; the others keep the Ken Burns drift (X12).
5. **Sound and haptics (X17): on by default?** A museum app plays in quiet rooms. **Recommended:** haptics on, sounds on but
   following the phone's silent switch, music off until chosen (as B has it).

## 7 · What blocks
- Nothing blocks this review. The B captures are of HEAD `4a6ca97`: P1b.2's profile bar and text steps (running in `src/`)
  will change S15, I11 and P2; the matrix should be captured again when P1b.2 lands.
- Motion was not captured as frame sequences and landscape was not captured (§0): P3's matrix adds both.
