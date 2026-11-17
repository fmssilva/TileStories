# Plan tracker: merge P1a.2 (router, devices, shot tools, first publish)

Brief: `__AI_worker.md` "And now". Lab: `proj_guides/_4.2_TileStories_Lab/lab_src`. GATE: Lab. No git writes.
While `scratchpad/FULL_RUNNING` exists: no test, self-test or shot tool (build.py only).

## Decisions
- The router's go is `goScreen` (many modules keep a local `go`).
- New area `src/screen/`: `router.js` (screens, go / back, overlay stack, action views by key, `screenChanged`), `device.js`
  (W's 4 devices, `ui.device`, the frame's fit, W's device buttons), `explore.js` (the non-AR path: Explore + No camera).
- The router is the only writer of `ui.view`, `ui.stop`, `ui.history`, `ui.overlays`, `ui.device` (a structure-check count, 0).
- Screen ids kept (`card` = the wall with the card sheet over it; tour data and row demos name it; P4 renames with W's Main AR).
- Overlays: the card sheet (its stop, `ui.stop`, the wall screen's base layer), Settings, Contents, the guide's ✦ menu,
  Ask about the view, Things to try: their old flags (`ui.settingsOpen`, `ui.contentsOpen`, `G.fab.menu`, `G.cam.open`,
  `G.gd.tryOpen`) replaced by `ui.overlays` (one truth). Back closes the top overlay, then the sheet, then the screen.
- Action views by key (W's On tap keys): `card`, `tour`, `ask`; P4/P5 add `model_screen`, `pano`, `then_now`...
- Events through `core/ext.js`: `configChanged` (undo.js: an Editor step, undo, redo), `profileChanged` (the visitor's
  Settings, the guide's Settings, the preview bar, Forget), `screenChanged` (the router, once per change).
- No camera: `LAB.noCamera` (the preview bar's switch, standing in for the phone's camera); `cameraOn()`; Explore is home then.
- Page renamed `tilestories-lab.html`; `tools/publish.py` lists the files map, sizes and batches, `--stage` builds the layout.

## Steps
1. [x] Before captures saved (the page built from HEAD: scratch `p1a2/before/before.html`) -- captured once the flag is gone.
2. [x] Router + migration script (src and tests), check_structure count, build 0.
3. [x] Devices: DEVICES, ui.device, fit, device buttons, landscape stops; SCREENS / LAB.screen / fitPhoneHeight gone.
4. [x] Explore + No camera + wall entry; action views.
5. [x] Events configChanged / profileChanged.
6. [x] Page rename + publish tool + temp-folder proof.
7. [x] Shot tools (tools/review: inventory, state_shots, section_shots) ported.
8. [x] Look: before/after identical (wall, card, dock, phone portrait); device cells + Explore light/dark judged.
9. [x] Tests: test10d (router, non-AR path), test10e (devices), test10f (publish), each MUTATE=1; core set joins.
10. [x] Docs: PROTOTYPE_STRUCTURE, README, STATE_MAP.
11. [x] Close: self-test every walk bad=0, `node run_tests.js --core`.
