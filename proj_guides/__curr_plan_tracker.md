# Plan tracker -- _3.2 step 11b (nav_contents, Reading Time, seen ticks in CardLocalState)

GATE (done): pushed 1 (04f92db..2578283); Game view + POI Editor captured and opened (`TestEvidence/Gate/gate_3_2_step11b_gameview.png`,
`gate_3_2_step11b_poieditor.png`, capture window closed); compile 0 error CS / ExitCode 0; SDF fonts clean; EditMode 1856/1856 (63 s) on clean 2578283.
PART 1: none open.

## Steps

- [x] 1. Reading Time (11b.1): `BlockKindDefinition` -- `minutes` common field (Number 0..60, default 0 = none), lazy Fields,
  `ClipLengthField` (audio_guide, video: no field, read the clip), `OffersReadingTime` false on header + navigation kinds;
  `BlockRegistry.Validate` refuses a ClipLengthField that is not an Audio / Video Asset field; pure `ReadingTimeRule`
  (field -> clip rounded up -> none). Tests: `ReadingTimeRuleTests`, registry validation, field lists per kind.
- [x] 2. Seen state (11b.3 data): `CardLocalState.Viewed` / `MarkViewed` / `CountViewed`; `NavScrollRule.HasViewed` (the spy line +
  on screen; the end rule shared with CurrentSection); `NavController.TargetViewed` (once per binding, never while jumping) +
  `EntryOf` via `BlockStackView.EntryOfBlock`. Tests: `CardLocalStateTests` (+ auto: key through real PlayerPrefs, per wall),
  `NavScrollRuleTests` (just above / below / end / off screen / no viewport).
- [x] 3. Kind `nav_contents` + `NavContentsBlockView` (11b.2): rows per target grouped by section, minutes, tick + second signal,
  tap = JumpToBlock; CardStrings (kind_nav_contents, nav_contents_minutes, nav_contents_read) en + pt; Navigation.uss tokens.
- [x] 4. Editor (11b.1, 11b.3c, 11b.4): Reading Time row on Card Content (generic drawer), reset notice counts the read blocks,
  Auto Nav Block (i) compares both kinds, guides (Playmode + Device). Tests: `DetailCardEditorTabTests` (Reading Time real click +
  Ctrl+Z, absent on audio / video / header; reset real click clears viewed incl. the auto: key + notice text; Auto Nav lists both;
  + Add block > navigation > Contents).
- [x] 5. Fixtures: The Lamp block_2 / 5 / 22 / 26 minutes (both config copies, content only).
- [x] 6. Phase A: `CardNavGalleryDefinitions` + nav_contents (4 cases, with / without ticks); `CardNavGalleryTests`.
- [x] 7. Phase B: `PoiCardNavContentsSceneTests` on LivingRoomScene (real drag ticks exactly three, survives reopen, reset clears,
  row tap lands +-1 px, no tick at peek, Auto Nav nav_contents).
- [x] 8. Captures: Lamp contents EN / PT 390 px with ticks, Editor Reading Time row, Auto Nav popup with both.
- [x] 9. Closing: full EditMode; PlayMode Card / Poi fixtures + CardLocalState greps (named).
- [x] 10. Docs (11b.5): `_3.2` status + 5.6 + section 3 + 7; `_3.1` CardLocalState + common fields; `_5.1` Reading Time + reset
  notice; `10-structure.md`; `40-testing.md` RUN TIMES if moved; `__mixed_TODOs.md` ideas.
