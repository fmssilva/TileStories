# Plan tracker -- _3.2 step 12 (compact row, expandable for every kind, open in full view)

GATE (done): pushed 1 (2578283..9941a73); Game view + POI Editor (Detail Card > Navigation) captured and opened
(`TestEvidence/Gate/gate_3_2_step12_{gameview,poieditor}.png`, capture window closed, none left); 0 error CS; SDF fonts clean;
EditMode 1874/1874 (75 s, focused) on clean 9941a73. Rides on 11a's full PlayMode 978/978.
PART 1: none open.

Decisions (autonomous): header excluded from expandable (pinned title); pinned-top / footer looks draw inline whatever their display
(`BlockDisplayRule.ShowsCompact`); compact row never loads media (clip kinds show format only); words = heading -> default heading ->
first words (BlockSummaryRule, moved from Editor) -> kind words; collapse pauses the block's playing visitor video
(ICollapsibleBlockView), audio plays on; full view = second pooled view, `BlockBindContext.FullView` (preview own slot, video no
full-screen offer); same-POI rebind reopens the full view and keeps expanded rows; peek / show-on-wall / jump close the full view;
expanded row header holds the full-view button; Editor Display popup names its options, every kind but header.

## 12.1 Compact row words
- [x] `BlockSummaryRule` (Runtime/Blocks) = the Editor's CardBlockSummary/OneLine moved; Editor + BlockRef drawer call it; tests moved
- [x] `CardNavModel.FormatLabelKey` public (the one format -> words mapping)
- [x] `CompactRowRule` (Runtime/Blocks/Navigation): heading / format key / minutes / icon + colour token
- [x] Tier 0 `CompactRowRuleTests` (+ summary tests) EN / PT / fallback; EditMode green

## 12.2 Expandable
- [x] `expandable` in DisplayModes of every kind but header (+ app size_comparison); `BlockDisplayRule.ShowsCompact`
- [x] `BlockViewPool` extracted; BlockStackView uses it (frames pooled apart)
- [x] `CompactRowView` (the slot logic stayed in the stack: no ExpandableSlot class); stack Expand / Collapse / IsExpanded / counts; expanded kept on same-POI rebind
- [x] `ICollapsibleBlockView` (video pauses its visitor video)
- [x] CardStrings rows (en + pt), USS tokens/classes
- [x] Editor: named Display popup on every kind but header, help, guides; Editor test (real click + Ctrl+Z)
- [x] EditMode: every kind lists expandable; ShowsCompact; stack acceptance (The Lamp all expandable: 0 views / 0 media in the scroll)

## 12.3 Full view
- [x] `TakeoverView.WhenPageLeaves` (run before the scope is released)
- [x] `BlockBindContext.FullView`; PreviewBlockView / VideoBlockView read it
- [x] `BlockFullView` (sheet-owned) + `IBlockHost.OpenFullView` (all implementers)
- [x] Sheet: reopen on same-POI Show; close on ShowHeaderAtPeek / ShowOnWall / Nav.Jumped
- [x] EditMode tests: leave hook order, full view binds a second pooled view, closes and returns it

## Tests (PlayMode)
- [x] Phase A `CardGalleryTests.Expandable`: every registered kind (not header) compact + expanded, real taps, renders
- [x] Phase B `PoiCardExpandableSceneTests`: real tap expand / collapse (owner asserts), Lamp all-expandable counts, audio / video /
      preview / AR hand-over and back, language switch / live edit / reduce motion with an open full view, POI switch closes
- [x] Captures 390 EN + PT: compact rows, one expanded, the full view
- [x] Editor capture: Card Content Display popup (`Editor/250_CardContent_Display_620.png`)

## Close
- [x] Full EditMode 1889/1889; Card / Poi targeted 823/825 -> fixed + reveal-path set 153/153; FULL PlayMode 1031/1032 (layout-struggling intermittent, passes alone: logged OPEN on the developer's call)
- [x] Docs: _3.2 row 12 + proof + TODO removed (+ step-13 note: seen tick on collapsed rows); _3.1 5.4 / takeover; _5.1; 10-structure; RUN TIMES

Notes: full EditMode 1889/1889 (77 s); Phase B fixture 7/7 (green after the InViewAndStill precondition); preview mutation
proved load-bearing (branch removed -> 4 slots instead of 5, red; restored). Findings logged: two full-screen buttons on an open video row
(_3.2 TODO), a collapsed row counts as seen (_3.2 TODO step 13), lost tap after a long ScrollTo (__mixed_TODOs Test harness).
