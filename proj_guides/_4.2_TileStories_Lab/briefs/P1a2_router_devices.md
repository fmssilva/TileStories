# P1a.2 · Router, devices, shot tools, first publish (merge round 3) -- builder brief

**WORKER:** Opus, effort High. A new shared owner (the router) and the phone shell, both shared ground.
**SUBAGENTS:** read-only audits only.

**TEST PLAN:**
- No opening baseline: P1a.1 (`8c34c32`) closed green on CORE.
- The FULL tier is running on a `git worktree` of `8c34c32` at `LAB_PORT=8643`, held by the Architect.
  - **While the file `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\2bab6f8c-33fb-4cd2-9e78-7d5d95377279\scratchpad\FULL_RUNNING` exists, run no test file, self-test or shot tool.** Two runs slow each other past their limits.
  - Do the code, the design and the docs first, and check the flag before each run.
- This round closes on **CORE**: `tests/runtest8.ps1` with every walk, `bad=0`, plus `node run_tests.js --core`. Add this round's new test files to the core set.
- Rounds since the last FULL: 3 (P0, P1a.1, this one). The FULL in the worktree resets that count when it is green.

**Read first, every line:**
- `proj_guides/_4.2_Lab_Merge_Plan.md` §1-3: layers, the router with the non-AR path, the Screen layer's Unity mapping, no shims.
- `proj_guides/skills/Feature_Prototyping.md` step 3.
- `lab_src/STATE_MAP.md`.
- `lab_src/PROTOTYPE_STRUCTURE.md`: Rules, Build, the phone and editor modules.
- `lab_src/README.md`: Build, Prove it, Levels.
- `.clinerules/30-ui-content.md`: "Every core feature must work without the camera".

From the World Lab, read and port:
- `_4.1.2_Prototype/lab_src/js/12_device.js` (the `DEVICES`, bezel, scale-to-stage, `isLand()`);
- its device buttons in `shell_body.html` / `28_editor_shell.js`;
- `tools/review/state_shots.py` and `section_shots.py`.

**Where:** `_4.2_TileStories_Lab` only. No git writes.

## Part 1 · Events
- Add the named events of the plan (§2 Core) through `core/ext.js`'s `emit`, at the one place each happens:
  - `configChanged`: `D` changed, through the Editor's write point that undo uses;
  - `profileChanged`: `P` changed;
  - `screenChanged`: the router.
- Do not convert existing listeners now. A module joins an event only when it is touched.

## Part 2 · The router (`src/screen/router.js`, Unity: the UI Toolkit screen stack)
- **One registry of screens:** `registerScreen(key, {render, title, ar:true|false})`.
- **Navigation:**
  - `go(key, params)` and `back()`;
  - an overlay stack, `pushOverlay` / `popOverlay`, for sheets and dialogs over a screen;
  - action views by key, `registerView`: card, settings, the guide's overlay; P4 and P5 add 360, Then & now and 3D.
- **What exists today becomes registered screens and overlays, with the same look:**
  - the wall (AR);
  - the card sheet;
  - Settings;
  - the guide's dock and overlays.
- **Each screen change goes through the router,** so `ui` holds the stack. No module sets screen visibility on its own any more.
- **The non-AR path (new, minimal):** an **Explore** screen without the camera.
  - It is a plain list of the points that have a card, built from the existing row and list parts and the existing card styles; real design is P3/P4.
  - A row opens the card, and the guide works there (a tour starts from the card).
  - Reachable from the wall (a button) and from a preview-bar switch, "No camera".

## Part 3 · Devices (W's shell; Unity: the Game view presets)
- Replace `SCREENS` (phone / land / tablet) with W's four devices:

  | Device | Size |
  |---|---|
  | phone portrait | 390 × 844 |
  | phone landscape | 844 × 390 |
  | tablet portrait | 820 × 1180 |
  | tablet landscape | 1180 × 820 |

  - Bezel, scale-to-stage, `isLand()`.
  - Device buttons in the top bar, as in W: phone and tablet; a second click turns it.
  - `ui.device` replaces the old screen key (update STATE_MAP).
- Keep B's preview-bar extras: language, dark, reduce motion, the grip.
- The card and the guide must stay correct in all four. Landscape and tablet layouts that were never designed are noted per screen in the report, not designed now (P3 / P4).

## Part 4 · Profile-matrix shot tools
- Port W's `state_shots.py` / `section_shots.py` to this Lab as `tools/review/state_shots.py` and `section_shots.py`.
  - Python Playwright with Chromium, contact sheets with PIL, the page over the Lab server.
- Rows are profiles. For now the profile columns are mode (light / dark) × text (default, larger); look (Premium / Playful) joins at P1b.
- Columns are the 4 devices.
- **States:**
  - the wall;
  - Explore;
  - the card at peek / half / full;
  - Settings;
  - the dock in a tour.
- **Sections:** every Editor fold with a demo; read the list from the Editor itself, as W's `inventory.py` does.

## Part 5 · The first publish, prepared
- Rename the build output to `tilestories-lab.html`. The page title becomes "TileStories Lab".
- The record copy `../TileStories Lab.html`, its `../assets/`, and the old `../Block Components Lab.html` keep their link as a frozen reference.
- **The published file set:**
  - the page;
  - `assets/`;
  - the voice packs and music the page reads (`../voicebench/out` public sets only, never `out-private`; `../music`).

  `tools/publish_list.py` prints the exact `files` map (published path → source path) and the total size.
  - Artifact limits: 16 MB per file, 64 MB and 255 files per publish, so a bigger set is published in several parts.
  - Check the relative URLs the page uses for voicebench and music, and make them work both from the server and from the published layout.
- The Architect does the publish with that list. You only prepare it and prove the layout with a local copy served from a temp folder.

## Part 6 · Docs
- PROTOTYPE_STRUCTURE: the router and screen rules ("every screen change goes through the router"), the new modules.
- README: devices, Explore / No camera, the shot tools, publishing.
- STATE_MAP: `ui.device` and the router's stack.

## Look (step 3)
Capture with the new tools:
- before and after for the wall, card and dock on phone portrait: they must be identical;
- the new device and landscape cells, plus Explore, light and dark: judge them, fix what is broken (clipped, overlapping, unreadable), and list what only needs design later.

## Tests
- **Router:** go / back / overlay order, with `screenChanged` emitted once per change.
- **The non-AR path:** No camera → Explore → a row opens the card → a tour starts and speaks a line.
- **Devices:** each device's size and orientation, with the card's stops correct in each.
- **The publish layout** from the temp folder: the page, a GLB, a voice line and a music track load.
- `MUTATE=1` for each new file.

## Done when
- CORE is green;
- the new tests pass with their mutations;
- the captures are judged;
- the publish list is ready.

**OUT OF SCOPE:**
- tokens and looks (P1b);
- the voice service (P2);
- W's app screens (P4);
- designing landscape or tablet.

**Final report:**
- changes by area;
- test counts;
- capture paths and verdicts;
- the publish list and its size;
- what is left open.
