# Current plan tracker: POI Detail Card, step 7C then step 8A (2026-09-28)

Ground truth for the two parts. Tick a line only when its proof was re-derived this session.

## Gate
- [x] UnityMCP works (telemetry_status ok); Game view capture opened; POI Editor window capture opened (read in POINT coordinates)
- [x] Commit "POI card: Tier 2 group B ..." done (stale .git/index.lock removed first)
- [x] Baseline: compile clean, EditMode 1229/1229, PlayMode 486/486

## Part 1 - step 7C (close-out, no new features)
- [x] 1.1 Fixture: The Lamp gets `related` carousel (manual: Lamp - Military + others) and `related` next_along_wall (nearest), before `sources`; both config.json copies
- [x] 1.2 Phase B test (PoiCardSceneTests): real tap on a carousel card selects via SelectionEventBus, card rebinds; next_along_wall = neighbour the WallAxisRule says (computed independently); wrap on the running wall; catalog-order test updated
- [x] 1.3 Captures in the real scene: related carousel + next_along_wall; today_map static + bridge (existing PNGs) - checklist answered
- [x] 1.4 Editor captures + checklist: Card Content rows of hotspot_image, wall_locator, today_map, related on The Lamp; fix what shows
- [x] 1.5 _5.1 updated (pending list, Card Content section, capture procedure note); _3.1 row 7C done with proof; [7C] TODO lines deleted
- [x] 1.6 Full EditMode + PlayMode green; commit checkpoint suggested

## Part 2 - step 8A (Tier 3 group A), one kind at a time through section 7
- [x] 2.1 CardLocalState: ICardStateStore + MemoryCardStateStore + PlayerPrefsCardStateStore, pure CardLocalState (wall + POI + block key), ICardEvents seam; tests; Detail Card > Test "Reset card state" row
- [x] 2.2 ContentSeenRule + stack/host seam (show_after_viewed); tests
- [x] 2.3 knowledge_check: definition, KnowledgeCheckRule (pure), round trip, view + Play.uss, Phase A entries + tests, Editor rows captured, Lamp fixture, Phase B
- [x] 2.4 feedback: definition, view + Community.uss, Phase A, event through ICardEvents, Editor rows captured, Lamp fixture, Phase B
- [x] 2.5 Docs: _3.1 (rows 7C + 8A, TODOs, design history), 10-structure.md, _5.1, __mixed_TODOs.md
- [x] 2.6 Final: EditMode + PlayMode green, 0 error CS, captures named
