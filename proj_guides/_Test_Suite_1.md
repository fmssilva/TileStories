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

**After (this block, fixture by fixture -- no FULL run yet):** every fixture red in the baseline was re-run green after its re-base, in
background BLOCK-tier groups (behind / behind each); full EditMode 2298 / 0 failed / 1 skipped, 3.5 min. The galleries timed again:
CardGalleryTests 818 s, CardNavGalleryTests 584 s (the run-to-run spread is ~10 %: 608 / 483 s in another run). Named skips that stand for
findings, not passing tests: Hero words contrast x16, F1 x2, F2 x1, F3 x1, F4 x1, F5 x4 (fab_dial cases). The FULL time after is the next
session's closing run.

## 2. S4a's verdict

S4a (a46bcc2) was committed without a full run. Of its 142 PlayMode reds, almost all were tests still pinned to the card before S4a,
not product faults. The five re-base patterns (`__curr_plan_tracker.md`) cover them: the chrome floats over the stack and waits for
the hero; a finger lands only where `CardTestInput.ReachableStack` says; the stack's room is the stop less its top; Full and the hero come
from `SheetStopRule.FullTopGap` / `HeroRule`. Two demo-wall choices had leaked into fixtures (the wall opens at Half; its Use Badge is off):
`SearchSceneFixture` now pins the card container to the framework default as it already pinned navigation, and the marker test owns its
badge switch.

S4a product faults the runs found:
- **Fixed (small, clear):** the landing inset was measured in the sheet's frame and used in the stack's (the stack starts at y 1): a jump
  landed 1 px under the strip and the spy disagreed with the screen at a section boundary (`PoiCardSheetView.RecomputeStops`). The rim
  light's band after a light / dark switch kept the old mode's width (dark lit a 3 px band on a 1 px rim): `CardLight.Resize` now reads
  `--ts-rim-band` for the mode shown.
- **F1, not fixed (a design choice):** at Full -- since S4a flush with the screen's top -- the floating placed-model chip lies over the
  card's X (`panel.Pick` at the X = `poi-card-chip-remove`): a visitor who taps X removes the model instead of closing. The two
  PlaceInAr tests that reach it are `[Ignore]` F1 until a block decides where the chip goes at Full (under the bar, hidden, or a gap).
- **F2, contrast:** the gear on S4a's see-through bar measures 6.6:1 (< 7:1); with the hero words (no scrim at the hero's foot,
  1.3-1.5:1) these are the open `_3.3` contrast items of the premium design block: `[Ignore]` with that reason.

## 3. The Core list (`[Category("Core")]`)

Not tagged yet (the next session's first sub-step): the list -- class level on 31 fast fixtures (~14.6 min at baseline), method level on
the slow ones -- is in `__curr_plan_tracker.md` NEXT 1, by critical path: open by a real marker tap and close (X, swipe down, empty tap):
PoiCardSceneTests; stops and drag: PoiCardSceneTests.DraggingUp_..., PoiCardSheetDragSceneTests; the block page and Back:
PoiCardPageChromeSceneTests.TheStoryPage_ARealTapOnBack_...; the Navigation Bar (jump, the group follows the scroll): PoiCardNavBarSceneTests,
PoiCardNavSceneTests.ARealFingerDrag_MovesTheSpy_...; the hero and light / dark: PoiCardHeroSceneTests.AtFull_..., PoiCardThemeSceneTests; the
story and its page: PoiCardStorySceneTests; audio play / stop: PoiCardAudioSceneTests; markers, search, zoom: MarkerConfigDrivesMarkerTests,
MarkerViewRuntimeTests, SearchSceneTests (3), SearchDemoTests, ARZoomCameraFovTests, ARZoomRoutingTests, LodRealPipelineTests, LodWallSceneTests;
the Editor's live sync and a config round trip: LiveSyncFieldMatrixTests.LiveEdit_ReachesTheRunningMarker, CardConfigRoundTripTests,
PoiCardLiveUpdateTests; the visitor-state isolation: VisitorStateIsolationTests.

## 4. Findings

- **The galleries are the cost.** CardGalleryTests (907 s) and CardNavGalleryTests (564 s) are exhaustive Phase A matrices over every
  entry x stop (x language); they are a BLOCK-tier bottleneck for any card block that touches a block's look.
- **Fixture-local copies of input helpers drift.** Nine fixtures had their own `DragStack`, `InViewAndStill` or tap-into-view, each
  rewritten when the chrome changed. `CardTestInput.IntoReach` / `TapInReach` are now the one way to put a finger on the stack, and
  they wait until the stack is still (a set offset never stops a running fling).
- **Assert messages that name the setup** (the X's picked element, the two landing insets) turned two hour-long guesses into one run.
- **F4 quarantine:** `PoiCardTapZoomTests.TheLampsTrueFalseStatement_...` -- since S4a the vertical and the sideways halves never pass
  in one run under real Input System fingers; `[Ignore]` with that name, owner: the next card block.
- **Watched flakes:** `InterleavedSpy` (the "drag 20" sighting) was a deterministic 1 px landing difference (fixed, product) plus taps on a moving stack
(IntoReach waits until still); `PoiCardSceneTests.ARealTapOnTheStickyAction` (OpenFull read before rest) -- the fixture's Rested waits on the
stop's height, green 56 / 56 once after its re-base (not yet watched over several runs); `ConfigUndoGroupingTests` slider drag -- green in both full EditMode runs, still
watched. The "9 doubled DetailCard navigation" reds never showed (EditMode 2298 / 0 twice); the lead (a static host slot) is in the tracker.

## 5. Fix list (block-sized chunks, expected saving)

1. **CardGalleryTests (818 s) -- one block:** a case per behaviour per block kind (each look once at its worst stop and language), the
   exhaustive entry x stop x language matrix kept as the gallery's renders, not as asserts. Expected saving ~8-10 min. Keep: every contract
   guard (fields, tokens, render targets, contrast) and every past bug's regression.
2. **CardNavGalleryTests (584 s) -- one block:** the five cases cut to the pairs that interact (big x pt_long for fit, two_families for
   collapse), the rest one case. Expected ~5 min.
3. **PoiCardRestingEndSceneTests (252 s) + PoiCardSceneTests (210 s) -- one block:** the scene loaded once per fixture where tests share
   no state; the resting-end strokes wait on the stack's rest condition, not seconds. Expected ~3-4 min.
4. **Shared input helpers:** the remaining fixture-local DragStack copies (Chrome, BottomBar, BottomElements, Menu, InterleavedSpy) onto one
   CardTestInput drag that waits until still. No time saving; removes the flake family.
