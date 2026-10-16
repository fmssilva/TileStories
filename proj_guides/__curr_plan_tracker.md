# Current plan -- _3.2 POI Card Navigation, step 5a (nav slots, NavElementRegistry, Phase A harness, text_tabs)

Brief: proj_guides/__AI_worker.md (2026-10-15). Re-read it and this file after a compaction.

## GATE (done)
- [x] UnityMCP answers (telemetry_status)
- [x] git push: 2 commits (ddffe39..4d8cf85)
- [x] captures opened: TestEvidence/Gate/gate_3_2_step5a_gameview.png, gate_3_2_step5a_poieditor.png (Detail Card > Navigation)
- [x] compile: last compile ExitCode 0, no error CS after it
- [x] SDF fonts clean, nothing to restore
- [x] baseline full EditMode 1711/1711 (85 s, focused) on 4d8cf85 (docs-only after 213d8d0, whose full runs were green)

## Decisions
- Strip = clipped viewport + translated row (NOT a native ScrollView): Unity 6000.3 ScrollView touch-scroll captures any move with
  >= 10 px of x (Horizontal zeroes y), opt-out flag isHandledByDraggable is internal.
- Shared SwipeGrab (Runtime/UI/Cards): never captures at down, judges after the slop with a claim predicate, then captures + Moved /
  Released / Ended. KnowledgeCheckBlockView migrates onto it. SwipeGrabRule.ClaimsVertical added.
- Vertical drag on the top strip drags the sheet (like grabber / header); sideways scrolls the strip.
- INavElementView.Bind(CardNavModel, NavController, BlockBindContext). Known slots: top only.
- Gallery cases: big (62 targets over the 8 in-nav families + sources), small (6 / 3), one family, big in PT; min_blocks 2.

## Facts measured
- Lamp stops at 390 x 844: peek 195.00, half 337.60, full 796.00 -- before the change, and after with Top Bar text_tabs and none
- Lamp at Half with the strip: sheet 338 = header 206 + strip 49 + sticky footer 81 -> stack viewport 0 px (51 px without): 5b finding
- SwipeGrab: trickle-down for events aimed inside, at-target for the area itself, follow a captured child (Clickable), judge once at the slop

## Steps
- [x] P1.1 Families table: one (i) on the header row; HelpInfoButton draw probe; Editor test lists the section's (i) titles
- [x] P1.2 capture Navigation at ~450 pt with one guide open; checklist
- [x] S1 Phase B stops test written FIRST and run on the old code (baseline numbers)
- [x] S2 SwipeGrab + ClaimsVertical + KnowledgeCheck migration (its fixtures green)
- [x] S3 NavElementDefinition / NavElementRegistry / BuiltInNavElements / INavElementView + Tier 0 tests
- [x] S4 NavSlotRule + NavStripRule (pure) + tests; config field `top`; NavSlots in the sheet; sheet drag refactor
- [x] S5 TextTabsNavView + Navigation.uss + token; style lists (LivingRoomScene, CardGalleryScene)
- [x] S6 Editor rows Show Navigation From + Top Bar + help + tests (undo, walker, round trip)
- [x] S7 Phase A: CardNavGalleryDefinitions, CardNavGalleryHarness, CardNavGalleryScene, CardNavGalleryTests, renders judged
- [x] S8 Phase B PoiCardTextTabsSceneTests + Lamp 390 Half/Full captures judged
- [x] S9 full EditMode 1720/1720 + FULL PlayMode 802/805 (background; developer closed on it), RUN TIMES
- [x] S10 docs: _3.2 status 5a + both [step 5] review TODOs removed; _5.1 rows; 10-structure; final report
