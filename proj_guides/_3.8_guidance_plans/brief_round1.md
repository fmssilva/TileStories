You are building round 1 of the guidance system in a browser prototype, the "Block Components Lab", in
`C:\Users\franc\Desktop\TileStories\proj_guides\_3.3.1_Prototype\lab_src\`. The Lab is the design reference for a Unity 6.3 app;
every feature must be buildable in Unity (name its path).

READ FIRST (all lines): `../../_3.8_Guidance_Study.md` (the decided design: this round is section 8, round 1; the decisions are
section 9); `PROTOTYPE_STRUCTURE.md` (rules, modules, "Where a change goes", the row-demo contract); `README.md`;
`../../skills/Feature_Prototyping.md` step 5 (the rules: one home per feature, extension points not wrappers, every feature in
the Editor, every row's demo); `src/core/ext.js`, `src/editor/shell.js`, `src/editor/demo.js`, `src/editor/focus.js`,
`src/world/wall.js` (the wall hint pill), `src/world/guide-button.js` (Azu beside the ✦, its remarks), `src/world/camera-ask.js`,
`src/guide/dock.js` (`besideWords`, `pointAt`), `src/mascot/models.js` (`mascotStill`), `src/agent/scenes.js` (the console's
scenes). The spec `../../_3.3_POI_Card_Styles.md` section 11 (Guide) and section 14 (Global Scene) — read; do not edit (the
Architect does).

## This round
1. **The Guide tab (the developer's decision).** A new top-level Editor tab **Guide**, after Detail Card. Today's
   Detail Card > 6 · Guide moves into it whole: its foldouts become the tab's sections, grouped and numbered for a clear read
   (e.g. 0 · Preview & Test, 1 · The guide (Narration, Voice test, Guide panel, Guide button, Captions, Focus, Invite to try,
   Resume, Hero button, Guide block), 2 · Mascot, 3 · Ask (Ask panel, Ask about the view, Azu comments, Ask Azu),
   4 · Guidance, 5 · First pages (an empty placeholder with one line: "Built in a later round" is NOT allowed — omit it until
   it exists)). Choose the exact grouping; keep every row, its help, its demo and its words' owner. 7 · Audiences stays in the
   Detail Card. Each section's focus (`FOCUS`) and the row demos keep working; the self-test walks the new tab like the others.
   Tests that open 6 · Guide's foldouts must be updated to the new tab — the only allowed test change for this part; list
   each.
2. **Guidance, the first slice** (new modules under `src/guidance/`):
   - `catalog.js`: the data model of study section 3.2 (kinds, groups, items; only the fields this round uses, but in the
     study's shape so later rounds add fields, not rewrite): kind App, group **The start** (In order), items **Opening
     message** (Essential, once a visit, "visit started") and **Tap a marker** (Teaches and Done when "opened a card"; the
     nearest marker; Shows Until done at most 3 a visit; Again after 25 s; at least 20 s after the opening; phone still 1.2 s).
     Words in Languages & Terms with an owner (the wall hint's key `wall_tip` may be reused for Tap a marker); per language,
     per the study's lines; a returning-visitor line for the opening.
   - `scheduler.js`: a pure `pick(items, visitor, context, now) → {item, why} | {none, why}` with the gates and limits this
     round needs: one at a time (counting Azu's existing wall remarks and any bubble), never over a sentence, quiet while the
     wall is being dragged / zoomed (the Lab's "phone moving") and for 0.7 s after, a panel open, the breathing room (15 s);
     the item's Shows, Again after, Done when, Essential. Deterministic (a fake clock in tests). The memory per item and
     visitor (this visit / ever) in the page's state, with **Reset the visitor**.
   - `present.js`: the bubble (the dock's material, 7:1 text, at most 2 lines, a tail to the mascot), the mascot (Azu beside the
     ✦, which is the explorer's picture by default) going beside the nearest marker and pointing, a ring on the marker
     (one soft pulse, then still); Reduce Motion: no travel, a still ring; **Mascot Off** (Guide button = ✦ only, or the
     mascot off): a plain bubble at the marker. App items are a bubble only (not spoken), per the decision.
   - The events this round needs: visit started, a marker in view, the phone still / moving (the wall's drag), card opened.
3. **The wall hint pill goes.** Remove it (`world/wall.js`); Global Scene > Wall hint becomes one link row ("Said by the
   guide: Guide › Guidance › The start", which opens it). The scene that shows the wall with no card opened now shows the
   opening message, then Tap a marker.
4. **The Editor's Guidance section** (in the Guide tab): 0 · Preview & Test gets **Show next now** (what `pick` would choose
   and why) and **Reset the visitor**; 1 · Overall: **Guidance** On / Off, **Breathing room** (5-60 s, 15); 2 · App ›
   The start: each item as a row (On, Label, importance dots, a short how-often line, ▸ Show now) that opens its fields:
   Says (per language), Shows, Again after, Done when, Essential, and the read-back line in words (study 3.2). Every row has
   its demo (`rowDemos`): an item row puts the phone on the wall and shows the item now; Again after / Shows show it twice
   with a ×10 countdown chip. The self-test fails a row of the Guide tab without a demo.
5. **Scenes in Ask Azu's console** the developer can press (their rule): "First visit on the wall" (the opening, then Tap a
   marker by the nearest marker), "Ignore it" (again after 25 s, at most 3), "Open a card" (never again, also after Reset of
   the visit but not of the visitor), "Panning the wall" (nothing while it moves), "Mascot Off" (a plain bubble).

## Design pass first (look, then tests)
Captures at 390 px, light and dark: the wall at the start (the opening bubble), Tap a marker (the mascot beside the nearest
marker, the ring), Mascot Off, the Guide tab's section list, the Guidance rows and an item opened. Open each yourself; judge
one focal point, nothing covering the marker or other words (measure: 0 px over text), contrast (text 4.5:1 or more, the ring
3:1 against the wall picture), 44 px targets, the bubble not under the ✦ or the nav bar. Fix and recapture. Save the final
captures in `C:\Users\franc\AppData\Local\Temp\claude\40339de5183b80ad\6cb9f8d2-d763-422a-8d55-d71491f4db7c\scratchpad\g1\`.

## Tests
`tests/test9e.js` (Playwright, the installed Edge): the opening once a visit; Tap a marker only after the phone is still 1.2 s
and ≥ 20 s after the opening; never while panning; again after 25 s, at most 3 a visit; never after a card opens (this visit
and, unless the visitor is reset, the next); never at the same time as a remark of Azu; Mascot Off; Reduce Motion; the pill is
gone; the Guide tab holds every former 6 · Guide row (count them before and after: the same rows); every Guidance row's demo;
the console scenes. A pure check of `pick` on a fake clock (each rule alone). A mutation proof (e.g. `pick` ignoring Done when
turns named checks red), then restore.

## Proof
`python build.py` (the check 0/0/0/0, no version names); `node run_tests.js --only=test9e` then the guide and wall group
(`test8o,test8p,test8s,test8u,test8v,test9d`); the self-test (`powershell -ExecutionPolicy Bypass -File tests/runtest8.ps1`, about
40 min, alone, run in the background and wait): `bad=0` (say how ok / behaviour moved and why); `node run_tests.js` (about 35 min):
all green (a start crash "page.waitForFunction: Timeout 20000ms" under load is known flakiness: rerun alone). Update
`tests_baseline.json`, `PROTOTYPE_STRUCTURE.md` (the guidance modules, the Guide tab, "Where a change goes") and `README.md`
(test9e).

## Limits
Edit only files under lab_src. Do not commit, do not publish. No keys, voicebench, mascot model files or Unity. No CSS-only
effects (name each one's Unity path in the report). Patch scripts with the Write tool (bash heredocs mangle apostrophes). If a
usage limit stops you, your work is saved; the Architect resumes you.

## Report (your final message)
What was built, module by module, with Unity paths; the Guide tab's sections (old → new place for every former 6 · Guide
foldout); the Guidance rows and defaults; the captures (paths) and what you judged and fixed; the tests, the mutation, the tests
you changed and why; the self-test line; the run_tests summary; 3-5 specific questions for the developer about the look and the
flow.
