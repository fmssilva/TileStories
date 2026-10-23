# Plan tracker -- _3.3 P4.3: the app's fonts (Fraunces + Atkinson Hyperlegible Next)

Brief: `__AI_worker.md` (P4.3, R16 row 1, R12.3). Worker: Opus-class. Only the FACES change (sizes / colours / layout: P4.4).
Do not commit (the Architect does). Before finishing: `git checkout -- Assets/Dev/StyleSpike/Fonts` (tests rewrite them).

## GATE (done)
- [x] push: 0 ahead (HEAD c558d0b = origin/master)
- [x] UnityMCP: telemetry_status ok, console clean of compile errors
- [x] captures opened: `TestEvidence/Fonts/gate_gameview.png` (Game view: wall, "Ribeira Palace" marker),
      `TestEvidence/Fonts/gate_poieditor.png` (POI Editor, Global Scene; right ~60 px = VS Code icon bar, no private content); 0 windows left
- [x] compile: 0 `error CS` in Editor.log, last Tundra build success; marker SDF + spike fonts: git clean -> nothing restored
- [x] baseline: none (HEAD c558d0b; P4.1 full PlayMode 1233/1235 two known reds, EditMode 2045/2046 focus flake)

## Findings (verified on disk)
- No USS in Framework / Apps sets a font (only Dev/StyleSpike): the card draws Unity's default runtime theme font today.
- No atlas builder in the repo (P0 built its assets ad hoc; the fontTools script was outside the repo).
- TextCore FontAsset (6.3): CreateFontAsset(Font, size, pad, mode, w, h, population, multiAtlas), TryAddCharacters, public ClearFontAssetData;
  fontWeightTable has no public setter (-> SerializedObject m_FontWeightTable).
- Title rules carry `-unity-font-style: bold` -> Fraunces (SemiBold only) would get FAUX bold: display asset's table [7] = itself.
- USS has no medium style: Medium only through the weight table (rich text font-weight=500). No role uses it before P4.4.
- `LabelFontKeyOptions` (Editor) hard-codes the 3 marker keys; `LabelsAndFontsAuthoringTests` pins 3.
- `ShippedTextGlyphTests.ShippedCharacters` lives in the test assembly; the builder needs the same list -> move it to Editor code.
- Italic (`About.uss` quote) stays synthesized: no Atkinson Italic in the brief -> `__mixed_TODOs.md`.

## Decisions
- Fonts: `Runtime/UI/Theme/Fonts/<Family>/` (.ttf + OFL.txt + README provenance), assets `<name> TMP.asset` / `<name> UITK.asset` beside.
- Library: `FontKeyLibrary.Entry.uiFont` (TextCore, optional); keys fraunces_semibold, atkinson_next_regular / _medium / _bold.
- Atlases: static, SDFAA, sampling 48, padding 6, 1024 x 1024; set = ASCII + Latin-1 + Latin Ext-A + typographic punctuation + shipped chars.
- Weights: the theme's type roles name the keys; the atlas command wires each UITK asset's weight table from them (display [6], [7] = itself).
- Type roles in `ThemeAsset.type`; generator emits them (USS var if a real panel resolves it, else direct url rules); display selectors = data.

## Steps
- [x] 4.3.0 Measure first: `TypefaceMeasureTests` (gated by TestEvidence/Fonts/MEASURING, phase in the file): 12 P4.1 gallery renders +
      Lamp / Painting x EN / PT x Peek / Half / Full sheet crops + stops / header heights -> `TestEvidence/Fonts/before/`. Run "before".
- [x] 4.3.1 Download google/fonts variable TTFs + OFL; cut static instances (fontTools); README; into Runtime/UI/Theme/Fonts/.
- [x] 4.3.2 FontKeyLibrary {key -> TMP + TextCore}; FontLibrary.asset + 4 keys; Editor key list 7; tests (library, authoring).
- [x] 4.3.3 ShippedTextCharacters (Editor) + FontAtlasBuilder (menu command) -> 8 static assets; weight tables; ShippedTextGlyphTests grows
      (every shipped + content char in all 8; ranges); mutation proof.
- [x] 4.3.4 Probe var() font on a real panel; ThemeAsset.type + generator + regenerate; font guard test; `ThemeTypeTests` (PlayMode, real
      scene: body / display everywhere, bold = Bold face not faux, display bold = no faux); mutation proofs.
- [x] 4.3.5 Targeted fixtures (Card / Poi / Search / Theme / Marker); re-base list below (old -> new, from source); findings.
- [x] 4.3.6 "after" measurement + side by side + glare render; look item by item.
- [x] 4.3.7 Full PlayMode (background), full EditMode.
- [x] Part 2 docs: 10-structure, _2.0, _3.3 R12 Build log, 40-testing RUN TIMES, __mixed_TODOs.
- [ ] Finish: restore spike fonts, LivingRoom scene open, final report (60-finishing 6.4).

## Measurements (before -> after) -- TestEvidence/Typeface/{before,after,compare}/
- lamp en: peek 195 -> 168, half 337.6 = , full 796 = ; header 206 -> 180; peek part 64 = ; title 59 -> 30 (one line)
- lamp pt: peek 195 -> 198; header 206 -> 210; title 59 -> 60
- painting en / pt: peek 102 -> 104; header 113 -> 116; peek part 65 -> 67; title 30 =
## Runs
- targeted (regex) 1132: 1126 + 6 red (3820 s) -> 4 re-based, 38/38 + 11/11 green; 2 = the Menu finding (left red)
- full PlayMode 1256 results / 1254 tests, 3577 s, background at every poll: 2 red = the Menu finding
- full EditMode 2050/2050 (149 s, focused)

## Re-base log (old -> new, source)

## Findings during the build
- (4.3.3) The GATE's POI Editor capture (LoadConfig) dirtied LivingRoomScene with the Edit-Mode rig; the MCP test run then SAVED it (+23k
  lines). Restored with git checkout (Unity had it open, not dirty) and reopened. Next capture: reopen the scene without saving after it.
- (4.3.4) var() CAN carry a font definition (probe: label by var = direct url = inherited child = Fraunces UITK) -> generator emits
  --ts-font-body / --ts-font-display tokens + `-unity-font-definition: var(...)`.
- (4.3.4) Mutations: About.uss font rule -> guard red; .card-block-heading dropped from the generated sheet -> 41 headings red;
  weight [7] emptied -> body bold 387 vs 361 / title 232 vs 204 red; all restored -> green. Widths: regular 337, bold 361 = Bold cut,
  medium 346 = Medium cut, title "St George's Castle" @24: 204 both.
- (4.3.6, early after-measure) FINDING: Lamp PT at Half loses the sticky CTA -- header 206 -> 210, room 131.6 -> 127.6 under
  CardChromeRule.MinStackViewportUnderFooter (24) after the footer. Not re-based; reported (the "Half is mostly header" TODO, P4.4/P5).
- FINDING: Peek moves: Lamp EN 195 -> 168 (title one line), Lamp PT 195 -> 198, Painting 102 -> 104 (Half / Full unchanged).
- FINDING (design): Atkinson's slashed zero in dates/times ("1500", "0:00"); the face has no plain-zero alternate (zero, zero.tf only).
- FINDING (design, P4.4): display headings (Fraunces 600 @18) read lighter than Atkinson Bold values beside them.

## Re-base log (old -> new, from source) -- targeted set 1132: 1126 passed, 6 red (3820 s)
- PoiCardEdgeSceneTests.TheStops_AreUnchangedWithTheScrubber_...: typed 5a standard 195 / 337.6 / 796 -> the card's own stops measured
  with edge None first (Peek 168 now: one-line title).
- PoiCardFloatingSceneTests.TheStops_AreUnchangedWithEachFloatingButton_...: same (own stops measured with None).
- CardVideoGalleryTests.LongChapters_...: "chip taller than touch + 1" stood for "wraps"; Atkinson's two lines made a 2-line chip exactly
  49 -> the wrap measured itself: the chip's words at its width > 1.5 lines (37 px vs one line 19; control: "0:00  Gate" < 1.5 lines).
  (A first try lengthened the fixture name on a wrong diagnosis -- the render showed it already wrapped; reverted, fixture unchanged.)
- PoiCardPaintingSceneTests.WhereTheChromeHidesTheStrip_...: precondition "Half hides the strip" was an accident of header height (the new
  faces gave The Painting's Half the room: FINDING, the strip shows at Half now) -> measured at Peek, where the rule always hides it
  (same class path, CardChromeView.TopShownClass).
- NOT re-based (developer, 2026-10-23: "leave red, report"): PoiCardMenuSceneTests.TheLampsStops_DoNotMove_WithAMenuSet_... and
  PoiCardFacetSceneTests.TheLampsStops_DoNotMove_WithTheFacetChipsInTheMenu: a Menu moves The Lamp's EN Peek 168 -> 198 (the menu button
  narrows the title column; "St George's Castle" wraps). FINDING for P4.4 / P5 header.
