# Current plan tracker -- _3.3 S3b: block templates, the Card Starter's offer, the story Editor's last rows

Brief: `proj_guides/__AI_worker.md`. Spec: `_3.3` R16 row 6b, the S3a review row (1, 5), R14.4, R14.5, R17.1, R17.3.

## GATE
- [x] UnityMCP works (telemetry_status ok); branch 0 ahead of origin (nothing to push)
- [x] Captures: `TestEvidence/Gate/S3b_gate_gameview.png`, `S3b_gate_poi_editor.png` (dark skin) opened; capture window closed
- [x] Compile: forced, `ExitCode: 0`, 0 `error CS`
- [x] SDF fonts unmodified

## Decisions (autonomous)
1. A template = `{ key, name, block }`, the block a content-free BlockInstanceData: every story edit / table is reused as is.
2. Re-apply matches by STRUCTURE (element-kind order, LCS), adds the missing rows in their template place, fresh ids, content never touched.
3. Wall template keys `template_N`, never handed out while a block references them; built-ins `builtin_story_magazine` / `builtin_story_short`.
4. Card Starter: a template pick ADDS one block (no question, one undo); the sets still replace.
5. A segment row with > 4 options = a compact inline dropdown (popup-styled button + inline list): real-click testable.
6. Preview Tap = Nothing: the Open Row still opens the page.

## Steps
- [x] 1 Data: BlockTemplateData + card_settings.block_templates; BlockSurfaces template value + preview_tap / preview_entry; ElementOptions; migrate shipped configs (one key added, nothing else)
- [x] 2 BlockTemplates.cs (pure): built-ins, CopyStructure, SaveAs, NewBlock, Reapply (LCS), NextKey; BlockTemplatesTests 6/6
- [x] 3 Element table reuse (owner + template mode); Block Library > Templates table (open/edit, rename, duplicate, delete+confirm)
- [x] 4 Card Content: Template line (Re-apply, Save as template), "+ From a template...", Card Starter offer (picker: several separators)
- [x] 5 Compact dropdown for > 4 options; Preview Tap + Block Entry rows + ComposedBlockView runtime
- [x] 6 BlockTemplateEditorTabTests 10/10, DrawingNeverWrites +1, LivePlayModeStoryEditorTests +1; re-based: StoryEditorTabTests (dropdown), CardStarterEditorTabTests (row count), ElementRegistryTests (field list)
- [x] 7 PlayMode Phase B (PoiCardStorySceneTests +1; found: the Open Row picks nothing -> Tap Nothing judged by position); targeted 4 fixtures 94 run, re-run of the fixed one 10/10 (+1 gated skip), Unity behind
- [x] 8 Captures (dark): TestEvidence/Editor/S3b/1_templates_table_editing, 2_templates_table, 3_add_block_from_template, 3b_add_row, 4_reapply_result
- [x] 9 FULL PlayMode 1431: 1381 passed, 0 failed, 50 gated skips (82 min, behind); full EditMode 2279: 2278 passed, 0 failed, 1 skip (after adding the two guards' entries for block_templates)
- [x] 10 Docs: _5.1, _3.1, _3.3 Build log + R16 row, 10-structure, 40-testing RUN TIMES; final report; LivingRoom scene ready
