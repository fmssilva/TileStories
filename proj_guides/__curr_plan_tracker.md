# Plan tracker -- POI Detail Card step 16 (close-out VERIFY, Domain Review AUDIT mode, no new code)

Brief: (1) the fix list of `_3.1.1` section 11 (15.A-15.F) is CLOSED: proof on disk per item or name what is missing, re-check the
matrix rows the chunks touched; (2) triage every open TODO of `_3.1` (+ its lines in `__mixed_TODOs.md`) to `_3.2` / `_3.3` / a
device-stage-3 list (`__mixed_TODOs.md`) or DROP with the reason; (3) the LATER list of `_3.1.1` section 11 gets a destination too.
Deliver: `_3.1.1_Audit.md` section 12; TODO lines moved and removed from `_3.1`; `_3.1` row 16 done with proof; final report (6.4) + 10-line
domain state. Allowed fixes: doc corrections, a missing test for an unproven item. Commit only if asked.
Tree trusted: green commit 3599632 (EditMode 1606 / PlayMode 778, 15.4); targeted fixtures only to re-prove closed items.

## GATE
- [x] UnityMCP answers (telemetry_status: enabled)
- [x] `git push`: branch was 11 commits ahead; pushed e5822e5..3599632
- [x] Asked (AskUserQuestion) to keep Unity uncovered -> "Ready, go"
- [x] Game view capture opened: TestEvidence/Gate/gate_game_view_16.png (first one landed in Assets/Screenshots, deleted)
- [x] POI Editor capture opened: TestEvidence/Gate/gate_poi_editor_16.png (Scene Configuration + three tabs, window closed, 0 left)
- [x] Oswald Bold SDF.asset not modified (git status clean on Assets)
- [x] Compiles: Editor.log last `error CS` (line 458923) is far older than the last `ExitCode: 0` (line 1281050); console 0 errors

## 16.1 Fix-list proof (read code + test names on disk, re-run the proving fixtures)
- [x] 15.A items 1-4, 15.B 1-3, 15.C 1-3, 15.D 1-3, 15.E 1-4, 15.F 1-5: symbol / test name / capture file located (greps, `ls`)
- [x] Finding: 15.C(2) "Placed by the wall" line is NOT visible at peek (capture PlacedPeek_en: header + audio chip only; the test asserts only that the line is not hidden)
- [x] Gap: BlockFieldDrawer.cs 510 lines still unsplit (audit section 7 asked at the visibility hook; not in the fix list) -> LATER
- [x] Targeted EditMode: 12 fixtures 135 / 135 (3.9 s) + Detail Card tab, round trip, live sync 196 / 196 (27 s), both in the background
- [x] Targeted PlayMode: PoiCardPlaceInArSceneTests, PoiCardLanguageSceneTests, CardTrackGalleryTests, PoiCardLiveUpdateTests 37 / 37 (150 s, background)
- [x] Matrix re-check of the rows the chunks touched (sources_at_end, peek_next_card, preview_language, keep_on_switch, point_label, placed state, language chip, visibility rows)

## 16.2 Triage
- [x] Every `_3.1` TODO line (15-98), section 12 open points, `__mixed_TODOs` card sections, `_3.1.1` LATER list -> destination + reason
- [x] Write into `_3.2` TODOs, `_3.3` new TODOs section, `__mixed_TODOs.md` new section (device / stage 3 / marker / test infra)
- [x] Remove from `_3.1` (TODOs + section 12); pointer line to `_3.1.1` section 12

## 16.3 Docs
- [x] `_3.1.1_Audit.md` section 12 (fix-list proof table, matrix re-check, triage table, domain state)
- [x] `_3.1` row 16 done + proof; header "16 is next" lines updated; `_3.1.1` section 11 status line
- [x] RUN TIMES unchanged (no full run); 10-structure.md untouched (no file added / removed)
- [x] Final report (60-finishing 6.4) + 10-line domain state
