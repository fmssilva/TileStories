# P1a.1 · State roots, undo, local server (merge round 2) -- builder brief

**WORKER:** Opus, effort High. This is a cross-cutting refactor of shared ground: the state every module reads.
**SUBAGENTS:** allowed for read-only audits (Sonnet). Never two builders editing `src/` at once.

**TEST PLAN:**
- No opening baseline: the last commit (`c8aa14d`, P0) is green.
- This round is SHARED GROUND, so it closes on the **CORE** tier:
  - `tests/runtest8.ps1` with every walk, `bad=0`;
  - `node run_tests.js --core`.
- The FULL tier then runs on a `git worktree` alongside the next round (rounds since the last full: 2, merge P0 and this one).
- Every test file has a 10-minute limit. One test file at a time.

**Read first, every line:**
- `proj_guides/_4.2_Lab_Merge_Plan.md` §1-3: roots, the Unity data homes, no shims, the round method.
- `proj_guides/skills/Feature_Prototyping.md` step 3 and step 5.
- `lab_src/PROTOTYPE_STRUCTURE.md`: Rules, Build, Lab-only.
- `lab_src/README.md`: Build, Prove it, Levels.
- `.clinerules/00-process.md`: Shared Core Rules.
- `.clinerules/10-structure.md`: search it for where each authored setting lives in Unity (ThemeAsset, the wall's config.json, `CardStrings.asset`, the card / block config classes).

**Where:** `C:\Users\franc\Desktop\TileStories\proj_guides\_4.2_TileStories_Lab` only. No git writes: the Architect reviews and commits.

## Why
Today one global `S` holds about 4,000 references in 95 of 102 modules. It mixes four kinds of state:
- what the author sets (`S.guideCfg`, `S.cardCfg`, `S.guidance`, `S.blockDefs`…);
- what the visitor chose (`S.vis`, `S.gdVisitor`, `S.lang`, `S.reduce`…);
- what is happening now (`S.view`, `S.stop`, `S.sel`, `S.dockH`, `S.tab`…);
- Lab-only tools.

Because of this mix:
- the visitor's Settings can overwrite the author's config (the voice bug);
- there is nothing to undo;
- nothing maps cleanly to Unity's data.

The merged Lab needs three roots, as the plan says:
- **`D`:** authored. Undoable. Each part names its Unity home.
- **`P`:** the visitor's profile and choices. Cleared by Forget. Never writes `D`.
- **`ui`:** runtime.

Lab-only state goes in a fourth root, **`LAB`** (scenes, self-test, probes, consoles), so it is visibly never ported.

## Part 1 · The State Map
Write `lab_src/STATE_MAP.md` (a new doc, version 0) before changing code. It is one table with a row per top-level `S` key, and each row gives:
- the key;
- what it holds, in a few words;
- its root (`D` / `P` / `ui` / `LAB`) and new path. Group `D` by its Unity home: `D.guide`, `D.card`, `D.guidance`, `D.wall`, `D.words`, …;
- **its Unity home**: the asset or class, from `10-structure.md` (or "new: guide-config asset", or "Lab-only");
- the modules that write it.

Rules for the map:
- **Mixed keys are split.** A key that mixes kinds is split by field. Example: `S.guideCfg.voice`, the author's default, goes to `D.guide`; the visitor's choice of the same thing goes to `P.guide`. Do not move the voice logic itself; that is P2. Only give the visitor's choice its own place where Settings already writes it, and make Settings write `P`.
- **Names follow Unity.** When `10-structure.md` names the field, use that name. Keep names otherwise; this is not a renaming round.
- **Mark every guess** "(guess)".

## Part 2 · The move (mechanical, in one pass)
- **Apply the map with a script you write** (keep it under `tools/`, run once, then delete it or keep it if it is reusable).
  - Each `S.<key>` reference becomes its new path. That includes `S[k]`-style dynamic access: find every one and handle it by hand.
- **Persistence:** if the Lab saves or loads state (localStorage, the JSON export, scenes that set state), the saved shape follows the new roots.
  - An old saved state is simply dropped, with a console line saying why.
  - No migration shim.
- **No aliases.**
  - `S` must not exist at the end: no `var S=D`, no getters, no Proxy.
  - `check_structure.js` gains a count, "references to `S.`", with 0 allowed.
- **Defaults.** The rule "a default set in one module only" keeps holding.
- **Tests and scenes** that poke `S.` are updated by the same script. Their logic does not change.

## Part 3 · Undo / redo on D
- Port the World Lab's undo (`_4.1.2_Prototype/lab_src/js/03_state.js`: `snap` / `undo` / `redo`, 300 steps) into `src/core/`.
  - It works on `D` only.
  - A snap happens once per Editor change, through the Editor kit's one write point. Find it in `editor/shell.js` / `editor/components.js`; there must be one.
- **Measure the cost of a snap.**
  - If a JSON copy of `D` is over 30 ms (the guidance catalogue is large), snap only the changed path: `{path, before, after}`. Choose one, and say why.
- **Buttons:** Undo and Redo in the Editor's top bar, plus Ctrl+Z / Ctrl+Y outside text fields.
- **Tests:**
  - undo an Editor change → the phone shows the old value; redo → the new one;
  - a visitor action (`P`) or a tap (`ui`) never enters the history.

## Part 4 · The local server and assets out of the page
- `tools/serve.py`: a tiny local http server for `lab_src` (Python's `http.server` is enough), at a fixed port with a clear message.
- **Tests:** `run_tests.js`, `runtest8.ps1` and the shot tools open the page over `http://localhost:<port>/…`. Each starts the server itself if it is not running.
- **Assets out of the page.** With http, `build.py` writes `block-components-lab.html` plus `assets/`: the 5 GLBs, the pictures and the thumbnails.
  - Then confirm, over http, that the GLBs, the canvas read-back (Photo of the view) and `guidance/present.js`'s thumbnail measuring all work.
  - The page should drop well under 5 MB.
- **Double-click on the page.** Opened by double-click (`file://`), the page still runs, but the 3D mascot and Photo of the view cannot load assets there. Show one honest line in the Editor at that moment: "Open with tools/serve.py for the 3D mascot and photos". No silent fallback.
- **Publish.** The record copy and the publish become a page plus its `assets/` files. Write the exact steps in README. Do not publish: the merged Lab's first publish is P1a.2.

## Part 5 · Docs
- `PROTOTYPE_STRUCTURE.md`: the rules gain "three roots + LAB" and "undo on D".
  - The module rows that changed are updated.
  - "The build only joins and embeds" is reworded to match `assets/`.
- `README.md`: the server, the new build output, the tests over http.
- The map (Part 1) stays as the reference that P1b and P2 extend.

## Look (Feature_Prototyping step 3)
Nothing the visitor sees may change. Capture before and after:
- the card at peek / half / full;
- the guide dock during a tour;
- Settings;
- the Guide tab and the Detail Card tab.

Each in light and dark. Compare them and say what differs; the answer should be nothing but timing. Add the Undo / Redo buttons to the Editor capture.

## Done when
- the build is green: structure 0, `S.` 0, the lint holding;
- CORE is green (self-test `bad=0`, `--core`);
- the undo tests pass;
- the page is under 5 MB over http;
- the captures are identical.

Final report:
- changes by area;
- the State Map's summary: how many keys went to each root, and the open guesses;
- test results with their counts;
- the snap timing;
- the captures (paths);
- anything left open.

**OUT OF SCOPE:**
- the router;
- devices;
- tokens;
- the voice logic (P2);
- any visual change.
