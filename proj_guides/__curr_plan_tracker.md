# Plan tracker -- _3.2 step 7b: dock, bar_sheet, next_up (bottom slot)

Brief: three elements in the BOTTOM slot 7a built. No new owner, container, stops, chrome arithmetic. No change to `CardChromeRule`.
If a NEW shared part or a `CardChromeRule` change turns out to be needed: STOP and report.

## GATE (done)
- [x] UnityMCP answered (TileStories@d41388e4, telemetry_status true, editor_state ready)
- [x] git push: 1 commit (9718433..4133349)
- [x] Game view capture `TestEvidence/Gate/gate_3_2_step7b_game_view.png` opened (flat gallery-scene colour, no other app)
- [x] POI Editor capture `TestEvidence/Gate/gate_3_2_step7b_poi_editor.png` opened (Detail Card > Navigation, Bottom Bar None); window closed, none left
- [x] tree compiles (last Editor.log compile ExitCode 0, console no errors); tree clean on 4133349
- [x] SDF fonts clean (no diff on Oswald / Roboto)
- [x] EditMode baseline 1769/1769 (113.8 s, Unity not focused) on 4133349 -- the commit I trusted for the full PlayMode 868/868

## Design (decided)
- Pure: `NextUpRule` (which target is next, its 1-based position, total, finished, current-not-a-target -> first), `BarSheetRule`
  (a tap opens the kinds only on the CURRENT section that has 2+ kinds, else it jumps).
- Views (Runtime/UI/Cards/Navigation): `BottomBarNavView` opened up (protected: element key, Tapped, labels-by-setting, section-changed);
  `DockNavView` and `BarSheetNavView` are thin subclasses (the TabsNavView -> IconTabsNavView precedent); `NextUpNavView` standalone.
- `NavController.HeadingOf(blockKey)`: the words the stack draws above that block (so Up next names what the visitor will see).
- Strings (CardStrings.cs Keys + .asset en/pt): nav_next_up, nav_next_up_position ({0} of {1}), nav_next_button, nav_next_up_done.
- Registry: BuiltInNavElements Dock / BarSheet / NextUp (+ Editor labels/help); Choice Sections / Sections / Blocks.
- USS: Navigation.uss only, tokens only.

## Steps
- [x] 1 Pure rules + Tier 0 (`NextUpRuleTests`, `BarSheetRuleTests`) on the shipped Lamp / Military / a reveal
- [x] 2 Strings (keys + asset rows) + HeadingOf + BottomBarNavView hooks
- [x] 3 `dock` (view + USS + registry + Phase A check)
- [x] 4 `bar_sheet` (view + kinds list + USS + registry + Phase A check + open-list test)
- [x] 5 `next_up` (view + USS + registry + Phase A check + finished render)
- [x] 6 Editor: registry-listed rows (no window edit expected), update Playmode guide + help, fix the Editor tests that list the bottom slot
- [x] 7 EditMode full green
- [x] 8 Phase B `PoiCardBottomElementsSceneTests` (dock drag, bar_sheet two taps, next_up Next + last) -> green
- [x] 9 Captures judged (Lamp Full EN + PT per element, Phase A renders)
- [x] 10 Targeted PlayMode: every fixture with Card or Poi (+ grep adds) green
- [x] 11 Docs: _3.2 row 7b + Built at 7b, Editor line; _5.1 checked; 10-structure.md; 40-testing RUN TIMES if moved
- [x] 12 Final report (60-finishing 6.4)

## Progress notes
- EditMode full 1795/1795 (90 s) after restoring the two rules from a deliberate mutation (13 tests went red, proving the Tier 0 / view tests bite).
- Phase A CardNavGalleryTests 63/63; Phase B PoiCardBottomElementsSceneTests 8/8 (Up next walked The Lamp in 40 presses, 56 -> 57 targets; widest PT dock pill 326 px).
- Captures judged; two fixes from them: bar_sheet arrow beside the icon, Next as an accent outline (not a second filled pill), finished look weighted.
- Targeted PlayMode close: 27 fixtures with Card / Poi in the name, 640/640 in 1928 s (Unity in front). Probe for Next on short cards: all gallery cards scroll; the non-scrolling edge is derived from NavScrollRule and documented, not tested.
- Final: EditMode 1795/1795 (86 s) after removing the probe; fonts clean; no InitTestScene; docs written (_3.2 row 7b + 5.2 + Editor line, _5.1 checked, 10-structure, 40-testing RUN TIMES, __mixed_TODOs).
