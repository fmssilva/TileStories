# Plan tracker -- _3.3 P1c: real names for the demo points

Brief: `__AI_worker.md` (P1c). Worker: Sonnet-class. Identity data only (config + test re-bases), no new runtime code.

## GATE (done)
- [x] push: 0 ahead (HEAD 2351b29 = origin)
- [x] UnityMCP confirmed: telemetry_status ok, console readable
- [x] captures opened: `TestEvidence/Card/P1c_gate_game.png` (Game view, Edit Mode: room scan + painting),
      `TestEvidence/Editor/P1c_gate_editor.png` (POI Editor window, Scene Configuration + 3 tabs); window closed, 0 left
- [x] compile: console clean (only XR Simulation duplicate-ID noise); fonts: git status clean -> nothing to restore
- [x] baseline full EditMode, Unity focused: 2033 tests, 1 FAILED = `KeywordListFieldTypingTests.TypingTwoKeywordsWithACommaBetween_GivesTwoKeywords`
      ("the click must focus the keyword field") -- an Editor-window focus test, not chased (to re-check alone after the edit)

## Findings (verified on disk)
- `POIData.name` is ONE plain string (`WallConfigData.cs:540`), no per-language form -> keep EN name, PT names as `search_keywords`
  (gap goes to `__mixed_TODOs.md`). The card header title is already per-language and authored (St George's Castle / Ribeira Palace).
- Config lives in `Apps/LivingRoom/config.json` and `StreamingAssets/LivingRoom/config.json` (identical, LF, JsonUtility pretty print).
  `generate_config.py` is the OLD generator (not the source of truth any more): not touched.
- `00-process.md` says "never change an existing POI's identity fields"; the brief is the explicit approval for this one rename block.
- Search index words: name, keywords, summary, category/badge/status/level labels. New names/summaries must avoid words the search
  tests use as queries: military, religious, residential, economic, infrastructure, lamp, painting, church, chapel, tower, lantern, crowd.

## Names (EN; PT for the two mains goes in keywords)
| id | name | summary |
|---|---|---|
| lamp | St George's Castle (kw: Castelo de Sao Jorge) | Medieval castle and royal palace above Lisbon |
| lamp_religious | Lisbon Cathedral | Romanesque cathedral begun in 1147, below the castle hill |
| lamp_military | Castle Keep | The castle's keep, the last refuge of its garrison |
| lamp_residential | Alfama | Lisbon's oldest quarter, a maze of lanes below the castle |
| lamp_economic | Feira da Ladra | Lisbon's open-air flea market, on Tuesdays and Saturdays |
| lamp_infrastructure | Tram 28 | The yellow tram that climbs the castle hill |
| painting | Ribeira Palace (kw: Paco da Ribeira) | Royal palace on the river, lost in 1755 |
| painting_religious | Igreja da Conceicao Velha | Rebuilt after 1755; its Manueline doorway survived |
| painting_military | Ribeira das Naus | The royal dockyard that built the ships of the age of discovery |
| painting_residential | Casa dos Bicos | A 16th-century house with a front of diamond points |
| painting_economic | Cafe Martinho da Arcada | Lisbon's oldest cafe, open under the square's arcades since 1782 |
| painting_infrastructure | Cais das Colunas | The stone quay where the square meets the river, between two columns |
| camera | The Camera (name kept: not in the brief) | Viewpoint over the Tagus riverfront |
| camera_religious | Jeronimos Monastery | A Manueline monastery funded by the spice trade, in Belem |
| camera_military | Torre de Belem | A fortified lookout guarding the river mouth since 1519 |
| camera_residential | Madragoa | A quiet riverside quarter once home to fishwives |
| camera_economic | Mercado da Ribeira | The riverside food market of Cais do Sodre |
| camera_infrastructure | Ponte 25 de Abril | The suspension bridge over the Tagus, opened in 1966 |
(the config carries the real accents; the table is ASCII only)

## Steps
- [x] 1 Config: JsonUtility load (round trip checked byte-exact first) -> edit -> pretty ToJson -> BOTH copies; 18 points, 2 PT keywords
- [x] 2 New EditMode fixture `DemoWallIdentityTests` (6 tests, passed by name)
- [x] 3 Re-bases (see the final report): CardConfigRoundTripTests, SearchSceneTests (3 places + a new test), PoiCardSceneTests (5 asserts),
      PoiCardSceneTests.Visit, PoiCardPaintingSceneTests, PoiCardFloatingSceneTests, FloatingSlotTests (2), PoiCardNavSceneTests.OpenBlockAndBack (4)
- [x] 4 Full EditMode 2039/2039 (focused); the KeywordList flake red once in the baseline, green after (it is the documented one)
- [x] 5 Targeted PlayMode: 17 fixtures 172/173 (1 red = my missed assert) -> fixed + PoiCardNavSceneTests found by grep -> re-run of
      PoiCardSceneTests + PoiCardNavSceneTests + SearchSceneTests 86/86
- [x] 6 Captures opened and judged: P1c_{painting,lamp}_{en,pt}_peek_390, P1c_painting_{en,pt}_nearby_390, Search_Query_PalaceByName,
      Search_Query_CasteloByKeyword (TestEvidence/Card, TestEvidence/Search)
- [x] 7 Docs: _3.3 TODOs, CONTENT_MAP, PACO / CASTELO text notes, __mixed_TODOs (per-language POI names), 10-structure (new test file)
- [ ] 8 final full EditMode after the scratch fixture was deleted; LivingRoomScene active; final report (no commit)
