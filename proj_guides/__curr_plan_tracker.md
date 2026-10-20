# Plan tracker -- _3.2 step 15b (layout `bento`)

GATE (done): UnityMCP answered (telemetry_status); 1 commit pushed (7eaa913..7abf8e2); captures `TestEvidence/Gate/gate_3_2_step15b_{gameview,poieditor}.png`
opened, window closed; tree compiled (3 old `error CS` lines precede the last build, which succeeded); SDF fonts unmodified (git clean); EditMode baseline
1928/1928 (99 s, unfocused) on clean 7abf8e2 (trusted).

Decisions: CardBentoView (sheet-owned, like CardPagerView) + pure BentoLayoutRule (reuses ProgressMapRule.Widths on both axes: the target wins);
the largest section leads a full row, the others pair in model order, an odd last one spans; a tap / any jump to a sectioned block opens the SECTION
in the full view (one page, its targets stacked under their headings, BlockFullView.SectionPageFor); Current = the open section, -1 on the overview;
Jumped first (closes overlay + takeover), then ONE takeover; Show After Reading: the section page measures HasViewed into the controller.

- [x] P1 BentoLayoutRule + CardLayoutRule bento + CardOptions Bento + config comment; Tier 0 BentoLayoutRuleTests, CardLayoutRuleTests +
- [x] P2 CardLayoutView bento state, NavController bento jump / spy, BlockFullView.SectionPageFor (+ viewed), sheet wiring, CardBentoView, USS / tokens;
      EditMode real-sheet test (jump -> section open, nav_jump, Current, sources scroll)
- [x] P3 Editor: Layout lists Bento, (i) with N, guides; DetailCardEditorTabTests (full EditMode 1938/1938); [x] capture `Editor/290_CardNavigation_Layout_Bento_620.png` judged
- [x] P4 Phase A gallery bento entries; Phase B PoiCardBentoLayoutSceneTests 6/6 (real taps, Back, owners, no second sheet, reading -- load-bearing, spy, Military);
      renders judged (fixes from them: block names in the tiles; none in a narrow tile)
- [x] P5a targeted set 892: 871 + 21 (20 MINE: the hidden bento host shifted the scroll's children -> host only in the scroll while bento; 1 knowledge
      swipe timing, not reproduced, logged) -> re-run 149/149 (CardNavGalleryTests whole + the swipe + bento scene); full EditMode 1938/1938
- [x] P5b FULL PlayMode 1113/1113 (3467 s, unfocused at both ends); flake: 0 lines in both long runs (left open); _3.2 status row, RUN TIMES
- [x] P5c docs: _3.2 (6.1 built 15b, TODO step 17, section 3 + 7), _5.1, 10-structure, __mixed_TODOs (video owner question, vacuous IsCurrent, few-sections idea, device line, swipe sighting)
