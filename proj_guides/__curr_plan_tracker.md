# Plan tracker -- _3.3 R16 row 4: S1 the elements core (data, registry, rules; no view)

HEAD ba9b2cb (tree clean but proj_guides/__AI_worker.md, 0 commits ahead of origin). Baseline: none (the folder-layout block closed
with full EditMode 2121 + full PlayMode 1310, 0 failed). Brief = approval. Do not commit (the Architect commits). Re-read this file
after a compaction.

## GATE
- [x] git push: 0 commits ahead of origin/master -> nothing to push
- [x] Game-view capture TestEvidence/Gate/gate_s1_gameview.png opened: the wall scene (room scan + painting) renders
- [x] POI Editor capture TestEvidence/Gate/gate_s1_poieditor.png opened: the window only (a first try caught a browser: deleted); 0 capture windows left
- [x] Tree compiles: forced compile, Editor.log ExitCode 0, zero `error CS`
- [x] Oswald Bold SDF.asset / Roboto-Bold SDF.asset unmodified
- [x] Ask Unity's place (4.2.3 c) before the first test run; wait on the Editor log's `[TestRun] ended`

## Audit facts (verified on disk)
- Config written by JsonUtility.ToJson(prettyPrint); shipped config round-trips BYTE-IDENTICAL today (687082 chars, 97 blocks, 149 item rows).
  JsonUtility writes every serializable field incl. empty lists -> any NEW serialized field on BlockItemData / BlockInstanceData / POIData /
  POICardData changes every row / block / POI on save. Hence: all S1 data under RESERVED KEYS inside the existing classes.
- Block keys come from TaxonomyRowKeys.NextFree (Editor): a deleted LAST key is handed out again (not "never reused") -> report, do not fix.
- Card text does not feed search today (__mixed_TODOs 413, a _2.6 decision) -> ElementRules.SearchWords built, not wired into POISearchIndex.
- Fixture POI: painting (Ribeira Palace): header, quick_facts, rich_text, person, timeline, gallery (paco pictures), ... media LivingRoom/CardMedia.

## Decisions (autonomous)
1. Storage: element row = BlockItemData with reserved sub-field keys id / element / motion / strength / tap / validated / ref_scope / ref_block /
   ref_row + repeated `media` entries (asset, text = caption per language, value = credit). Typed wrappers: ElementRow, ElementMedia, ElementRef.
2. Surface data = common fields appended AFTER a kind's own fields when it declares an Elements field: preview_mode, preview_rows,
   component_overrides (open_row = the Open Row override). Reading time = the existing `minutes` (0 = Auto from rows).
3. Ids el_N never reused: high-water mark in the reserved block value element_last_id.
4. shared_elements: NOT a POIData field yet (byte-identity); ElementRefRule takes the shared pool as a parameter (empty today).
5. ElementRegistry standalone with Shared (NavElementRegistry precedent); BuiltInElements fills it.
6. Wire ReadingTimeRule + BlockSummaryRule + BlockStackBuilder (required Elements, no complete row); ElementMotionRule (Auto + Reduce Motion).

## Steps
- [x] 1. Data: BlockFieldType.Elements; BlockKindDefinition surface fields + keys; BlockRegistry Elements validation; HasContent;
         ElementOptions, ElementRow (+ElementMedia), ElementRef, BlockSurfaces (+ id rule) -- compile
- [x] 2. Registry: ElementKindDefinition, ElementRegistry (Validate, Shared), BuiltInElements (14 v1 elements, defaults), ElementRow.Create /
         CreateRef / Duplicate -- compile
- [x] 3. Rules: ElementRefRule (+ElementScope), ElementPreviewRule, ElementRules (minutes, first words, search words, to-validate),
         ElementMotionRule, ElementValidation; wire ReadingTimeRule, BlockSummaryRule, BlockStackBuilder -- compile
- [x] 4. Tests (Tests/Editor/Blocks/Elements/): PaintingStory fixture; ElementDataRoundTripTests (byte-identical x2, old config, full block,
         ids); ElementRegistryTests; ElementRefRuleTests; ElementPreviewRuleTests; ElementRulesTests (+ composition); ElementValidationTests
- [x] 5. Run: new tests -> full EditMode -> mutation proofs (one per rule file) -> full EditMode again
- [x] 6. Targeted PlayMode (background): LivingRoomConfigIntegrationTests, PoiCardSceneTests + grep hits of the changed rules
- [x] 7. Docs: _3.1 (5.3 + elements data), _3.3 (R16 row 4 + Build log), 10-structure (own lines), 40-testing RUN TIMES if moved
- [ ] 8. LivingRoomScene open, final report (60-finishing 6.4)

## Results so far
- New fixtures 71/71 (after one test-order fix); full EditMode 2192, 0 failed, 1 skipped (recorder), background both ends, 185 s
- Mutation: 7 mutants (one per rule file) all killed by their own tests, restored, full EditMode green again
- Docs: _3.1 5.3 + 5.5; _3.3 R16 row 4 + Build log S1 (PlayMode line pending); _3.2 Reading Time sentence; 10-structure; 40-testing RUN TIMES
- Targeted PlayMode 277/277 (8 fixtures), ~22 min, background both ends
