# Plan tracker -- M2: fast, honest test runs (Test Suite Review 2, chunks F1 + F2 + F4 + F11, + F10's two deletions)

Brief: `_3.3_POI_Card_Styles.md` M2 (pasted in chat; `proj_guides/__AI_worker.md`). Worker: Opus, effort Medium. Test infrastructure only,
no product code. Source of truth: `proj_guides/_Test_Suite_2.md` (2.1, 3.2-3.4, 4). STOP with one AskUserQuestion.
TEST PLAN: full EditMode (with and without the Live assembly); CORE once, in the background (Unity c).

## Decisions (autonomous)
- The split fixtures' abstract `*Fixture` bases fold back into their one kept class; helpers only the deleted tests used go.
- DetailCardEditorTabTests 312-314 are NOT an old-element sweep: they are the reflection walk's edits for the live `top` / `bottom` /
  `menu` config fields (every card_settings field undoes / redoes / survives Save) -> kept. No other old-element sweep in that file.
- The Live assembly holds the 5 fixtures that `new EnterPlayMode` (Card, StoryEditor, NavigationBar, CardLocate, CardSections);
  LivePlayModeConfigTests (19 pure tests, 1.8 s) and LodZoomLiveSyncTests (no Play Mode) stay. A rule test keeps Play Mode out of
  the main Editor test assembly.
- NavBarShapeRuleTests: measure first; the fix keeps every sample and assert (precision unchanged).
- CORE Sections test: TheDefault_IsOpen (4.6 s, the cheapest default test); jump landings stay guarded by the NavBar's real-tap test
  and PoiCardNavSceneTests.AJumpFromPeek.
- CardSmoke: one cheapest test per row of `_Test_Suite_2.md` 3.2 (no generic "a block kind" smoke: a kind block runs its own selection).

## Steps
- [x] 0. GATE: UnityMCP live (TileStories@d41388e4, telemetry on), HEAD bb32f6d = origin/master, SDF fonts unchanged, compile clean
       (0 error CS, ExitCode 0), Play Mode off. Editor.log is 444 MB (flag in the report).
- [x] 1. Audit: references (Haiku Explore), split classes, Live fixtures, per-method times from the last FULL (C2, scratchpad full_pm_c2.xml)
- [x] 2. PART 1 (F1 + F10): delete the whole files; strip + fold the 9 split files; NavigationElements cases; LookMeasure constants;
       comments that name deleted tests; `_3.3` [rebuild] lines; structure map; `_Test_Suite_1.md` rows -> compile -> full EditMode
       RESULT: full EditMode 2209 / 2208 passed / 0 failed / 1 skipped, 298.6 s, behind / behind (was 2240 / 2208 / 0 / 32)
- [x] 3. PART 2 (F2): Core tags (NavBar 2 methods, SectionSheet off, Sections' default on) -- reflection: 26 Core classes + 20 methods;
       `_Test_Suite_1.md` section 3 rewritten after the CORE run (step 6)
- [x] 4. PART 3 (F4): group_names `CardGalleryTests\..*\("quick_facts` = 6 / 6 passed, 9.7 s (exactly the expected cases); CardSmoke 21
       methods = 22 / 22 passed, 82.1 s; PoiCardPagesSceneTests (folded) 8 / 8, 53.4 s -- all behind / behind;
       `40-testing.md` 4.2 BLOCK rewritten; `_Test_Suite_2.md` 3.2 selections
- [x] 5. PART 4 (F11): NavBarShapeRuleTests measured (Token() = 0.338 ms a read, ~98k reads in RaisedDepth; 41.2 s before) -> tokens read
       once: 0.2 s; Live assembly (5 fixtures, 103.8 s) + PlayModeEntryRuleTests + 2 InternalsVisibleTo grants -> compile clean ->
       EditMode without Live 2200 / 2199 / 0 / 1, 2.0 min (behind / behind) -> full EditMode 2211 / 2210 / 0 / 1, 3.4 min (ended in front)
- [x] 6. PART 5: CORE PlayMode 175 / 174 / 0 / 1, 12.0 min + EditMode 120 / 120, 14 s (behind / behind); RUN TIMES; `_Test_Suite_1.md`
       section 3; `_Test_Suite_2.md` 3.6 + status; structure map; LivingRoomScene active, Play Mode off; report + commit line
