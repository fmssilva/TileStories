# Current plan tracker -- POI Detail Card: evidence clean-up, step 11-fix, step 12

Task source: proj_guides/__AI_worker.md. Domain spec: proj_guides/_3.1_POI_Card_Blocks.md. Tick a box only when its proof ran.

## Gate + baseline (2026-09-28)
- [x] UnityMCP answered (telemetry_status true); Game view capture + POI Editor window capture opened and read; window closed, 0 POIEditorToolWindow left; gate PNGs deleted
- [x] Tree committed (git status: only proj_guides/__AI_worker.md and __curr_plan_tracker.md modified)
- [x] Editor.log: last compile ExitCode 0 (the 3 old `error CS` lines are from an earlier session, line 3.6M of 5.3M)
- [x] EditMode baseline 1317/1317 (Unity unfocused)
- [x] PlayMode baseline 557/557 (Unity unfocused)

## PART 0 -- evidence clean-up (no behaviour change)
- [x] 0.1 `TestEvidence.PathFor(domain, fileName)` (Framework/Tests/Runtime/TestEvidence.cs) -> `<project>/TestEvidence/<domain>/`
- [x] 0.2 replaced every copy: CardGalleryChecks, DisplacementDemoFixture, EffectsPreviewRenderTests, LodRealPipelineTests, SearchSceneFixture, LodWallSceneTests, MarkerGalleryTests (CardGalleryTests only called CardGalleryChecks)
- [x] 0.3 364 PNGs moved to TestEvidence/<domain>/ (+291 MarkerGallery), Assets/Screenshots deleted
- [x] 0.4 TileStories/.gitignore + `git rm -r --cached MarkerGalleryScreenshots` (staged, not committed)
- [x] 0.5 7 InitTestScene scenes (+ metas) deleted; the four "developer decides" files untouched
- [x] 0.6 docs: _3.1, _5.1, 40-testing.md, 10-structure.md, _2.x docs that say Assets/Screenshots
- [x] 0.7 proof: TestEvidenceRuleTests 4/4 + TestEvidenceTests; after the FULL PlayMode run `find Assets -name "*.png" -newer <run start>` = nothing; 426 renders landed in TestEvidence/

## PART 1 -- step 11-fix
- [x] 1.1 CardStrings: wall > app > framework per language, then the fallback (5-arg constructor, callers updated)
- [x] 1.2 `CardStringSources` registry (Runtime/Blocks): named app tables, Shared, refuses duplicate names / keys
- [x] 1.3 PoiCardHost + CardGalleryHarness pass the app entries (harness gets a Language property)
- [x] 1.4 LivingRoomCardStrings.asset (Resources/LivingRoom/, en + pt), LivingRoomCardTexts keys, registered by LivingRoomBlocks (Edit Mode + runtime, idempotent)
- [x] 1.5 Card Texts lists app rows under "<app> (app texts)"; FrameworkCardStrings() now loads the framework table by exact path (found bug: two CardStringTable assets)
- [x] 1.6 tests: CardStringsTests (order, registry), CardStringTableChecks (shared table + source-scan helpers), app source scan, REAL typing on an app row + Ctrl+Z
- [x] 1.7 size_comparison labels + round coin + slot geometry; tests; captures EN + PT checked (gallery + real Lamp)
- [x] 1.8 `BlockRegistry.Ordered`; Block Library + "+ Add block" use it; BlockRegistryOrderTests + real window test; capture checked
- [x] 1.9 captures checked: Card_size_comparison_*, Search_Lamp_SizeComparison(_pt), Editor_CardTexts_AppRows, Editor_BlockLibrary_Ordered
- [x] 1.10 full EditMode 1360/1360 and PlayMode 581/581 after Parts 1 and 2 (Unity in the background)

## PART 2 -- step 12
- [x] 2.1 `PoiCardHost.Rebind` (keeps stop + scroll)
- [x] 2.2 `LivePlayModeCardApplier` (+ WallSession.ApplyCardSettings), registered in LivePlayModeConfigPush
- [x] 2.3 LiveSyncFieldMatrixTests: card rows, exclusion removed
- [x] 2.4 Phase B: real window edit while Play Mode runs changes the open card (text, block added, variant changed), undo brings it back
- [x] 2.5 `card_settings.demo_card` (off by default) + pure DemoCardRule.IsAllowed + PoiCardHost opens it; DevFeatureBuildGuard entry
- [x] 2.6 Detail Card > Card Container > Test: Show demo card + POI popup + stop popup, Open Gallery; guide says Play Mode / UI Builder only
- [x] 2.7 tests: real-click Editor test, build-guard test, PlayMode demo card test
- [x] 2.8 captures: Test foldout with demo controls, live edit before/after in Game view

## Close
- [x] docs: _3.1 (rows 11-fix + 12 with proof, TODOs, design history), 10-structure.md, _5.1, 40-testing.md
- [x] full EditMode 1360/1360 + PlayMode 581/581 green, last compile ExitCode 0, no windows left, no InitTestScene in Assets
- [x] final report: suites + counts, what I looked at, what I could not verify, one-line commit message, exact `git add` paths
