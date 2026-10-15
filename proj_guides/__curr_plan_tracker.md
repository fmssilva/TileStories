# Current plan -- _3.2 POI Card Navigation, step 4 (BlockRef, Open Block, system Back)

Brief: proj_guides/__AI_worker.md (2026-10-15). Re-read it and this file after a compaction.

## GATE (done)
- [x] UnityMCP answers (telemetry_status)
- [x] git push: 1 commit (7815cf7..ddffe39)
- [x] captures opened: TestEvidence/Gate/gate_3_2_step4_gameview.png, gate_3_2_step4_poieditor.png (Detail Card > Navigation)
- [x] compile: Editor.log ExitCode 0, no error CS
- [x] SDF fonts (Oswald Bold, Roboto-Bold): clean, nothing to restore
- [x] baseline full EditMode 1694/1694 (106 s, background) on ddffe39

## Decisions
- BlockRef value = a block key of the SAME POI, stored in `value` (like PoiRef).
- Pure `BlockRefRule` (Runtime/Blocks): JumpTargets(entries, isRevealed) = keys in the scrolling stack (not the header, not a
  pinned-top / footer look, not waiting for Show After Reading); Problem(poi, key, built) = None / Missing / NotInScroll / Itself.
  Reused later by hotspot pins (step 10) and nav_bento tiles (15).
- `BlockBindContext.JumpTargets` filled by `PoiCardSheetView.Show` (null = the caller has none: no Open Block button drawn).
- Repeated actions: open_block keyed by action + target -> two Open Block buttons to DIFFERENT blocks = no warning; same block twice = warned.
- Open Block words: the row's own -> the target's Heading -> card text `open_block_button`.
- Validate: "Options / OptionLabels only on a Choice field" (refuses a BlockRef with a fixed list); ChoiceDefault / LibraryDefault already refused generically.
- Back: NavBackRule decides, PoiCardSheetView.Back applies (close -> CloseRequested, like the X), PoiCardHost.Update wires Keyboard.current.escapeKey.
- Editor popups (three rows + BlockRef) open native menus: driven through the window's write path + Ctrl+Z (documented limit).

## Steps
- [x] 1. Part 1: PoiEditorWindowHost asserts EditorGUI.activeEditor once (constructor); PoiEditorWindowHostTests 2/2
- [x] 2. BlockRef core: enum, HasContent / HasItemContent, Validate Options check, BlockRefRule + BlockRefRuleTests 6/6
- [x] 3. Open Block: action + target sub-field + icon + card text; ActionsRule / RepeatedActionRule; context JumpTargets; view Run;
        Lamp block_18 + two Open Block rows (both config copies, +65 / -0 each); Tier3GroupBRulesTests +3
- [x] 4. Editor BlockRef row (+ options, warnings); drawer split into BlockFieldDrawer + .Values / .References / .Assets;
        DetailCardEditorTabTests +2. Full EditMode 1705/1705 (109 s, background)
- [x] 5. Escape probe: the scene's EventSystem (InputSystemUIInputModule) turns a real Escape into ONE NavigationCancelEvent to
        the focused search field's text element; the search keeps words, query, results and focus -> no collision (kept as a test)
- [x] 6. Back: back_button data + options, NavBackRule + NavBackRuleTests 5/5, PoiCardSheetView.Back, PoiCardHost.Update wiring
- [x] 7. Editor rows + (i) + guides; DetailCardEditorTabTests +1, walker 9 navigation fields, round-trip default. EditMode 1711/1711
- [x] 8. Phase B PoiCardNavSceneTests 15/15 (+5: probe, Open Block tap, dead target after a live edit, Escape walk, none + live)
- [x] 9. Captures judged: Editor/170_CardNavigation_JumpBack_620(_b) (gap fix), Editor/171_CardContent_OpenBlock_620_b,
        Search/Search_Card_OpenBlock_PillRow_Lamp_390 (icon redrawn as a down arrow)
- [x] 10. Closing: full EditMode 1711/1711 (77 s), FULL PlayMode 793/793 (1854 s); docs _3.2 / _3.1 / _5.1 / 10-structure /
        __mixed_TODOs / RUN TIMES; final report 6.4
