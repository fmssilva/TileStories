# Plan tracker -- POI Detail Card 15.3 (audit fix E: visitor language + small layout items)

Brief: 15.3.1 fallback language, 15.3.2 visitor language choice + Preview Language, 15.3.3 English words on the PT card,
15.3.4 layout items (timeline end, related picture-less title, Size Comparison panel label), 15.3.5 close.
Tree trusted: green commit cf84c80 (no opening full baseline). ONE full EditMode + PlayMode at the end (15.3.5).
After a compaction: re-read this file and the brief's sub-step list; continue at the first unticked box.

## GATE (done 2026-10-02)
- [x] UnityMCP is the right server (telemetry_status ok)
- [x] Game view capture opened (TestEvidence/Card/gate_game_view.png: the room scan, edit-mode render) and POI Editor capture opened
      (TestEvidence/Editor/gate_poi_editor.png: the window, nothing else); window closed, 0 left
- [x] Oswald Bold SDF.asset not modified (git status shows only two guide .md files modified)
- [x] Tree compiles (no error CS; TileStories.dll 08:14 = last green commit's build, no .cs modified)

## Findings that shape the plan (verified on disk)
- The one text rule already exists: BlockFieldReader.Pick(entries, language, fallback) = language -> fallback -> first non-blank.
  The bug was the CALLER: PoiCardHost.Show passed `language` as BOTH language and fallback (and CardStrings / CardGlossary the same).
- Visitor state lives in CardLocalState (PlayerPrefs, wall-scoped key, index for ResetAll).
- Dev-only switches: CardDemoRule.IsAllowed + DevFeatureBuildGuard.Registry.

## 15.3.1 One language rule
- [x] CardLanguageRule (pure, Runtime/Blocks): Choices, Fallback, Shown, Next
- [x] BlockFieldReader.Pick stays THE text pick; CardStrings gets the "any language the key has" tier; glossary already uses Pick
- [x] PoiCardHost.Show + CardGalleryHarness: language = Shown, fallback = Fallback(languages), same for CardStrings/CardGlossary
- [x] (no BlockBindContext.Languages needed: Settings.languages carries the choices)
- [x] Pure tests (CardLanguageRuleTests 17/17)

## 15.3.2 Visitor language choice + Preview Language
- [x] CardLocalState.Language / SetLanguage (wall-scoped, in the ResetAll index)
- [x] CardStrings.Keys.LanguageSwitch (EN + PT row in CardStrings.asset)
- [x] PoiCardSheetView language chip (left of the X, shows the NEXT language; hidden with <2 choices) + LanguageRequested
- [x] Host: chip -> State.SetLanguage -> Rebind (scroll kept)
- [x] CardSettings.preview_language, gated by CardDemoRule.IsAllowed, DevFeatureBuildGuard row
- [x] Editor: Card Container > Test > Preview Language (popup, (i), Test guide); changing it clears the saved visitor choice
- [x] Tests: CardLocalState, Editor row, Phase A chip (72), Phase B real tap (6); SearchSceneFixture pins the saved language
- [x] Captures 390 px EN + PT (Card_language_chip_*, Card_Lamp_language_*_scrolled, Editor/preview_language_row.png)

## 15.3.3 English words on the PT card
- [x] The Lamp's PT card dumped (every visible text) + scrolled captures: no English card string or block field left
- [x] Remaining English = taxonomy (Royal Government, Intact, Partial Damage, Destroyed) + marker names of cardless points
      (Lamp - Residential, Lamp - Infrastructure) + baked "PLACEHOLDER" picture banners: ONE [later] TODO in _3.1
- [x] PT recaptured (Lamp_pt_*.png)

## 15.3.4 Layout items
- [x] CardTrackRule + TimelineBlockView.FitEvents + RelatedBlockView.FitCards (whole cells at rest and at the end of the swipe)
- [x] Size Comparison: Point Label field (app kind), Lamp fixture "The tile panel" / "O painel de azulejos"
- [x] Phase A (CardTrackGalleryTests, SizeComparisonGalleryTests) + Phase B (LivingRoomCardSceneTests) + captures judged

## 15.3.5 Close
- [x] Full EditMode green (1587/1587)
- [x] Full PlayMode green (833/833, 1855 s)
- [x] _3.1 (TODOs, container section, config table), _3.1.1 status, _5.1 4.7, 10-structure.md updated
- [x] _3.1 row 15.3 with the counts
- [x] Final report (60-finishing 6.4)
