# World Editor Lab -- source

## Working rules (same as the main project)
One Lab, one artifact: every version is published over the same artifact, never a copy:
https://claude.ai/artifact/AmWUgE6nA9VLY7YeoUB4ir . This folder is the source of truth; `../World_Editor_Lab.html` is built
from it. Edit the parts, never the built page.

For every feature, in this order:
1. **Implement** in the parts (`js/`, `css/`), keeping the code organised: one concern per file, no dead code, names that
   say what they are. A feature the Unity guides will need gets a comment that names its Unity counterpart.
2. **Build and check it runs:** `python build.py <out.html>`, then the page loads with no console error.
3. **Check it visually** (screenshots, opened and looked at): modern, premium, minimalist, immersive. On a phone and a
   tablet, portrait and landscape, Dark and Light, Premium and Playful. Nothing overlaps, nothing covers the POI, text is
   readable. For a new screen or a design round, two independent critic agents review the screenshots (premium AR design;
   UX and accessibility), then re-check after the fixes.
4. **Add tests** for the base case and the edge cases (`tests/regress.py` feature checks, `tests/sweep.py` placement over
   many camera poses and devices), run them, compare with `tests/report_baseline.json`, then re-baseline.
5. **Publish** over the same artifact; record the version in `../../_4.1.6_Session_Log.md` and update the guides (version 0).

Editor rules: Unity conventions (foldouts, ReorderableList for named lists, Prefab-style overrides: bold + blue bar + ↺);
every section starts collapsed and section titles stick to the top; clicking a config opens the screen that shows it;
the view is full height with only the screen tabs, device buttons, ▶ / ↻, Undo / Redo and ⌨ above it.

## The parts
- `PARTS` -- the order the parts are joined in. Order matters: it is one script, run top to bottom.
- `shell_head.html`, `css/*.css`, `shell_body.html`, `js/*.js`, `shell_end.html` -- the page.
- `media/*.txt` -- the panorama, the sample photos, the fonts and the Playful sprites (data URIs), inlined at `@@NAME@@`.
- `build.py` -- `python build.py ../World_Editor_Lab.html`.

js/, in load order:

| File | What |
|---|---|
| 00_core | constants and small helpers |
| 01_data, 02_media | the sample config (styles, levels, POIs, Wall Life, app, experiences); media sources |
| 03_state, 04_geometry | UI state, undo, selectors; splines → points, offsets, precision |
| 05_render_regions, 06_render_wall | drawing a region / point / label; the canvas, spotlight, the wall as seen |
| 07_screens, 08_wall_life | the Style and Precision screens; Wall Life (K1-K6) state and drawing |
| 09_handles_loop, 10_input, 11_play_hud | Scene handles and the frame loop; mouse and keys; Play, screens, HUD |
| 12_device | the device frame (phone / tablet, portrait / landscape, buttons in the top bar) and the 3D camera: project, unproject, navigation |
| 20_editor_kit | the Unity-like rows, fields, foldouts, help texts |
| 21-27 | the editor tabs: Global Scene, Wall Life rows, Specific Marker, App (+ the phone), cards, Experiences, Decisions |
| 28_editor_shell, 29_json | render the editor, tab buttons; the config.json preview |
| 30_tap (+ css/tap.css) | On tap: base elements (buttons), tag and action components, the styles and their resolver (default › level › POI + field overrides), the renderer shared by the phone and the Wall screen, the action views (card, 360, then & now, 3D preview) |
| 31_main | App › 4 Main AR screen: search (+ Settings gear), category filter, Near, mini-map, the action bar (BAR catalogue, 2-5 buttons), coach mark, out-of-view hint; Pointing inside |
| 32_design (+ css/ds.css, css/fonts.css) | App › 1 Design system, the app's UI kit: foundations (look and mode, colour, type, spacing and shape, depth and motion), components (buttons, panels, controls, lists and messages), patterns (navigation, sheets and dialogs); one phone page per subsection; the component builders (dsBtn, dsRow, dsList, dsNavBar…) used by Start, Onboarding and Explore |
| 33_markers | point (marker) styles and their drawing; the Playful "Tile disc" |
| 34_profiles | visitor profiles (look, mode, audience, time, text, interests, prefs); the look views (D.ds / D.themes are Proxies of the live look); the profile bar; onboarding, Settings, You; Coming soon copy |
| 35_kids_kit, 36_kids_flows | the Playful kit (Azu sprites, 9-slice frames, doodles) and its flows (Azu greeting, sticker, tile album) |
| 99_boot | start-up |

CSS, in load order: `fonts.css`, `lab.css` (the editor and the stage), `tap.css` (On tap), `ds.css` (the app UI kit, both
looks, base) → `kids_00…50_*.css` (Playful, scoped `.tapc.look-playful`) and `prem_{foundations,surfaces,main,tap,flows}.css`
(Premium, scoped `.tapc:not(.look-playful)`), one file per group.

## Safety net (run after every change)

One command, from `lab_src/`:

```
python tests/run_checks.py            # build + regress + compare + both sweeps + lint
python tests/run_checks.py --quick    # the same without the two sweeps
```
It exits non-zero when a step fails; `PYTHONUTF8` is not needed. It needs Python Playwright with Chrome.
A difference from the baseline must be one you meant; when it is, save the new report as the baseline.

The steps it runs, each usable on its own:
```
python build.py out/lab.html
python tests/regress.py out/lab.html out/report.json     # 153 feature checks + every editor header + screens
python tests/compare.py tests/report_baseline.json out/report.json   # what changed against the baseline
python tests/sweep.py out/lab.html --verbose             # 4 devices × 6 styles × 80 poses + side views + big markers: rules R1-R6
SWEEP_INIT="setProfile('visitors','children')" python tests/sweep.py out/lab.html   # the same under Playful
python tests/unity_lint.py                               # 0 problems: only Unity-feasible CSS
```
Expect: regress 153 checks, 0 failed; both sweeps clean (2,640 poses each), page errors 0; lint 0 problems.

### Unity lint
`tests/unity_lint.py` keeps the CSS inside what UI Toolkit can do (`../../_4.1.9d_Unity_Feasibility.md`).
- `kids_*.css` and `prem_*.css` are checked strictly: any problem fails.
- `ds.css` and `tap.css` carry older rules, so they use a **ratchet**: the count per rule may only go down.
  The counts are stored in `tests/unity_lint_ratchet.json`; when a count falls, the file is lowered to the new number.
- Soft shadows are tagged `/* unity: sprite */`, gradients `/* unity: ramp */`, keyframes `/* tween: */`.
- `backdrop-filter` is allowed only on the camera glass, tagged `/* unity: camera-glass */` (in Unity, a blurred copy of the
  camera feed).

### Every editor section has a demo
A section (fold) opens the screen that shows it: `screen` / `step` for a screen, `demo: { step, preview, poi, view }` for a
specific demo (a UI kit page, a component, a POI's tap, an action view). Opening the section, or a click inside it, puts the
phone on that demo (`runDemo`, only when it is not already there). The only sections without one are plans with nothing
built yet (Geo-reference, Ambient sound, Capture & share), the locked Unity sections and the config.json preview; a test
(`every editor section has a demo…`) keeps it so.

## Review tools (`tools/review/`)
- `inventory.py`: lists every editor fold into `inventory.json`.
- `section_shots.py`: one contact sheet per editor fold (6 profile rows × devices) from `inventory.json`.
- `state_shots.py`: contact sheets of named app states (onb, settings, you, main, imm, pan, soon…).
- `fitcheck.py`, `probe.py`: measure clipping / overflow / 48 pt targets.
- `AGENT_PROMPT.md`, `BRIEF_v20_example.md`: how review and fix agents are briefed.

## Design tokens
The tokens follow the Unity theme (`Theme.default.uss`); the final list is `../../_4.1.7_App_Design_System.md`.
- Fonts: `css/fonts.css` embeds the project's own Fraunces SemiBold and Atkinson Hyperlegible Next (`media/FONT_*.txt`, base64),
  and Fredoka for the Playful titles.
- Sizes are written in pt: `--P` = 1 pt (`--u` / 3.9). Type: 28 / 24 / 20 / 16 / 15 / 14 / 13; controls 48, chips 32.
- `dsT()` = the live look's tokens (`D.ds`) with the text size scaled by the visitor's text setting (S / M / L / the phone's,
  `textScale()`); `applyDS()` writes them as CSS variables. The look, the mode, the audience and the time are profile
  dimensions (`js/34_profiles.js`), not token sets.
- Colour: `D.themes` (light, dark) hold the roles; the accent comes from `ACCENTS` (closed list, `setAccent`).
- Glass: `D.ds.glass` = blur (a camera-blur shader in Unity) or solid (no blur, dense tint: the Unity 6.3 look).
