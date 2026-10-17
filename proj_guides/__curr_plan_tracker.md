# Plan tracker -- _3.2 step 8: menu overlay owner, drawer_tree, grid_overlay (+ the two DECIDED TODOs)

Brief: proj_guides/__AI_worker.md + the step 8 brief. Spec: _3.2 sections 4.2, 4.5, 5.3, 9; TODOs [step 8] x2.

## GATE (done)
- [x] UnityMCP answers (telemetry_status)
- [x] git push: 1 commit (4133349..c22876b)
- [x] captures opened: TestEvidence/Gate/gate_3_2_step8_game_view.png, gate_3_2_step8_poi_editor.png (Detail Card > Navigation)
- [x] compile: zero error CS since the last build (ExitCode 0); SDF fonts clean
- [x] baseline full EditMode 1795/1795 (139 s) on clean c22876b (rides on 7a's full PlayMode 868/868)

## Design (decided)
- Owner `CardMenuOverlay` (Runtime/UI/Cards/Navigation), sheet-owned, a layer inserted in the sheet's Layer BEFORE the takeover root.
  API: Open(key, panel or null, onClosed), Close(), IsOpen, OpenKey, OpenMenu() (the bound Menu element). Panel = modal (scrim + panel);
  null = anchored client (More list, kinds list: drawn in place, no scrim). One at a time (an Open closes the previous).
  Closes on: Back (NavBackRule branch), scrim tap, NavController.Jumped, a touch / wheel on the stack (TrickleDown), a stop change,
  every Show / Hide, a takeover opening.
- Elements reach it through BlockBindContext.Overlay (the card's ONE overlay owner).
- NavBackRule.Next(settings, fullViewOpen, overlayOpen, stop): full view -> overlay -> lower -> close.
- NavSlotKeys.Menu built; NavSlotRule.MenuElement; NavSlots binds the menu view (not parented; the owner hosts it when open).
- Header menu button on the sheet root (beside language chip + X), shown only with a bound Menu; tokens for its place.
- CardNavModel.By(groupBy): itself or the lazily built pivot (grid's Purpose / Format switch).
- More slot shows the current overflow section's icon + word + the up arrow; still opens the list / the Menu.
- bar_sheet: the "spy left the family" close removed (jump + scroll closes cover it).

## Steps
- [x] 1. PART 1 owner + clients: CardMenuOverlay, NavController.Jumped, BlockBindContext.Overlay, NavBackRule branch, sheet wiring
      (Back, stop change, Show/Hide, takeover), More + kinds lists through the owner. Tests: NavBackRuleTests (+overlay), CardMenuOverlayTests
      (EditMode, real sheet: one at a time, every close trigger, Back at each layer incl. takeover), view tests updated.
- [x] 2. PART 1.2 (code done; its real-tap test is in Phase B) More marks the overflow section (icon + word + arrow). Tests: EditMode view test (7th of 8), gallery asserts updated.
- [x] 3. 8.1 menu slot + header button + data `menu` (default none) + CardStrings rows + menu icon + USS (scrim, panel).
- [x] 4. 8.2 drawer_tree view (outline families -> kinds with counts, mark, close X, scroll).
- [x] 5. 8.3 grid_overlay view (tiles, Purpose / Format switch on CardNavModel.By, kinds in place) + CardNavModel.By + its Tier 0.
- [x] 6. 8.4 More opens the Menu when set; Editor Menu row (i) / undo / live; DetailCardEditorTabTests + walker + round trip.
- [x] 7. Phase A: gallery entries drawer_tree / grid_overlay x cases (menu opened), CardNavGalleryTests; renders judged.
- [x] 8. Phase B PoiCardMenuSceneTests (real input): stops 195.00 / 337.60 / 796.00 with Menu set; menu button -> drawer -> leaf tap
      jumps + closes; scrim tap closes + scroll kept; Escape closes overlay then lowers; More opens the Menu; More list: Escape closes it
      not the sheet, a drag closes it, a jump closes it; kinds list: Escape; 7th of 8 marks the slot + opens; grid switch + tiles.
- [x] 9. Visuals: drawer + grid on The Lamp at 390 EN / PT, scrim contrast, header with menu button; Editor Menu row capture.
- [x] 10. Full EditMode; Card / Poi targeted set green; FULL PlayMode in the background (editor_is_focused at first poll and end).
- [x] 11. Docs: _3.2 (status row 8 + proof, both [step 8] TODOs removed, 4.2 / 4.5 / 5.2 / 5.3 / 7 built notes), _5.1 (Menu row,
      header button), 10-structure.md, 40-testing RUN TIMES; final report (60-finishing 6.4).

## Progress notes
- Steps 1, 3-6 code + EditMode: targeted 146/146 (CardMenuOverlayTests 14, NavBackRuleTests +2, CardNavModelTests By, DetailCardEditorTabTests Menu row + walker 17, round trip).
- Closing: full EditMode 1813/1813; Card / Poi 28 fixtures 661/661 (1723 s); FULL PlayMode 921/921 (2108 s, partly background). Unity crashed once while idle (native audio assert flood -> OOM), logged in __mixed_TODOs.
