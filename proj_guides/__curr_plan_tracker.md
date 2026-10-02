# Plan tracker -- POI Card 15.1: audit fixes A + D (started 2026-10-01)

Tree = green commit e1473b3 ("Audit the POI card domain, fix the focus-dependent typing test, tidy the guides") + docs only.
GATE: TestEvidence/Card/Gate_15_1_GameView.png + TestEvidence/Editor/Gate_15_1_POIEditor.png opened and read (capture window closed,
0 left), Oswald font unmodified, Editor.log 0 `error CS` / ExitCode 0 (Play Mode was on: stopped first).
Brief: proj_guides/__AI_worker.md. Audit: _3.1.1_Audit.md sections 5 + 11 (15.A, 15.D). No opening baseline; ONE full run at the end.

## 15.1.1 Field visibility (15.A items 1-3)
- [x] Runtime/Blocks/FieldShownWhen.cs: Looks(...) / Choice(key, values...) / Filled(key); BlockFieldDefinition.ShownWhen
- [x] Runtime/Blocks/FieldVisibilityRule.cs: resolved variant (own else Library default), resolved Choice (own else Library/ChoiceDefault),
      IsShown(block field), IsShown(item sub-field, row), IsShownInLibrary(field, kind, settings, pois) (default look OR any block's look)
- [x] Declarations: place_in_ar (height_cm / marker_multiple), knowledge_check (per look), dialogue (choice/reply n+1 after choice n),
      header (per look; subtitle not compact), video (chapters look)
- [x] Drawer hook: DrawBlockFields + DrawItemsBlockField (skip hidden); Block Library Default Fit row; Display popup only with 2+ modes
      (column space reserved)
- [x] Tests: FieldVisibilityRuleTests (pure + declaration guard); real-window test (Scale Mode + KC look: rows appear/disappear,
      hidden value survives switch back, Ctrl+Z, Save + LoadConfig under Temp/); DrawingNeverWrites / BlockKindRoundTrip / EveryKind green
- [x] Captures 620 pt: Place In AR, Knowledge Check (3 looks), Dialogue, Header; 4.5 design questions answered

Proof: EditMode full 1538/1538; targeted 49/49 by name (FieldVisibilityRuleTests x11, the real-window test, DrawingNeverWrites x2,
BlockKindRoundTrip x35, EveryRegisteredKind). Captures 620 pt: CardContent_PlaceInAr_RealSize / KnowledgeCheck_MC_TF / _TF_IC / _IC /
Dialogue / Dialogue_b / Header_ImageParallax (TestEvidence/Editor). Audit miscounts noted: Image Choice draws 11 (not 10), Model Turntable 3
(Picture = its fallback).

## 15.1.2 Collapsed-row summary (15.A item 4)
- [x] Summary column (heading in first wall language, else first text), ellipsis fit within the row budget, tooltip = full text
      (placed AFTER the delete: the existing ItemsRows test pins "a block's delete sits right after its row's cells"; row capped
      by CardBlockRowWidth = min(view - margin - indent, 800))
- [x] Real-window test (text, ellipsis, inside the row, none when open) + capture
Proof: EditMode full 1539/1541 (2 input tests failed once in the run after an orphaned job, Editor focused; pass alone and in fixture
order 39/39 -- recheck at the full run). New: ABlockSummary_..., FitWithEllipsis_..., ACollapsedBlockRow_... (widest/880/620).
Captures: CardContent_Summaries_880pt.png, _620pt.png. Lesson: a file save during an EditMode run killed it (orphan, healed 318 s).

## 15.1.3 Default Media (i) column (15.D item 1)
- [x] Rows through DrawEditorRow, cells in ONE rect of the title label's width + style (DefaultMediaCellsRect), split in C#;
      PoiEditorWindowHost.Resize / Width / RootScreen (a fixed spacer was 1.6 pt off: margins of the cells' own styles)
- [x] Render test: row (i) x == title (i) x at 880 and 620 pt (+-0.5 pt); recapture DetailCard_DefaultMedia_880pt / _620pt.png
Proof: EditMode full 1542/1542 (Unity in the background).

## 15.1.4 _5.1 4.7 (15.D item 2)
- [x] Default Media, Block Library default rows, field visibility, Display popup + summary; pending list entries updated
      (header / KC / dialogue / Default Media captures recorded), 4.7 Tests sentence

## 15.1.5 Audio fade-pause (15.D item 3)
- [x] CardAudioService: pause during a switch fade kept, new clip starts paused; Resume/Toggle during fade cancels; reset on Stop / new fade
- [x] Pure tests (CardAudioServiceTests x3) + coordinator test (CardVideoRulesTests, real CardVideoService) + Phase B (PoiCardVideoSceneTests)
Proof: EditMode full 1546/1546 (+4, the new tests); PlayMode targeted 46/46 (PoiCardVideoScene, PoiCardAudioScene, CardAudioGallery,
PoiCardRealAudio, CardVideoGallery), the new Phase B test by name.

## 15.1.6 Close
- [x] Full EditMode + PlayMode green: EditMode 1546/1546, PlayMode 741/741 (1645 s), both with Unity in the background
- [x] _3.1 (row 15.1 + 4 TODOs closed + section 3 / 8.2 + Design history + flaky note), 10-structure.md (3 new files, 11 entries), _5.1 4.7
      + pending list, __mixed_TODOs (2 ideas); final report (60-finishing 6.4)
Not mine, left as Unity wrote them: LivingRoomScene.unity (Test Runner save dropped two stale `_debug: 0` lines), TileStories.slnx (order).
