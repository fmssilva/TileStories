# Plan tracker -- _3.2 step 15a (nav_rails + nav_bento) + the step 14 mid-swipe finding

GATE (done): UnityMCP answered (telemetry_status); 2 commits pushed (6db17e5..7eaa913); captures `TestEvidence/Gate/gate_3_2_step15a_{gameview,poieditor}.png`
opened, window closed; tree compiled (0 error CS); both SDF fonts unmodified (git clean); EditMode baseline 1909/1909 (71 s) on 7eaa913 (trusted).

## Part 1 -- mid-swipe page
- [x] 1.1 MEASURE (Phase B, real drag): nothing is bound on the neighbouring page mid-swipe; its blocks bind at `CardPagerView.Finish` (settle).
- [x] 1.2 Fix without a second stack: the arriving cue lists the section's targets as compact rows while a drag is claimed; cleared at Finish.
- [x] 1.3 Recaptured `Nav_Lamp_pages_midswipe_{en,pt}_390.png`, judged (pages fixture 11/11).

## Part 2 -- the two kinds (family `navigation`; no slot, container or stop)
- [x] 2.0 Shared bits: `CardTrackFit.Apply(VisualElement viewport, ...)`; `BlockStackView.RowOfBlock` + `NavController.RowOf`; CardStrings en + pt; tokens.
- [x] 2.1 `nav_rails`: `NavRailRule`, field `max_swipes`, `NavRailsBlockView` on `NavStrip` + `CardTrackRule/Fit` + Peek Next Card; "All n" card.
- [x] 2.2 `nav_bento`: items `tiles`, `BentoTileRule`, `NavBentoBlockView`; picture or the family icon; Editor stale-pick warning via the BlockRef drawer.
- [x] 2.3 Auto Nav Block: rails join, bento does not (`BlockKindDefinition.NeedsAuthoredContent`).
- [x] 2.4 Fixtures: Lamp - Residential authored card (bento + 4 blocks), both config copies; The Lamp untouched; the card gallery got a `nav_bento` entry.
- [x] Tier 0: `NavRailRuleTests`, `BentoTileRuleTests`, `AutoNavBlockTests` +, `DetailCardEditorTabTests` +, round trip, reading time. EditMode 1928/1928.
- [x] Phase B: `PoiCardNavRailsSceneTests` 7/7, `PoiCardNavBentoSceneTests` 4/4. Phase A: `CardNavGalleryTests.RailsAndBento` (rails x5, bento x5, Peek off).
- [x] Visuals judged: rails rest / swiped / lower EN + PT, bento Residential EN + PT, `Nav_nav_*` cases, Editor `280_CardContent_Bento_620.png`.
- [x] Docs: `_3.2` TODO + 5.6 + 6.1 + 7 + 3 + 2.2, `_3.1` track rule, `10-structure.md`, `__mixed_TODOs.md` device list.
- [ ] Closing: second targeted run (galleries + BottomElements flake check) green; `_3.2` status row 15a written with the counts; final report.
