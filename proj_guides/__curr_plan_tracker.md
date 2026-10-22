# Plan tracker -- Domain Review _3.2 (POI Card Navigation), mode FIX, chunks D + G (2026-10-22)

Brief: Domain_Review FIX, audit `_3.2.1_Audit.md` section 11, chunks D and G ONLY (the domain's last code block). A refactor: NOTHING
changes for the visitor; no test changes an expectation (STOP if one has to). Trusted commit: 110d07b (pushed at the gate).

## GATE
- [x] UnityMCP answered (telemetry_status); .clinerules 00-60 read (10-structure.md searched: 331 KB), Domain_Review.md, audit F3/F4/7/11
- [x] `git push` (a6baedb..110d07b)
- [x] no SDF font modified (git status of TileStories/: clean)
- [x] compile: 0 `error CS`, Tundra build success
- [x] full EditMode baseline 2018/2018, 104.5 s (no 880 pt failures this time)
- [x] captures opened: TestEvidence/Gate/gate_3_2_fix_dg_poi_editor.png (window closed, none left), gate_3_2_fix_dg_gameview_ui.png
- [x] stops BEFORE: The Lamp in LivingRoomScene at 390 x 844: 195.00 / 337.60 / 796.00 (Game view size restored after)

## What PoiCardSheetView owns today (585 lines)
1. building + wiring 12 parts (constructor) -- WIRING, stays
2. card lifecycle Show / BindNav / Hide -- stays, minus the full-view reopen decision
3. height state: SetStop, RecomputeStops (peek measure), ApplyHeight, custom style -- the sheet's core, stays
4. the vertical drag: pointer capture on grabber + header, the strip's SwipeGrab, velocity on a Clock, snap / close -> SheetDrag
5. the full view's assembly: OpenPages, OpenFullView (siblings + the alone fallback + start page + chip words / icons), the crumb
   section + its tap, the bento section page, the same-point reopen across a rebind, the full view's event wiring -> CardFullViewOpener
6. IBlockHost surface + Back's dispatch + TapOutside -- the sheet's role (applies rules), stays; OpenTakeover / OpenFullView delegate
7. header buttons (X, language, menu) -- view drawing of rule results; NOT split (would add forwarding for 3 buttons + 1 event)
Out: CardLayoutRule.FirstShown (audit chunk D) needs deleting test assertions: not this block (brief: no test edits) -- reported.

## D1 SheetDrag (Runtime/UI/Cards/SheetDrag.cs, 114 lines)
- [x] moved verbatim: pointer handlers, strip SwipeGrab, Begin/DragTo/End, velocity, Clock, IsDragging; release -> Snap -> SetStop / RequestClose
- [x] sheet: Drag property; internal ApplyHeight + RequestClose; Hide -> Drag.Forget(); CardTestInput uses Drag.Clock (2 lines, no expectation)
- [x] PlayMode PoiCardSheetDragSceneTests 5/5 (follows exactly; slow vs flick; clamp + close; strip claims late)
- [x] mutations: speed ignored -> flick case red (Half expected, Peek); no clamp -> 996 for 796 red; restored

## D2 CardFullViewOpener (Runtime/UI/Cards/CardFullViewOpener.cs, 137 lines)
- [x] moved verbatim + the full view's event wiring; KeepAcrossRebind / ReopenAfterRebind (keys cleared on use)
- [x] sheet: FullViewOpener property; IBlockHost OpenTakeover / OpenFullView delegate; internal Context. Sheet 585 -> 421 lines
- [x] EditMode CardFullViewOpenerTests 3/3 (deleted block not reopened; lightbox not reopened, block is; closed card / out-of-map section)
      ("a remembered key never leaks" dropped: KeepAcrossRebind always overwrites, so no break could make it red -- a test that proves nothing)
- [x] mutations: no missing-key guard -> deleted-block test red (+ old BlockFullViewTests); no upper bound -> out-of-map test red ONLY (a real gap)

## G LayoutStruggleGuard (Tests/Runtime/LayoutStruggleGuard.cs)
- [x] finding: the collectors unsubscribe BEFORE the TearDown unload; the audit's 17 lines sit between "[WallSession] Ready" and the unload
- [x] guard listens from SetUp's load to the end of TearDown's unload; fails naming the test, the scene load # and the phase
- [x] in SearchSceneFixture (+ LivingRoomCardSceneTests through it), LodWallSceneTests, SizeComparisonGalleryTests
- [x] LayoutStruggleGuardTests 2/2 (Finish() = End without Assert.Fail, so the test reads the words)
- [x] mutation M5: a struggling line during SearchSceneFixture's unload -> all 5 drag tests red in TearDown, naming test, load #, phase; restored

## Visual + stops
- [x] stops AFTER 195.00 / 337.60 / 796.00 (same as GATE); captures TestEvidence/Card/FixD_Lamp_{Peek,Half,Full,FullView}_390.png judged

## Closing
- [x] full EditMode 2021/2021; targeted set 960/960 (3292 s, background); stops AFTER 195.00 / 337.60 / 796.00
- [x] FULL PlayMode #1: 1191/1192 (3686 s, background) -- PoiCardTopElementsSceneTests icon_tabs drag: probe proved the stack still glides after
      Settle(0.6) in 30/30 drags; fixed with StackAtRest (wait on a condition); fixture 15/15
- [x] FULL PlayMode #2: 1192/1192 (3311 s; unfocused first poll + end, Unity in front ~19-47 min); closing full EditMode 2021/2021 (105.4 s)
- [x] docs: audit (D + G FIX STATUS + the timing finding), _3.2 (rows 19, 19d; sections 5.7, 6.2), 10-structure.md, 40-testing RUN TIMES,
      __mixed_TODOs (GUARDED entry)
- [x] LivingRoomScene open; final report per 60-finishing 6.4
