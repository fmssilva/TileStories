# Plan tracker -- POI Detail Card 15.4 (audit fix F: code and test health) + two 15.3 follow-ups

Brief: 15.4.1 ElementPool, 15.4.2 CardContextBuilder (host < ~250 lines), 15.4.3 file splits, 15.4.4 source-rule scan + VttRule BOM,
15.4.5 language-chip test cost, 15.4.6 Peek Next Card, 15.4.7 close (BACKGROUND full PlayMode + full EditMode, docs).
Tree trusted: green commit 809f910 (no opening baseline). Name baseline (TestRunnerApi.RetrieveTestList at 809f910):
TileStories/TestEvidence/Baseline/PlayMode_names_before_15_4.txt (833), EditMode_names_before_15_4.txt (1587).
15.4.1-15.4.5 change no behaviour: after each, full EditMode + the PlayMode fixtures they touch.

## GATE
- [x] UnityMCP answers (telemetry_status)
- [x] Asked (AskUserQuestion) to keep Unity uncovered
- [x] Game view capture opened: TestEvidence/Gate/gate_game_view_15_4.png (LivingRoom scan, painting)
- [x] POI Editor capture opened: TestEvidence/Gate/gate_poi_editor_15_4.png (Detail Card > Card Container), window closed (0 left)
- [x] Oswald Bold SDF.asset not modified
- [x] Compiles: Editor.log last build ExitCode 0 / Tundra success, no error CS after it

## 15.4.1 ElementPool<T>
- [x] Runtime/UI/Cards/ElementPool.cs: ctor(Func<int,T> create, Action<T> release); Take(); ReleaseAll(); Shown; Created
- [x] Tests/Editor/UI/Cards/ElementPoolTests.cs (pure, 5)
- [x] Convert the 19 index pools: Actions, FunFact, ProcessSteps, QuickFacts, Status, Swatches, ShowOnWall, Feedback, Hotspot, Sources,
      KnowledgeCheck, Poll, Timeline, PracticalInfo, CardTextView, RichText sections, Dialogue bubbles + choices, StoryChapters segments
- [x] Left as they are (different mechanism): BlockStackView per-kind stack, Gallery / Related / WallLocator free-lists
- [x] EditMode 1592/1592 (86 s); PlayMode 404/404 (967 s, CardGallery 325, PoiCardScene 55, CardTrackGallery 4, LiveUpdate 14, LanguageScene 6). Full EditMode; PlayMode: CardGalleryTests + the card gallery/scene fixtures of those views
## 15.4.2 CardContextBuilder + CardOwners
- [x] Runtime/Blocks/CardContextBuilder.cs (pure): Languages, MediaRootChanged, Build, SkipWarning; used by PoiCardHost AND CardGalleryHarness
- [x] Runtime/UI/Cards/CardOwners.cs (+ CardTapOutside, CardDemoOpener, CardStyleSheets): audio coordinator, video, preview, AR, sound coordinator (+ Use*, CardShown/Closed, Shutdown, Dispose)
- [x] PoiCardHost 412 -> 266 lines; CardContextBuilderTests (6)
- [x] EditMode 1598/1598; PlayMode card set 550/551 (PoiCardLanguageSceneTests.ThePick: tap never reached the chip, no SwitchLanguage in the log; 3 reruns green; precondition made exact; seen-once note for _3.1 sec 12). Full EditMode; PlayMode: PoiCard*SceneTests, PoiCardLiveUpdate, TapZoom, LivingRoom scene tests, gallery fixtures
## 15.4.3 Splits
- [x] DetailCard.cs (518 -> 306) -> + CardTexts / CardGlossary / CardDefaultMedia partials
- [x] CardGalleryTests (core + 8 family files), PoiCardSceneTests (core + 8); 380/380 names identical to baseline, 887 s; per family: gallery core 463 s/270 (the generic entries), About 49, Media 27, Play 31, Stories 23, Visit 21, Ar 11, Community 9, Meta 4; scene core 89, About 51, Play 32, Visit 25, Media 20, Stories 17, Ar 7, Community 7, Meta 3
- [x] EditMode 1598/1598; both fixtures 380/380 (background)
## 15.4.4 Source rules
- [x] CardViewSourceRulesTests scans UI/Cards + Blocks/Preview + Blocks/Ar; plant __PlantedLiteral.cs (new UnityEngine.Color32) -> red ("writes a literal look") -> removed -> green; regex widened to (UnityEngine.)Color(32)
- [x] VttRule.cs BOM written as its ASCII escape (0 non-ASCII bytes) + AudioRulesTests.Vtt_AFileSavedWithAByteOrderMark (Unity TrimStart() also strips it: the test pins behaviour); EditMode 1599/1599, audio PlayMode 28/28
## 15.4.5 Language chip cost
- [x] CardLanguageRule.ChipLabel (pure) + CardLanguageRuleTests.TheChipLabel_... ; PoiCardSheetView uses it
- [x] CardLanguageChipGalleryTests 72 -> 8 (14.7 s, was ~250 s): removed the 68 EveryHeaderEntry_TheTitleWrapsBeforeTheChip cases; added TheTitleWrapsBeforeTheChip_OnEachHeaderStructure x3 + AWallWithThreeLanguages; EditMode 1600/1600, chip + language scene 14/14
## 15.4.6 Peek Next Card
- [x] container.peek_next_card (default true) + Card Container row (i), undo, live
- [x] CardTrackRule peek mode (Peeks + CellWidth overload) + CardTrackFit (one fit for both views) + --ts-track-peek: 32 (plain number: CustomStyleProperty<float> cannot parse "32px", the first run read 0)
- [x] Tests: CardTrackRuleTests +5, CardTrackGalleryTests (15.3 tests now OFF, +6 ON cases), DetailCardEditorTabTests row + guard count 9, PoiCardLiveUpdateTests live; 25/25, scene 55/55, gallery tracks 15/15
- [x] Gallery entries timeline_horizontal_long_nopeek + related_carousel_nearest_nopeek; captures checked: Card_timeline_horizontal_long_peek_rest|end, Card_related_carousel_nearest_peek_rest, _nopeek, Editor CardContainer_PeekNextCard_620pt
## 15.4.7 Close
- [x] BACKGROUND full PlayMode 778/778, 1602 s, editor_is_focused false early + end; names = baseline - 68 removed (15.4.5) + 13 added
- [x] Full EditMode 1606/1606, 95 s (background)
- [x] RUN TIMES (40-testing), _3.1 row 15.4 + TODOs + sec 3/4/5.1/12, audit status, 10-structure.md, _5.1 (file map, Card Container rows)
