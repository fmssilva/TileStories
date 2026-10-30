# Test Suite Review 1 -- the three tiers, the S4a re-base, the first savings

The first run of `skills/Test_Suite_Review.md` (AUDIT + FIX). Per-fixture times come from `TestRunRecorder`
(`TestEvidence/Harness/fixture_times.csv`, every run); the baseline table is `TestEvidence/Harness/pm_before.csv`.

## 1. Time table

**Before** (HEAD a46bcc2, S4a, full PlayMode, Unity behind / behind): 102 fixtures, 1461 tests, 142 failed in 51 fixtures, 44 skipped,
**89.0 min**. Full EditMode: 2298 tests, 6 failed, 1 skipped, ~3.3 min.

The top 20 hold 61 of the 89 min (68 %); the two Phase A galleries alone 24.5 min (27 %):

| fixture | tests | s |
|---|---|---|
| CardGalleryTests | 392 | 907 |
| CardNavGalleryTests | 157 | 564 |
| PoiCardRestingEndSceneTests | 16 | 256 |
| PoiCardSceneTests | 56 | 249 |
| PoiCardTopElementsSceneTests | 15 | 178 |
| PoiCardNavOptionsSceneTests | 27 | 150 |
| PoiCardPageChromeSceneTests | 28 | 145 |
| PoiCardHeroSceneTests | 34 | 142 |
| SearchSettingsFieldTests | 33 | 127 |
| PoiCardBottomElementsSceneTests | 8 | 108 |
| PoiCardNavSceneTests | 18 | 102 |
| CardStoryGalleryTests | 34 | 98 |
| PoiCardNavContentsSceneTests | 9 | 94 |
| PoiCardMenuSceneTests | 11 | 79 |
| PoiCardInterleavedSpySceneTests | 7 | 77 |
| PoiCardPagesSceneTests | 11 | 76 |
| PoiCardFloatingSceneTests | 10 | 74 |
| SectionSheetSceneTests | 13 | 73 |
| PoiCardFacetSceneTests | 8 | 71 |
| CardStoryMediaGalleryTests | 21 | 69 |

**After** (second session's closing FULL, Unity behind / behind): 102 fixtures, 1339 tests, **0 failed**, 67 skipped, **82.1 min**
(`TestEvidence/Harness/pm_after.csv`). The 122 fewer tests are the gallery cuts (section 5). The 67 skips are 25 findings (section 2's
table) and 42 measurements that run only on demand. Full EditMode: 2298 / 0 failed / 1 skipped, 3.9 min.

| fixture | tests | before s | after s |
|---|---|---|---|
| CardGalleryTests | 308 | 907 | 573 |
| CardNavGalleryTests | 119 | 564 | 404 |
| PoiCardRestingEndSceneTests | 16 | 256 | 244 |
| PoiCardInterleavedSpySceneTests | 7 | 77 | 238 |
| PoiCardSceneTests | 56 | 249 | 222 |
| PoiCardTopElementsSceneTests | 15 | 178 | 219 |
| PoiCardBottomElementsSceneTests | 8 | 108 | 170 |
| PoiCardNavOptionsSceneTests | 27 | 150 | 148 |
| PoiCardPageChromeSceneTests | 28 | 145 | 128 |
| SearchSettingsFieldTests | 33 | 127 | 110 |
| PoiCardHeroSceneTests | 34 | 142 | 69 |

Read with care: the baseline ran 142 reds, and a red stops early, so a fixture that was red then and green now can look slower
(TopElements, BottomElements). InterleavedSpy is slower by design: the walk waits for the stack to rest (section 4). The gallery times
swing ~10-30 % from run to run (CardGallery 573-818 s across this session's three runs).

## 2. S4a's verdict

S4a (a46bcc2) was committed without a full run. Of its 142 PlayMode reds, almost all were tests still pinned to the card before S4a,
not product faults. The five re-base patterns (`__curr_plan_tracker.md`) cover them: the chrome floats over the stack and waits for
the hero; a finger lands only where `CardTestInput.ReachableStack` says; the stack's room is the stop less its top; Full and the hero come
from `SheetStopRule.FullTopGap` / `HeroRule`. Two demo-wall choices had leaked into fixtures (the wall opens at Half; its Use Badge is off):
`SearchSceneFixture` now pins the card container to the framework default as it already pinned navigation, and the marker test owns its
badge switch.

S4a product faults the runs found:
- **Fixed (small, clear):** the landing inset was measured in the sheet's frame and used in the stack's (the stack starts at y 1): a jump
  landed 1 px under the strip and the spy disagreed with the screen at a section boundary (`PoiCardSheetView.RecomputeStops`).
- **F3, not fixed:** the rim light's band after a light / dark switch keeps the old mode's width (dark lights a 3 px band on a 1 px rim).
  A first fix (`CardLight.Resize` reading `--ts-rim-band` for the mode shown) did not change the result and was reverted.
- **F1, not fixed (a design choice):** at Full -- since S4a flush with the screen's top -- the floating placed-model chip lies over the
  card's X (`panel.Pick` at the X = `poi-card-chip-remove`): a visitor who taps X removes the model instead of closing. The two
  PlaceInAr tests that reach it are `[Ignore]` F1 until a block decides where the chip goes at Full (under the bar, hidden, or a gap).
- **F2, contrast:** the gear on S4a's see-through bar measures 6.6:1 (< 7:1); with the hero words (no scrim at the hero's foot,
  1.3-1.5:1) these are the open `_3.3` contrast items of the premium design block: `[Ignore]` with that reason.

Every named skip, its finding and its owner (the Architect's answers, second session). A named `[Ignore]` / `Assert.Ignore` carries the
finding id; there is no Quarantine category.

| skip | where | finding | owner |
|---|---|---|---|
| -- | PoiCardHeroSceneTests.EveryWordOfTheHero_... (16) | FIXED in Premium P1: v5's words shade (ramp + the --ts-scrim-hero floor); 8.29-19.65:1 on the render | -- |
| -- | PoiCardPlaceInArSceneTests (TheLamp_ThePlacedArch..., KeepModelOnSwitch_...) | F1 FIXED in Premium P1: FloatingSlotRule puts the slot under the chrome row when it cannot rest above the sheet; guarded by WithAModelPlaced_AtEveryStop_AFingerOnTheXOrSettingsLandsOnThatMark (panel.Pick at Peek / Half / Full) | -- |
| -- | CardSettingsGalleryTests (the settings mark) | F2 FIXED in Premium P1: the sliders mark in a glass disc, >= 7:1 as seen on the render over a black and a white camera in both modes; over the hero 8.48-15.30:1 on the render (PoiCardChromeMarksSceneTests) | -- |
| -- | CardLightSceneTests.TheRimLight_RidesTheWholeMetalBand_... | F3 FIXED in Premium P1: the band was read back from a style pass that had not run (and the theme resolver held no lengths); CardLight now asks the theme (ThemeResolver.TryPixels) on every paint | -- |
| x1 | PoiCardTapZoomTests.TheLampsTrueFalseStatement_... | F4: vertical and sideways halves never pass in one run since S4a | the next card block |
| -- | CardNavGalleryTests (fab_dial cases) | F5 FIXED in Premium P1: the icon strip's marked-tab glass (two classes) out-ranked the dial pill's surface; the dial's words now asserted at 7:1 | -- |
| x29 | Typeface (12+4), SectionSheet (6), RestingEnd (5), CardLight (1), Story (1) measure tests | not a finding: a measurement, runs when its `MEASURING` file exists | the block that measures |
| x13 | ThemePixelSet gallery (12) + scene (1), `[Explicit]` | not a finding: a pixel-set measurement, run by name before and after a change that must move no pixel | the block that changes the theme |
| `TestSkips.OldNavigationElements` | Always at FIXTURE level: a fixture whose subject is a KEPT old element is skipped whole; a mixed one is split into `XFixture` (abstract: set-up + helpers), `X` (its card tests, on the default bar) and `XOldNavigationTests` (skipped). EditMode, whole: BottomElementsNavViewTests (bar_sheet, next_up), CardMenuOverlayTests, NavBottomElementRulesTests; split: NavElementRegistry, CardLayoutRule, NavTopElementRules. PlayMode, whole: PoiCardTopElements / BottomElements / Menu / Facet / RestingEnd / NavContents SceneTests, CardNavGalleryTests (+ Facets, FullView); split: PoiCardChromeScene, PoiCardHeroScene, PoiCardLayoutScene, PoiCardPagesScene, PoiCardPaintingScene, SectionSheetScene, SectionSheetMeasureScene. REMOVED with the retired elements (_3.3 R1): EditMode AutoNavBlock, BentoLayoutRule, BentoTileRule, EdgeScrubberRule, NavFitRule, NavRailRule, NavigationPresets, NavigationPresetEditorTab, LivePlayModeNavigationPreset, BentoLayoutSheet, BottomBarNavView Tests and the OldNavigation siblings of DetailCardEditorTab, FloatingSlot, NavFloatingRules, SheetDragScene; PlayMode PoiCardTextTabs / BottomBar / Edge / Floating / NavBlock / NavRails / NavBento / BentoLayout / NavOptions / NavCombinations / InterleavedSpy SceneTests and CardNavGalleryTests.RailsAndBento; the retired elements' cases inside the kept fixtures | not a finding: the Navigation Bar is on by default, so the kept elements draw only while it is off; the developer rebuilds them on the bar's pattern (`_3.3` TODOs, [rebuild]) | the block that rebuilds each kept element |

## 3. The Core list (`[Category("Core")]`)

**Tagged and timed (second session):** 48 `[Category("Core")]` attributes, 31 on classes and 17 on methods.
First run, Unity behind both ways, all green:
- PlayMode: 227 tests, 0 failed, 1 skipped (the story measurement, gated on a file), **17.0 min**.
- EditMode: 118 tests, 0 failed, **20 s**.
- CORE total **~17.5 min**, against the brief's 25-30 min target. The Architect accepted ~20 min: widen it only for a missing critical path.
- Per-fixture times: `TestEvidence/Harness/core_first.csv`. Slowest: Floating 87 s, SectionSheet 71 s, LiveUpdate 63 s, BottomBar 61 s, Story 58 s.

Run it with `run_tests` `category_names: ["Core"]`, PlayMode then EditMode. The class-level tags are on these 31 fast fixtures:
PoiCardSheetDragSceneTests, PoiCardThemeSceneTests, PoiCardNavBarSceneTests, PoiCardAudioSceneTests, PoiCardStorySceneTests,
MarkerConfigDrivesMarkerTests, ARZoomCameraFovTests, ARZoomRoutingTests, LivingRoomConfigIntegrationTests, ThemePipelineTests,
PoiCardLanguageSceneTests, PoiCardChromeSceneTests, PoiCardTextTabsSceneTests, PoiCardLayoutSceneTests, SectionSheetSceneTests,
PoiCardExpandableSceneTests, PoiCardVideoSceneTests, PoiCardModel3DSceneTests, PoiCardPanoramaSceneTests, PoiCardBottomBarSceneTests,
PoiCardFullViewSiblingsSceneTests, PoiCardLiveUpdateTests, LodRealPipelineTests, OrientationWallSessionIntegrationTests,
DisplacementSettingsRealMarkerTests, PoiCardFullHeightSceneTests, PoiCardFloatingSceneTests, PoiCardPaintingSceneTests, LodWallSceneTests,
SearchDemoTests, MarkerViewRuntimeTests. The method-level tags are on the slow fixtures, by critical path:

- open by a real marker tap, and close (X, swipe down, empty tap): PoiCardSceneTests {ARealMarkerTap_OpensTheCard_ForThatPoi,
  TheX_ClosesTheCard_..., PullingTheCardDownBelowPeek_ClosesIt, ATapOnEmptyCameraSpace_Closes_...}
- stops and drag: PoiCardSceneTests.DraggingUp_GoesToHalf_AtMost40Percent_ThenFull (+ PoiCardSheetDragSceneTests, class)
- the hero: PoiCardHeroSceneTests {AtFull_TheWholeHeroIsTheScreen_..., ARealTapOnTheHerosAction_...}
- the block page, Back and Next / Previous: PoiCardPageChromeSceneTests {TheStoryPage_ARealTapOnBack_..., TheFoot_RealTapsOnNextAndPrevious_...}
- navigation (a jump, the spy follows a real finger): PoiCardNavSceneTests {AJumpFromPeek_..., ARealFingerDrag_MovesTheSpy_...}
  (+ PoiCardNavBarSceneTests, class)
- search (a typed query, a marker tap, select and filter): SearchSceneTests {ATypedQuery_..., ARealMarkerTap_Selects_...,
  SelectingAndFiltering_NeverUndoEachOther}
- the Editor (EditMode): LiveSyncFieldMatrixTests.LiveEdit_ReachesTheRunningMarker (its rows, 116 cases),
  CardConfigRoundTripTests.EveryCardField_WalkedByReflection_SurvivesAJsonRoundTrip,
  VisitorStateIsolationTests.ThisRun_KeepsTheVisitorsStateInAStoreOfItsOwn_NeverTheAppsPlayerPrefs

## 4. Findings

- **The galleries are the cost.** CardGalleryTests (907 s) and CardNavGalleryTests (564 s) are exhaustive Phase A matrices over every
  entry x stop (x language); they are a BLOCK-tier bottleneck for any card block that touches a block's look.
- **Fixture-local copies of input helpers drift.** Nine fixtures had their own `DragStack`, `InViewAndStill` or tap-into-view, each
  rewritten when the chrome changed. `CardTestInput.IntoReach` / `TapInReach` are now the one way to put a finger on the stack, and
  they wait until the stack is still (a set offset never stops a running fling).
- **Assert messages that name the setup** (the X's picked element, the two landing insets) turned two hour-long guesses into one run.
- **F4:** `PoiCardTapZoomTests.TheLampsTrueFalseStatement_...` -- since S4a the vertical and the sideways halves never pass
  in one run under real Input System fingers; `[Ignore]` with that name, owner: the next card block.
- **Watched flakes, second session:**
  - **The doubled DetailCard reds -- cause found and fixed at the source.** `PoiEditorWindowHost` sent the POI Editor's rect probes
    through two static hooks, so any other POI Editor that painted wrote its rects over the test's: a host an earlier test left open
    (`Host.Editor` is static, so it painted the current test's editor at its own screen place), a real POI Editor window left open, or
    the developer's own POI Editor. The probes now record only while this test's host paints, and a leftover host paints nothing. The
    constructor closes leftovers, and `Close` (called in every user's TearDown) fails the test that left one open.
  - No test leaks one today: a code search found no test that opens two hosts, and full EditMode is 2298 / 0 with the check on. The
    doubling could not be reproduced, so the fix is proven by mechanism, not by a red that turned green.
  - **InterleavedSpy walk:** its drags now wait on `CardTestInput.UntilStill` (three frames unmoved), not `Settle(0.6)`. With the fling
    running to its end, a 20 % step passed Listen (one block) between two readings: the step is now 8 % of the viewport. Green; 102
    drags, 240 s (was 160 s).
  - **`PoiCardSceneTests.ARealTapOnTheStickyAction`:** green in this session's closing FULL (1339 / 0 failed); still watched.
  - **`ConfigUndoGroupingTests` slider drag:** green in all four full EditMode runs; still watched.
- **P3c grew two card fixtures with render tests** (Words On Picture, Raised Depth, Icon Colour): PoiCardHeroSceneTests 34 -> 58 tests
  (~242 s), PoiCardNavBarSceneTests -> 47 tests (~486 s; its class Core tag now carries the depth and icon renders too, ~3 min of CORE);
  LivePlayModeNavigationBarTests +1 (enters Play Mode, ~25 s, EditMode suite). A PlayMode run is refused while the Editor is in Play
  Mode: check `editor_state` `play_mode` before `run_tests` (a run was refused once while the developer had pressed Play).
- **P3d** added PoiCardLocateFabSceneTests (9 tests, ~63 s; a card fixture of BLOCK from now on) and LivePlayModeCardSectionsTests (enters Play
  Mode, ~14 s, EditMode suite); PoiCardSectionsSceneTests 33 tests (~367 s: real-stroke Reveal walks, Tint Depth renders). Watched: the live
  test's real Tint Depth drag took 2 undo steps once in a full EditMode run (1 in every run of its own); it now records the count
  (`TestEvidence/Editor/live_tint_depth_drag_steps.txt`) and walks Ctrl+Z by it -- the grouping's fixture, ConfigUndoGroupingTests, stays green.
  The bar labels' ratios are written fresh per case (`labels_ratios_<position>_<mode>.txt`): the old appended file mixed runs.
- **P3e** rewrote PoiCardLocateFabSceneTests for the pin (17 tests, ~240 s: every Style x Colour rendered light / dark over the paper and the
  hero) and added PoiCardBrightPageSceneTests (4 tests, ~90 s: 30 Start x Colour x mode ground renders) -- both card fixtures of BLOCK from
  now on -- and CardLocateEditorTabTests / LivePlayModeCardLocateTests (enters Play Mode, ~19 s) to the EditMode suite. SHARED GROUND touched:
  `CardTestInput.HoldTheWallStill` / `HoldSectionsStill` now also freeze the Locate Button's float -- a still render needs every clock held, and
  a floating pin made every `GrabStill` of the card wait out its 29 frames ("the picture never held still"). A new moving part of the card
  joins those two helpers the same day. The pin then moved to its own layer over the card: PoiCardLocateFabSceneTests 25 tests (~300 s; the
  hero words unchanged with the button off, the float inside its target at Size 0.6 / 2, the halo's edge or the disc >= 3:1 on every hero picture
  light / dark, Half / Full). SHARED GROUND touched: `CardTestInput.ReachableStack` stops above the shown Locate Button and its chip (a finger
  there presses it) -- the NavBar's real strokes started on the chip once the 1x pin lowered it. A still render of a TURNTABLE hero (the Sacred
  Arch turns under Reduce Motion) waits for stillness only where it measures (`PoiCardLocateFabSceneTests.GrabStillAround`).
- **Two silent Painting tests.** `TheTabsWithChipsStrip_...` and `WhereTheChromeHidesTheStrip_...` read the wall's top element through
  `Assume`. When the developer moved the wall to icon tabs, they turned Inconclusive, and Inconclusive does not fail a run. They now
  set tabs with chips themselves, assert it, and are re-based on pattern 2. A wall-choice `Assume` hides a test; this was the only one
  left.

## 5. Fix list (block-sized chunks, expected saving)

Done this session (one case per behaviour; the whole entry matrix stays in the gallery scenes for the eye):
- **CardGalleryTests, 392 -> 308 cases, 818 -> 759 s.**
  - Headers: each look keeps its long texts at every stop and every other content at Full. A dropped short or no-subtitle entry at
    Peek / Half is proven by the long one at that stop, plus its own content at Full.
  - Blocks: each kind keeps every content once (at its first look), and each look keeps its long entry. A dropped (look, content)
    entry is proven by those two.
- **CardNavGalleryTests, 157 -> 119 cases, 584 -> 499 s** (404 s in the closing FULL; CardGalleryTests 573 s). The elements, their menus and the full view keep pt_long, one_family and
  two_families. big is proven by pt_long (same sections, longer words), small by two_families and pt_long. Layouts, nav blocks and the
  per-case tests keep all five.
- **The saving is smaller than planned (~2.4 min, not ~13).** A case's cost is mostly the scene load in `[UnitySetUp]` and the render,
  not its asserts, so cutting cases saves little per case. The next lever is the scene loaded once per fixture (chunk 1 below).

Still to do (each a block):
1. **Gallery scene once per fixture (CardGalleryTests 759 s, CardNavGalleryTests 499 s, CardStoryGalleryTests 98 s):** load the scene in
   a `[UnitySetUp]` guarded by "not loaded yet" and reset the harness between cases (`Show` already rebuilds the card). Measure first: one
   fixture's per-test durations (`include_details`). Expected ~5-8 min if the load is half of each case.
2. **PoiCardRestingEndSceneTests (256 s) + PoiCardSceneTests (249 s):** the resting-end strokes wait on `UntilStill`, not seconds; the
   wall scene once per fixture where tests share no state. Expected ~3-4 min.
3. **PoiCardTopElementsSceneTests (178 s), PoiCardNavOptionsSceneTests (150 s), PoiCardPageChromeSceneTests (145 s):** `Settle(n)` waits
   replaced by conditions (`UntilStill`, the sheet's stop height), each fixture timed before and after. Expected ~2-3 min.
4. **Shared input helpers:** the fixture-local DragStack copies (Chrome, BottomBar, BottomElements, Menu) onto one CardTestInput drag that
   starts inside `ReachableStack` and ends on `UntilStill`. No time saving; removes the flake family.
5. **PoiCardInterleavedSpySceneTests (238 s, was 77 s):** the walk's 8 % steps each wait for rest. Step by the stack's next block
   boundary instead of a fixed share of the viewport (one drag per block, not per 8 %). Expected ~2 min.
