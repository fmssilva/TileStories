# Plan tracker -- _3.3 R16 row 3b: UI folder layout, the render-target guard, the spike out of the repo

HEAD b505825 (clean tree, 0 commits ahead of origin). Baseline: none (last block closed with full PlayMode 1336 + full EditMode 2119, background).
Brief = approval. Do not commit (the Architect commits). Re-read this file after a compaction.

## GATE
- [x] git push: branch 0 commits ahead of origin/master -> nothing to push
- [x] Game-view capture (TestEvidence/Gate/gate_game.png) opened: the wall scene renders
- [x] POI Editor window capture (TestEvidence/Gate/gate_poi_editor.png) opened: the window, nothing else; closed after (0 windows left)
- [x] Tree compiles: scriptCompilationFailed=False, last compile ExitCode 0
- [x] Oswald Bold SDF.asset / Roboto-Bold SDF.asset unmodified (git status clean for them)
- [x] Ask Unity's place (4.2.3 action c, word for word) before the first test run; wait as 4.2.5 says

## Audit facts (verified on disk)
- UI folders today: Cards (128 files incl. Blocks), Markers (61), Navigation (15, = search), Theme (21), Shared (5), Zoom (3), Filter (2), Minimap (2), Results (1), 6 empty scaffolds
- ~60 code/test files write old paths as strings (CardStrings.asset, CardTokens.uss, PoiCard.uss, POI_Marker/POI_Cluster.prefab, IconLibrary/FontLibrary.asset,
  PanelSettings.asset, Zoom uxml/uss, Navigation/Cards folder scans, Markers/Fonts SDF assets); About.uss has RELATIVE url("../../../Markers/Rings/...")
- No .unity / .prefab / .asset / .json text names an old path (they bind by GUID); config.json has none
- Folder scans: CardViewSourceRulesTests (Runtime/UI/Cards), ThemeTokenGuardTests (Cards / Navigation / Zoom), LivingRoomSceneWiringTests:104 (Cards)
- The spike (Assets/Dev/StyleSpike) is referenced by nothing in Framework / Apps; only TestRunReportTests:118 uses its assembly name as test-case data, and 40-testing.md 4.2 / 10-structure.md mention it

## Steps
1. [x] PART 1a: MoveAsset (one folder at a time, in Unity): create Screen/ World/ Blocks/ Components/; Cards/Blocks -> Blocks (FIRST, before Cards moves);
       Cards -> Screen/Cards; Navigation -> Screen/Search; Zoom, Filter, Minimap, Results -> Screen/; the five empty scaffolds -> Screen/;
       Markers -> World/Markers; GuideCharacter -> World/GuideCharacter; Components/README.txt. Check 0 errors, the .meta pairs and GUIDs kept.
2. [x] PART 1b: fix every string path (code, tests, About.uss relative url, Fonts/README.txt, SectionOpenerView comment); grep the whole repo for each old path -> 0 hits;
       0 missing references (console + LivingRoomScene + prefabs)
3. [x] PART 1c: full EditMode green before the guard: 2119 run, 0 failed, 1 skipped, Unity behind
4. [x] PART 2: Tests/Editor/UI/UiRenderTargetGuardTests.cs -- the R15.2 guard, ONE allow-list with a reason per line (and a stale entry fails); mutation proof
5. [x] PART 3: delete Assets/Dev/StyleSpike with Unity's API; drop its mention in TestRunReportTests data, 40-testing.md 4.2, 10-structure.md; compile + suites
6. [~] PART 4 (10-structure tree, _3.3 R15, guide paths done; left: R16 row 3b status, final grep): docs as current truth: 10-structure.md (tree + tags), _3.3 R15 (+ R16 row 3b status), every guide path, Components/README
7. [ ] Full EditMode, then FULL PlayMode (background, action c); update 40-testing.md RUN TIMES if one moved > 10 %
8. [ ] LivingRoomScene left open and ready; final report (60-finishing 6.4)

## Decisions (autonomous)
- Guard file: Framework/Tests/Editor/UI/UiRenderTargetGuardTests.cs (EditMode, mirrors Runtime/UI)
- Scan = comments stripped, word-boundary type names; UnityEngine.UI / TMPro checked in Screen/, Blocks/, Components/ (Theme/ and Shared/ serve both systems)
- Allow-list entries are (file, rule) -> reason; an entry the scan no longer needs fails the test, so S2's fixes force the list to shrink
