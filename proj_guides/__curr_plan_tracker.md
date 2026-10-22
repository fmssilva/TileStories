# Plan tracker -- Domain Review _3.2 (POI Card Navigation), mode FIX, chunks C + F + F6 (2026-10-21)

Brief: Domain_Review FIX, audit `_3.2.1_Audit.md` section 11, chunks C and F plus F6 ONLY. No new owner, no container (STOP if one is
needed: chunk D). Trusted commit: a6baedb (pushed at the gate). Baseline: full EditMode 2003/2007 -- the 4 failures are the POI Editor
window clamped to 849.6 pt (this screen reports 1080 x 1920 at 125 %, the tests pin 880 pt): DetailCardEditorTabTests
ACollapsedBlockRow_..., DefaultMedia_EveryRowsInfoButton_..., HierarchyTableClickTests DeletingALevelStillInUse_..., SpinRingCheckbox_...
(environment, not the code; same 4 expected at the end).

## GATE
- [x] UnityMCP answered (telemetry_status); .clinerules 00-60 read (10-structure.md searched: 330 KB), Domain_Review.md, audit read whole
- [x] `git push` (a6baedb pushed)
- [x] no SDF font modified (git status: only IPCE and proj_guides docs)
- [x] captures opened: TestEvidence/Gate/gate_3_2_fix_cf_gameview.png, gate_3_2_fix_cf_poi_editor.png (window closed, none left)
- [x] full EditMode baseline 2003/2007 (see above)

## C3 (F6) Segmented + Show Counts at 390 px
- [x] pure rule NavFitRule.CountsFit (word + gap + count <= room) + EditMode test
- [x] SegmentedNavView: one decision for the strip, count display off when it cannot fit; the word is never cut
- [x] PlayMode (new fixture PoiCardNavCombinationsSceneTests): whole words at 390 with counts on; counts shown when words are short
- [x] (i) of Show Counts says it; mutation + recapture

## C2 Next Up under Pages
- [x] reproduce red: Pages + Next Up on The Lamp, the top of page 3 (real drag) -> NextBlock not on the shown page
- [x] CardLayoutRule.SpyBlock scoped to the shown section (tabs + pages); NavController bind fallback
- [x] NextUpRule.ShowsPosition (Pages: the pager's counter is the only one); NextUpNavView
- [x] tests (rule + scene), mutations, recapture; guide 5.2 sentence

## C1 Edge scrubber in Pages / Accordion
- [x] decision: not offered there (the layout owns the right edge) + Editor warning naming the layout
- [x] CardLayoutRule.LayoutOwnsTheEdge + NavSlotRule.EdgeElement; NavigationWarningRule.EdgeUnderALayoutThatOwnsIt
- [x] tests (rules, warning, scene: no scrubber, the peek is what a finger finds), mutation, recapture; Edge (i)

## F1 sub-headings
- [x] DrawNavigationSubHeading; five groups; row order per section 7 inside each; section 7 updated
## F2 Placed Model Control -> Card Container (JSON key stays)
- [x] row + (i) + guides move; _3.2 section 3, _5.1
## F3 audit section 5 gaps
- [x] Edge warning (C1); Families At The End grey-out: audit claim checked against BlockStackBuilder.FamilyIsAtEnd -- it applies to EVERY family,
      so greying it would remove a real option: pin the real behaviour with a test, record in the audit
- [x] Editor tests for F1/F2: row where it says, (i) answers, undo works

## Closing
- [x] full EditMode (2018/2018; the 4 window-width failures of the gate did not recur); PlayMode fixtures whose name holds Card / Poi / SizeComparison
- [x] docs: _3.2.1_Audit (C, F, F6 closed + matrix cells), _3.2 sections 3 / 5.2 / 5.5 / 7, _5.1, 10-structure.md, RUN TIMES
- [x] LivingRoomScene left open; final report per 60-finishing 6.4

## Result (2026-10-21)
All steps done. Full EditMode 2018/2018 (93.5 s); Card / Poi / SizeComparison PlayMode 949/949 (3234 s, 45 fixtures). Mutations proven and restored.
Docs: audit FIX STATUS, _3.2 (3, 5.2, 5.5, 7, status row 19c), _5.1, 10-structure.md, 40-testing RUN TIMES. Not committed (developer asks).
