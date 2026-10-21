# Plan tracker -- _3.2 step 17b (the Card Starter, the demo point, the Preset row first)

GATE (done): UnityMCP answered (telemetry_status; execute_code needed its tool group `scripting_ext` activated); 1 commit pushed
(50c6723..ae1222f); captures `TestEvidence/Gate/gate_3_2_step17b_{gameview,poieditor}.png` opened (a first POI Editor capture showed VS Code:
deleted, window moved over Unity, recaptured; capture window closed, 0 left); tree compiled (Editor.log 0 `error CS`, ExitCode 0); neither
SDF font modified; EditMode baseline 1972/1972 (135 s) on clean ae1222f (trusted). Rides on 15b's full PlayMode 1113/1113.

## Finding at the plan (asked, the developer chose 2026-10-20)
`_3.2` 8.2 says the wall's own preset avoids re-baselining the suite; the 35 card PlayMode fixtures (all `SearchSceneFixture`) load the
SHIPPED LivingRoom config, so the wall's config IS what they measure. Chosen: write the preset judged best to the wall AND pin every card
fixture to the FRAMEWORK default navigation in `SearchSceneFixture.SetUp` (beside the LOD pin); the shipped value kept as
`ShippedNavigation` for the test of the wall's own choice. Shared ground -> a FULL PlayMode run at the end (background).

## Decisions (autonomous)
- Card Starter = `CardStarters` (Editor assembly, Specific Marker folder; the NavigationPresets shape): Starter {Label, Suits, Entries};
  Entry {kind, variant, preset Choice values}. Data only: no content is written, nothing copied from the point's name / summary.
- "Start from scratch" = the starter with no blocks (one rule): on an empty point it writes nothing; on a point with blocks it asks like any.
- Keys: new blocks never reuse a replaced block's key (NextFree over old + new).
- Row: Specific Marker > Card Content, FIRST row, "Card Starter" popup-styled button (reads the matching starter, else Custom / None).
- Picker: 17a's NavigationPresetPickerPopup becomes ONE shared named-option picker used by both (uniform pattern), each with its own kind.
- Point with blocks: ONE EditorDecision naming the point and the count (pure builder), confirm "Replace N Blocks"; apply = ONE ApplyPopupEdit.
- Tests that use The Painting as "a point with no card" move to The Camera (same shape).

## Steps
- [x] 1. Part 1: Preset row FIRST in Navigation + Editor test (order) + recapture 300_CardNavigation_Preset_620.png
- [x] 2. Shared picker (NamedOptionPickerPopup), 17a tests green
- [x] 3. CardStarters data + Apply + Tier 0 tests (CardStartersTests 6, key rule proved load-bearing)
- [x] 4. Card Starter row + picker + EditorDecision + real-window tests (CardStarterEditorTabTests 6) -- capture pending (step 7b)
- [x] 5. Sets judged on the real card: Rich + Wall Locator, Simple + Related (both read the point itself: zero authoring, lead on)
- [x] 6. Fixture pin in SearchSceneFixture + ShippedNavigation; EditMode twin ShippedWall.WithFrameworkNavigation (6 files);
        LivePlayModeNavigationPresetTests starts from the wall's own preset
- [x] 7. Demo content through the real window (Rich point starter on The Painting + hand content EN/PT + Zoom Image + Wall Locator;
        Residential + Icon Grid / Contents / Rails with headings; wall preset Rich point); Save + Copy; 3 tests re-pointed to The Camera
- [x] 7b. Captures: Card Starter row + picker (Editor/310, 311), judged
- [x] 8. Phase B: PoiCardPaintingSceneTests 7/7 + renders judged. FOUND + FIXED (both test-first, red -> green):
        (a) icon_tabs_chips' emptied chip row collapsed (103 -> 55 px) -> layout loop "Layout update is struggling" (USS min-height);
        (b) its chip row's inline Visible beat the hidden strip -> chips floating over the title at Half (inherit instead)
- [x] full EditMode 1984/1984 (94 s, background)
- [x] 9. 2.3 finding: names / summaries the fixtures read, listed in __mixed_TODOs (nothing renamed)
- [x] 10. Full EditMode 1984/1984; targeted PlayMode 945/945 (45 fixtures); FULL PlayMode 1130/1130 (3428 s, background ~20 min then front)
- [x] 11. Docs: _3.1 (8.5 + fixture), _5.1 (rows, popups), _3.2 (17b row, TODO, 8.2 corrected, 5.1 corrected, 7, 8.1), 10-structure,
        40-testing (pin rule + RUN TIMES), __mixed_TODOs
