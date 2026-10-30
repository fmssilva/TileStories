# Plan tracker -- M3: the reference card (Test Suite Review 2, chunks F3 + F6, + the Editor log chunk)

Brief: `_3.3_POI_Card_Styles.md` M3 (pasted in chat; `proj_guides/__AI_worker.md`). Worker: Opus, effort Medium. Config content + a test
helper + moving tests; no new product feature. Source of truth: `proj_guides/_Test_Suite_2.md` (1.3-1.6, 2.2-2.4, 3.2, 4).
TEST PLAN: the Ribeira-reading selections + CardSmoke + EditMode without Live; then ONE FULL in the background. STOP with one AskUserQuestion.

## Decisions (autonomous)
- Golden test = every registered kind at least once on the Ribeira, no kind + look twice, ONE named exception list
  (`nav_contents`: kept for a later rebuild, `_3.3` TODOs; its edge card lamp_residential) with a stale-entry check. "Exactly once" cannot
  hold: block_14 / 15 / 16 are three knowledge_check looks and block_15 (true / false) must stay for Show After Reading. -> 19 new blocks.
- actions at `sticky_cta` (1.4 names it; F6 needs a sticky action on the golden card for the bar's fixture); hidden at the default bar.
- Stand-ins, marked in the visitor's words + TODO: model_3d / place_in_ar `default:azulejo_arch`, panorama_360 `default:tiled_room_360`;
  video = the Lamp's real Lisbon square + castle clip under its truthful title; size_comparison 60 x 45 cm (to be measured).
- Swatch colours sampled from the real `azulejo_detail.jpg`.
- `ReferenceCard` in `Framework/Tests/Runtime/` (public: EditMode, PlayMode and the app's test assemblies reference that assembly).
- PART 3 moves only Ribeira keys (StoryKey x3 files, PaintingStory HeaderKey / GalleryKey); Lamp keys move with F7 (out of scope).
- PART 4: 135.7 of ~140 MB per full EditMode run = Unity's "DPI setting of a visual element that is not on a panel" x11994 (Live
  fixtures: live push -> PoiCardHost.Rebind -> HeroWordsOnPicture.Fit -> InkRects measure on a detached label). Fix at the source: no
  measure off a panel (verify the fit re-runs on attach). No product warning removed.

## Steps
- [x] 0. GATE: UnityMCP live (TileStories@d41388e4, telemetry on), HEAD b0fb045 = origin/master (nothing to push), Play Mode off,
       LivingRoomScene active, compile idle. Editor.log 767 MB (prev 3.86 GB).
- [x] 1a. Audit: 37 registered kinds, Ribeira holds 16 (18 blocks); subagents: Sonnet draft (ribeira_new_blocks.json), Haiku key list
       (3 files; + PaintingStory found by hand = 4 sites)
- [x] 1. PART 1: 19 blocks (block_20..38, key memory 38) + block_15 show_after_viewed, through the classes (round trip byte-identical
       first), both copies identical; structural diff = only `painting`; builder 37 entries / 0 skipped. Fact fixes: "five centuries",
       the opera in the 1580s, actions -> sticky_cta, status heading neutral, PT greeting; swatches sampled (#203766 / #7789A1 / #BBB7AD)
- [x] 2. LOOK (TestEvidence/Card/M3/, Reduce Motion for stills): About split in two on the test card (block_6 authored after Media,
       framework default = end families last) -> block_6 moved after block_26; re-captured: one section per group. Developer: ACCEPTED;
       the POI's "intact" status -> _3.3 TODO
- [x] 3. PART 2: ReferenceCard + ReferenceCardTests 6 / 6 (2.8 s); 00-process "Real fixtures, one home" names it
- [x] 4. PART 3 + F6: the 4 Ribeira key sites -> ReferenceCard (StoryKey x3, PaintingStory Header / Gallery keys); the sticky action
       -> PoiCardNavBarSceneTests.StickyActionShown_... on the golden card (+ the hero-row / PT-words checks of Hero's Lamp branch, the tap,
       the words EN / PT); forcing dropped in Hero x2 and PoiCardSceneTests x2 (+ the pinned-footer duplicate); PoiCardSceneTests.About's 2
       sticky tests, Hero's forced tap and Locate's sticky matrix deleted; Core moved from Hero's forced tap to Locate's real pin tap (the
       shipped default's "show me where it is"); LocateFabRuleTests' NavBarRule lines -> NavBarRuleTests (2 tests); NavBarScene's
       under_title half out (merged group + bar off kept); Chrome's ReduceMotion_MakesTheBottomAreaJump. Not held: Sections' "default is
       Open" (it asserts only what renders)
- [x] 5. PART 4: HeaderBlockView.FitWords and SizeComparisonBlockView.Layout wait for a panel (+ AttachToPanelEvent);
       PoiCardLiveUpdateTests.ALivePush_RefitsTheHerosWordsOnceOnThePanel_... red (6 warnings: size_comparison) -> green, 0 warnings in the
       run; growth measured in the FULL's EditMode
- [x] 6. Runs (all Unity behind / behind): EditMode half 2207 / 2206 / 1 red (CardNavModelTests' interleaved About: case built in the
       test now) -> 27 / 27; selection 377 PlayMode, 7 red (BrightPage x2, Painting, Collect, Sections x3: the 18-block card's numbers) ->
       read from the card -> 8 / 8 + 2 / 2; CardSmoke 22 / 22 (75 s); FULL PlayMode 1153 / 1130 / 0 failed / 23 skipped (73.7 min, 0 DPI
       warnings, log +10.3 MB); full EditMode 2218 / 2217 / 0 / 1 (4.4 min, log +1.77 MB, was ~141 MB)
- [x] 7. Docs: _Test_Suite_2 (1.4, 3.2, section 4 status), _Test_Suite_1 (Core list), 10-structure, 00-process, 40-testing (RUN TIMES,
       4.2.6 rule), _3.3 TODOs + status, __mixed_TODOs, CardMedia/README.txt; report; LivingRoomScene; AskUserQuestion
