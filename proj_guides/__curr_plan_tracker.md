# Plan tracker: merge P1b.1 (the theme on Unity's tokens: the table, its test, the looks, one dark mode)

Brief: `__AI_worker.md` "And now". Lab: `proj_guides/_4.2_TileStories_Lab/lab_src`. GATE: Lab. No git writes.
Spec: `lab_src/THEME_MAP.md` (its §5 recommendations accepted; §1.1 Unity's values win).

## Decisions
- `core/theme.js` (new, after `core/undo.js`): `D.theme` in the ThemeAsset's shape (`key`, `defaultMode`, `shared`, `modes[]`,
  `knobs[]`, `type`), tokens as `{name, value[, proposed:true]}`, colours `#RRGGBB` or `#RRGGBB@a`. `applyTokens()` writes one
  `<style id="ts-theme">`: a rule per mode (`.ts-app.ts-theme-default.ts-mode-<m>`), the font rule, then a rule per knob option
  (`.ts-<knob>-<option>` shared, `+ .ts-mode-<m>` per mode). Regenerated on `configChanged`. `themeMode()` = ThemeModeRule
  (visitor `P.vis.mode` > the wall's `D.wall.theme.mode` (follow = the phone's appearance) > `D.theme.defaultMode`).
  `themeClasses()` / `themeSwap()` = ThemeController.Apply / SetMode: one class per knob and the mode on every `.ts-app` root.
- The wall's picks: `D.wall.theme = {mode, accent, look, roundness, density, glass, motion}` (WallConfigData, guess): the
  ThemeAsset keeps the theme's own `defaultMode`, as ThemeModeRule's three layers.
- A look picks named options (`picks`: Playful -> accent cobalt, roundness round, glass solid); a look's pick wins over the
  wall's for that knob (read-time, no write). Knob options: look premium/playful, accent 4 + cobalt, roundness sharp/soft/round,
  density comfortable/compact, glass blur/solid, motion follow/reduced. New knobs and options flagged `proposed`.
- `P.dark` goes (computed, not stored); `.cv.dark`, `#ph.dark`, `.dark` rules key on `.ts-mode-dark`. ACCENTS / v8vars,
  the card's Default mode row, `D.card.cardCfg.mode/accent/round`, `D.card.motionLevel`, `D.guide.pSkin` light/dark go.
  The panels' dark style stays a guide option (`D.guide.darkPanels`: Aurora / Ink glass).
- The card's table renamed by a script: paper->sheet-open, ink1->ink, ink2->ink-2 (not in the dock's own scope), hair->hair,
  box->surface-2, accent2->accent, the section colours->family-*, bar->bar (card only); `--fam` stays a per-element parameter.
- Ratchets (`tools/theme_ratchet.py`, run by build.py, `tools/theme_ratchet.json`): colour literals per file outside
  `core/theme.js` and the Editor (`src/editor/`, LAB); phone font sizes under 13 px per file. Counts only go down.
- Editor: a new **App** tab (`editor/app-tab.js`, W's tab name; walked by the self-test): Look and mode (the wall's default
  mode, accent swatches, look), Depth and motion (roundness, density, glass, motion level), and a developer-only Free colours
  fold (LAB ONLY; picker + hex + live contrast badge in linear light).
- Fredoka embedded (`media/FONT_FREDOKA.txt`, W's) for Playful's display face.
- Tests: `test10g` (Node, the token test), `test10h` (browser: mode/look by real clicks, one class, the Editor accent/look +
  Undo); both MUTATE=1; both join the core set.

## Steps
1. [x] Before captures (HEAD) -- scratch `p1b1/before`.
2. [x] `core/theme.js`: the table (generated once from the .uss + asset, plus the PROPOSED list), applyTokens, themeMode,
   classes, contrast helpers; Fredoka; MODULES.
3. [x] One dark mode: P.dark out, the class on every root, dark selectors renamed, per-module settings out, preview bar /
   Settings / panel skins on the resolver.
4. [x] The card's table renamed to `var(--ts-*)` (script), the card's own table and ACCENTS gone; font sizes touched -> type tokens.
5. [x] Ratchets in build.py; build 0.
6. [x] App tab: rows + demos + undo; free colours dev-only with the badge; self-test walks it.
7. [x] Look: the matrix (Premium/Playful x light/dark x text default/L, 4 devices) over the states; Editor section; fixes.
8. [x] Tests: test10g, test10h (+ MUTATE=1); the tests that set P.dark / cardCfg.mode updated.
9. [x] CORE: runtest8.ps1 (every walk bad=0), `node run_tests.js --core`.
10. [x] Docs: PROTOTYPE_STRUCTURE, README, STATE_MAP, THEME_MAP rows.
