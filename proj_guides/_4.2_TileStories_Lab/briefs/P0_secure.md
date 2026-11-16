# P0 · Secure (merge round 1) -- builder brief

Plan: `proj_guides/_4.2_Lab_Merge_Plan.md` (read sections 1-3 fully). Rules: `.clinerules/00-process.md` (Shared Core Rules:
no shims / wrappers / adapters; version 0 docs), `proj_guides/skills/Feature_Prototyping.md`.

State at the start:
- Two local commits: "Track the World Editor Lab v20.1…" and "Move the Block Components Lab to _4.2_TileStories_Lab…".
- The Block Lab builds from `proj_guides/_4.2_TileStories_Lab/lab_src` (`python build.py`: structure 0/0/0/0, 14.6 MB).
- The World Lab passes `python proj_guides/_4.1.2_Prototype/lab_src/tests/run_checks.py`.

Two builders work in parallel, each only in its own folder:
- **P0-W** in `_4.1.2_Prototype/lab_src`;
- **P0-B** in `_4.2_TileStories_Lab`.

Neither runs git commands that write. The Architect reviews and commits.

Everyday rules for both:
- Every test file runs with a 10-minute limit.
- Check the CPU before a long run.
- Never run two Playwright test files at once in the Block Lab: they slow each other past their limits.

## P0-W · the World Lab's last cleanup before it freezes (Sonnet, Medium)
1. **Remove dead code.** Prove each item dead by grep before you delete it:
   - `D.ds.audiences`, `D.app.theme`;
   - `resolvedLook` / `lookCache` (careful: `lookCache=null` is written by `tools/review/state_shots.py`; remove it there too if it is dead);
   - `STEPS`, `ACT_ICON`;
   - the `sel-ring` and `kidtone` CSS;
   - the unused Playful colours at `js/34_profiles.js:100`.

   If an item turns out to be used, leave it and say where it is used.
2. **Add regress checks for v19/v20** in `tests/regress.py`. They are the spec the port will be measured against, so make them assert behaviour, not pixels:
   - the profile bar switches look, mode, audience, time and text;
   - onboarding v2: the look first, then 7–11 | 12+ with one example card, then time and interests;
   - Settings has its 5 areas;
   - `D.profile.prefs` apply at once: names off, arrows off, mini-map;
   - You: counts and saved places;
   - the action bar: 2–5 buttons, with Settings hidden when the gear shows;
   - the immersive card: photo, 2 lines, Story first;
   - Coming soon: both POIs, copy per audience, the What's coming page;
   - every tap style shows the badge.

   About 10–15 checks.
3. **Re-baseline.** Run `python tests/run_checks.py`: all green. The new checks are the only differences; save the report as `tests/report_baseline.json`. Update the counts in `lab_src/README.md` and `proj_guides/_4.1.13_Handoff.md`.
4. **Freeze.** Add one line at the top of `lab_src/README.md`: "Frozen at v20.1 (2026-11-16): the source of the port into `_4.2_TileStories_Lab` (`_4.2_Lab_Merge_Plan.md`). Bug fixes only."

Done when `run_checks.py` is all green on the new baseline.

## P0-B · the Block Lab ready to be the home (Opus, Medium)
1. **Assets out of the page.**
   - `build.py` writes `block-components-lab.html` plus an `assets/` folder beside it with the 5 mascot GLBs and the pictures. The page loads them by relative URL instead of embedding them.
   - Tests and the self-test open the page from the same folder, so the relative URLs work. Check that `file://` loading of GLBs works with the three.js loader (fetch on file:// may be blocked). If it does not, keep the GLBs embedded and report: the publish step will use a multi-file artifact instead.
   - Target: a page under 5 MB.
   - "The build only joins and embeds" stays true: copying files into `assets/` is a build step, and code is never rewritten.
2. **Fonts embedded.** Today the Block Lab loads its fonts from Google Fonts.
   - Embed Fraunces SemiBold and Atkinson Hyperlegible Next (the weights it uses) as `@font-face` data URLs, the way the World Lab does: `_4.1.2_Prototype/lab_src/media/FONT_*.txt` + `css/fonts.css`. Copy those files.
   - A weight the World Lab lacks (Atkinson 600?) is added from the same family.
   - Remove the Google Fonts link.
3. **The Unity lint, in the build.**
   - Port `_4.1.2_Prototype/lab_src/tests/unity_lint.py` to `lab_src/tools/unity_lint.py`. Same rules and tags (`unity: sprite`, `unity: ramp`, `tween:`, `unity: camera-glass`, `backdrop-filter:none` allowed).
   - Add a ratchet over EVERY CSS file under `src/`: `tools/unity_lint_ratchet.json` holds per-file, per-rule counts. A CSS file that is not in the json is linted strictly, so new or fully ported files start clean.
   - `build.py` runs it after `check_structure.js` and fails the build if a count goes up, the way the structure check does. Expect about 490 items today.
4. **`check_structure.js`.** The acorn path is hard-coded to `C:/Users/franc/Desktop/TileStories_Website/node_modules/acorn`. Make it resolve normally (a local `package.json` devDependency, or `require.resolve` with a clear message), with no machine-specific path.
5. **Name what is Lab-only.** In `PROTOTYPE_STRUCTURE.md`, add a section "Lab-only (never ported to Unity)". It lists the scenes, the self-test, hash routes (`#selftest`, `#probe`, `#dcshot`, `#pshot`…), the preview bar's tools, the Ask Azu console and the voice bench.
   - Any Editor row or button that is Lab-only but looks like an app feature gets a visible "Lab only" tag, the Unity `DevFeatureBuildGuard` idea.
   - Keep this small; list what you tagged.
6. **Rename in the docs only.**
   - Update the folder README's title and first paragraph: "TileStories Lab (the merged Lab; was the Block Components Lab)", with a pointer to the merge plan.
   - The page title and the artifact change at P1a, not now.
7. **Tests (feature tier):**
   - `python build.py` (structure 0, lint ratchet holding);
   - the test files that load the mascot GLBs or check fonts (find them with grep: `azu3d`, `AZUGLB`, `models`, `font`), via `node run_tests.js --only=…`;
   - `powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1 -tabs guide`;
   - at most 2 more files that you name.

   Take captures of the phone (card and guide dock, light and dark) and the Editor before and after, and check they look identical.

Done when the build passes with the lint ratchet, the named tests are green, the page is smaller, and the captures are identical.

## Final report (both)
Changes per file, one line each; test results; captures (paths); anything left open.
