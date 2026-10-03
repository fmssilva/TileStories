# Architect hand-off -- from `_3.1` POI Detail Card to `_3.2` POI Card Navigation (2026-10-03)

Read this once at the start of the new Architect chat, after section 1 "Ground" of `__AI_Architect.md`. It is REPLACED at the
next domain change (it is in git). It tells you where to look, not what to conclude: check the claims on disk.

## 1. Where `_3.1` ended
- Closed 2026-10-02/03: status rows 0-16 done; audit `_3.1.1_Audit.md` (sections 1-11 = the audit, 12 = the close-out
  verification with proof per fix-list item, the triage table 12.5 and the domain state 12.8). `_3.1` has NO open TODO.
- What the card delivers: a bottom sheet of ordered blocks (34 kinds, 9 families, 69 looks; The Lamp fixture has 62 blocks,
  EN + PT). Every wall choice is a POI Editor row with (i), default, undo/save, live sync; fields that do not apply to the chosen
  option are hidden (`FieldShownWhen` / `FieldVisibilityRule`) but keep their values. One owner per medium (audio, video, preview,
  AR placement) with tested release paths. `PoiCardHost` 266 lines, decisions in the pure `CardContextBuilder`; row pools share
  `ElementPool<T>`; card sources scanned for literal looks and strings.
- Last full runs (15.4, Unity in the background): EditMode 1606/1606, PlayMode 778/778 (~27 min). RUN TIMES live in
  `40-testing.md` 4.2. Blocks since the last background full run: 1 (block 16 ran targeted fixtures only).

## 2. What moved where (do not re-triage)
- `_3.2` "## TODOs": the re-plan TODO, family order + `meta` last (vs the 15.2 container option Sources At The End), `open_block`,
  display mode `expandable`, story_chapters buttons that move, a mini control for a placed model (Remove only on the first card;
  "Placed by the wall" not visible at peek -- the one PARTIAL fix-list item, 15.C.2), card real estate at Full, `SwipeGrabRule`.
- `_3.3` "## TODOs": picture tokens, the language chip as a list, `--ts-track-peek`, outline contrast, the "not finished" looks.
- `__mixed_TODOs.md` "POI Detail Card close-out": device checks, ship blockers (media licences), Stage 3/4/6, taxonomy names per
  language, flaky tests, Phase B fixture gaps, `BlockFieldDrawer.cs` (510 lines) split, non-ASCII arrows in `SpecificMarker.cs`.

## 3. Open risks to keep in view
- Visitors get no word that a model is placed while the card is at peek (`_3.2` decides the mini control).
- The Portuguese card still shows English taxonomy and marker names (cross-domain, `__mixed_TODOs.md`).
- No media licence: blocks any build outside the developer's machine.
- place_in_ar, gyro, show_on_wall and Peek Next Card are proven in the Editor only (device list).
- Known once-seen flakes: the Live Play Mode typing test, `PoiCardLanguageSceneTests.ThePick` (`_3.1` section 12).

## 4. How the process stands (already in the guides; pointers only)
- Block size, Worker model tiers, test plan, background run every third block: `__AI_Architect.md` sections 4-5.
- Every answer ends with "Your tasks now" incl. WHERE WE ARE and where Unity must be: `__AI_Architect.md` section 6 item 8.
- Waiting for long runs (sleep, no polling), the AskUserQuestion moments, what the developer can do during a run:
  `40-testing.md` 4.2 and 4.2.3.
- Commits: one line, no trailer (`.claude/settings.json` `attribution.commit: ""`), every commit pushed; the Cowork VM cannot
  reach GitHub, so the next Worker's GATE pushes: `__AI_Architect.md` section 4, `60-finishing.md` 6.4.
- Domain close-out flow (audit -> fix blocks -> verify pass -> this hand-off -> new chat): `__AI_Architect.md` section 5b.

## 5. First job in the new chat
`_3.2` and `_3.3` were planned in ONE pass on 2026-09-26, before Tiers 1-5 were built. Run `proj_guides/skills/Domain_Planning.md`
for `_3.2` (the Architect does it in the chat, or briefs a Worker; decide by its size): ground on what the built card offers now,
research navigation patterns with citations (thesis-grade), make every layout mode and navigation element a POI Editor option,
fold in the `_3.2` TODOs above, and rewrite the status table into small blocks. Then the first `_3.2` Worker brief.
